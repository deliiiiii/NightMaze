namespace YuanSheng;

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class StaticAbstractGenerator : IIncrementalGenerator
{
    static readonly DiagnosticDescriptor ruleNotStatic = new(
        id: "SA001",
        title: "必须为静态函数",
        messageFormat: "接口函数 '{0}' 标记了 [StaticAbstract], 必须声明为 static",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // static readonly DiagnosticDescriptor ruleNoGeneric = new(
    //     id: "SA002",
    //     title: "接口必须包含泛型参数",
    //     messageFormat: "接口 '{0}' 包含 [StaticAbstract] 函数, 必须至少包含一个泛型参数",
    //     category: "Design",
    //     defaultSeverity: DiagnosticSeverity.Error,
    //     isEnabledByDefault: true);

    static readonly DiagnosticDescriptor ruleImplMissing = new(
        id: "SA003",
        title: "未实现静态抽象函数",
        messageFormat: "类型 '{0}' 实现了接口 '{1}', 但未提供匹配的 public static {2} 函数",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor ruleNotExactlyOneGeneric = new(
        id: "SA004",
        title: "接口必须包含且仅包含一个泛型参数",
        messageFormat: "接口 '{0}' 包含 [StaticAbstract] 函数, 必须有且仅有一个泛型参数",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // static readonly DiagnosticDescriptor ruleGenericInParam = new(
    //     id: "SA005",
    //     title: "入参不能包含接口泛型参数",
    //     messageFormat: "接口函数 '{0}' 的入参不能使用接口的泛型参数 '{1}'",
    //     category: "Design",
    //     defaultSeverity: DiagnosticSeverity.Error,
    //     isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. 筛选带有 [StaticAbstract] 的方法声明
        var methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is MethodDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // 2. 收集所有具有基类的类型声明（寻找潜在的实现类）
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax { BaseList.Types.Count: > 0 } and not InterfaceDeclarationSyntax,
                transform: static (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
            .Where(static t => t is not null);

        var compilationAndData = context.CompilationProvider
            .Combine(methodDeclarations.Collect())
            .Combine(typeDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndData, static (spc, source) => Execute(spc, source.Left.Right!, source.Right!));
    }

    static IMethodSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var methodDecl = (MethodDeclarationSyntax)context.Node;
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl);
        if (methodSymbol is null) return null;

        var hasStaticAbstractAttr = methodSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name == "StaticAbstractAttribute");

        return hasStaticAbstractAttr ? methodSymbol : null;
    }

    static void Execute(SourceProductionContext context, ImmutableArray<IMethodSymbol> methods, ImmutableArray<INamedTypeSymbol> implementations)
    {
        if (methods.IsDefaultOrEmpty) return;

        // 按所属接口分组
        var interfaceGroups = methods.GroupBy(m => m.ContainingType, SymbolEqualityComparer.Default);

        foreach (var group in interfaceGroups)
        {
            var interfaceSymbol = (INamedTypeSymbol)group.Key!;

            // 校验：接口必须有且仅有一个泛型参数
            if (interfaceSymbol.TypeParameters.Length != 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(ruleNotExactlyOneGeneric, interfaceSymbol.Locations[0], interfaceSymbol.Name));
                continue;
            }

            // var typeParam = interfaceSymbol.TypeParameters[0];
            var interfaceMethods = group.ToImmutableArray();
            var validMethods = interfaceMethods.ToList();

            // 只要存在合法的函数，就放行至代码生成环节
            if (validMethods.Count == 0) continue;

            var targetImplementations = implementations
                .Where(t => t.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceSymbol.OriginalDefinition)))
                .ToList();

            GenerateExtensionCode(context, interfaceSymbol, validMethods, targetImplementations);
        }
    }

    static void GenerateExtensionCode(SourceProductionContext context, INamedTypeSymbol interfaceSymbol, List<IMethodSymbol> validMethods, List<INamedTypeSymbol> implementors)
    {
        var sb = new StringBuilder();
        var typeName = interfaceSymbol.Name;
        var extClassName = $"{typeName}Ext";
        string? namespaceName = interfaceSymbol.ContainingNamespace.IsGlobalNamespace ? null : interfaceSymbol.ContainingNamespace.ToDisplayString();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("using System;");
        sb.AppendLine("using General;");
        sb.AppendLine("using GeneralPreview;");
        sb.AppendLine("using Sirenix.Utilities;");
        sb.AppendLine();
        if(namespaceName != null)
            sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine($"public static class {extClassName}");
        sb.AppendLine("{");
        
        sb.AppendLine($"    extension<T>(Factory<T>) where T : {typeName}<T>");
        sb.AppendLine("    {");

        foreach (var method in validMethods)
        {
            var returnType = method.ReturnType.ToDisplayString();
            var methodName = method.Name;
            
            // 提取参数列表声明
            var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            // 提取供默认接口实现调用的原始参数名列表
            var fallbackArgs = string.Join(", ", method.Parameters.Select(p => p.Name));

            sb.AppendLine($"        public static {returnType} {methodName}({parameters})");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (typeof(T))");
            sb.AppendLine("            {");

            foreach (var impl in implementors)
            {
                // 1. 找到当前实现类具体实现了哪个泛型版本的接口
                var constructedInterface = impl.AllInterfaces.First(i => 
                    SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceSymbol.OriginalDefinition));

                // 2. 找到该具体接口中对应的被替换过泛型参数的函数符号
                var constructedMethod = constructedInterface.GetMembers().OfType<IMethodSymbol>()
                    .First(m => SymbolEqualityComparer.Default.Equals(m.OriginalDefinition, method.OriginalDefinition));

                // 3. 严格校验签名：名称、修饰符、参数数量、返回值类型、所有入参类型
                var hasMatchingMethod = impl.GetMembers().OfType<IMethodSymbol>().Any(m =>
                    m.Name == methodName &&
                    m.IsStatic &&
                    m.DeclaredAccessibility == Accessibility.Public &&
                    m.Parameters.Length == constructedMethod.Parameters.Length &&
                    SymbolEqualityComparer.Default.Equals(m.ReturnType, constructedMethod.ReturnType) &&
                    m.Parameters.Select(p => p.Type).SequenceEqual(constructedMethod.Parameters.Select(p => p.Type), SymbolEqualityComparer.Default));

                if (!hasMatchingMethod)
                {
                    context.ReportDiagnostic(Diagnostic.Create(ruleImplMissing, impl.Locations[0], impl.Name, interfaceSymbol.Name, methodName));
                    continue; // 发生错误仍继续检查其他类以抛出完整报错
                }

                // 4. 针对当前具体实现类，构建带类型强转的传参列表
                var specificArgsList = new List<string>();
                for (int i = 0; i < method.Parameters.Length; i++)
                {
                    var interfaceParam = method.Parameters[i];
                    var constructedParamType = constructedMethod.Parameters[i].Type;

                    // 对比原始接口的参数类型与替换后的参数类型是否一致
                    // 如果不一致，说明该参数类型中包含泛型参数，需要强转
                    if (!SymbolEqualityComparer.Default.Equals(interfaceParam.Type, constructedParamType))
                    {
                        var targetParamTypeName = constructedParamType.ToDisplayString();
                        specificArgsList.Add($"(({targetParamTypeName})(object){interfaceParam.Name})");
                    }
                    else
                    {
                        specificArgsList.Add(interfaceParam.Name);
                    }
                }
                string specificArgs = string.Join(", ", specificArgsList);

                sb.AppendLine($"                case var t when t == typeof({impl.ToDisplayString()}):");

                // 若返回值是T类型，强制转换为(T)(object)以绕过泛型约束检查
                sb.AppendLine($"                    return {(returnType == "T" ? "(T)(object)" : string.Empty)}{impl.ToDisplayString()}.{methodName}({specificArgs});");
            }

            sb.AppendLine("            }");
            sb.AppendLine($"            MyDebug.LogError($\"{{typeof(T).GetNiceName()}} 调用接口上的静态抽象函数:{methodName} 失败. 将返回接口提供的默认值.\");");
            // 这里使用 fallbackArgs，因为默认实现依然接收未被具体子类替换的原始泛型签名参数
            sb.AppendLine($"            return {typeName}<T>.{methodName}({fallbackArgs});");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{extClassName}.g.cs", sb.ToString());
    }
    // static bool ContainsTypeParameter(ITypeSymbol type, ITypeParameterSymbol targetTypeParam)
    // {
    //     // 直接匹配
    //     if (SymbolEqualityComparer.Default.Equals(type, targetTypeParam)) 
    //         return true;
    //     
    //     // 嵌套泛型（如 List<T>）
    //     if (type is INamedTypeSymbol { IsGenericType: true } namedType)
    //     {
    //         return namedType.TypeArguments.Any(t => ContainsTypeParameter(t, targetTypeParam));
    //     }
    //
    //     // 数组类型（如 T[]）
    //     if (type is IArrayTypeSymbol arrayType)
    //     {
    //         return ContainsTypeParameter(arrayType.ElementType, targetTypeParam);
    //     }
    //
    //     return false;
    // }
}