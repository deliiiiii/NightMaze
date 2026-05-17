using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace YuanSheng;

[Generator]
public class UniActionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => s is MethodDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: (ctx, _) => AnalyzeMethod(ctx))
            .Where(m => m != null);

        context.RegisterSourceOutput(pipeline, Generate);
    }

    class MethodInfo
    {
        public string FileNameHint { get; set; } = "";
        public string SourceCode { get; set; } = "";
        public List<Diagnostic> Diagnostics { get; set; } = new();
    }

        static MethodInfo? AnalyzeMethod(GeneratorSyntaxContext context)
    {
        var methodDecl = (MethodDeclarationSyntax)context.Node;
        var model = context.SemanticModel;
        var symbol = model.GetDeclaredSymbol(methodDecl);

        if (symbol is null) return null;
        var containingType = symbol.ContainingType;

        // ==================== 0. 检查容器特性 [ActContainer] ====================
        bool hasActContainer = containingType.GetAttributes().Any(a =>
            a.AttributeClass?.Name is "ActContainer" or "ActContainerAttribute");

        if (!hasActContainer) return null;

        // 1. 检查 Obsolete (作为生成的标记)
        var obsoleteAttr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == nameof(ObsoleteAttribute));
        if (obsoleteAttr == null) return null; // 忽略非目标函数

        var info = new MethodInfo
        {
            FileNameHint = $"{containingType.Name}_{symbol.Name}"
        };

        // 验证逻辑
        bool hasError = false;

        // ==================== 文件名强校验 ====================
        string fileName = Path.GetFileName(methodDecl.SyntaxTree.FilePath);
        if (!fileName.StartsWith("Act"))
        {
            // 如果标记了 [ActContainer]，强制要求文件名开头为 Act
            info.Diagnostics.Add(CreateDiagnostic("SG001", $"类{containingType.Name}标记[ActContainer]后, 文件名开头必须为Act", methodDecl.GetLocation()));
            hasError = true;
        }

        if (symbol.DeclaredAccessibility != Accessibility.Private)
        {
            info.Diagnostics.Add(CreateDiagnostic("SG002", "欲自动生成UniAction, 函数必须是 private", methodDecl.GetLocation()));
            hasError = true;
        }

        if (symbol.ReturnType.Name != "UniTask")
        {
            info.Diagnostics.Add(CreateDiagnostic("SG003", "欲自动生成UniAction, 函数必须返回 UniTask", methodDecl.ReturnType.GetLocation()));
            hasError = true;
        }

        if (!symbol.Name.EndsWith("Async"))
        {
            info.Diagnostics.Add(CreateDiagnostic("SG004", "欲自动生成UniAction, 异步函数必须以 Async 结尾", methodDecl.Identifier.GetLocation()));
            hasError = true;
        }

        if ((symbol.Parameters.LastOrDefault()?.Type.Name ?? string.Empty) != "CancellationToken")
        {
            // 尝试定位最后一个参数，如果没有参数则定位括号
            var loc = methodDecl.ParameterList.Parameters.LastOrDefault()?.GetLocation() ?? methodDecl.ParameterList.GetLocation();
            info.Diagnostics.Add(CreateDiagnostic("SG005", "欲自动生成UniAction, 函数最后一个参数类型必须是 CancellationToken", loc));
            hasError = true;
        }

        // 禁止在函数内部调用其他被 [Obsolete] 标记的函数
        var invocations = methodDecl.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var result = model.GetSymbolInfo(invocation);
            var targetSymbol = result.Symbol;
            if (targetSymbol == null) continue;

            // 检查被调用的函数是否含有 Obsolete 特性
            var targetHasObsolete = targetSymbol.GetAttributes()
                .Any(a => a.AttributeClass?.Name == nameof(ObsoleteAttribute));
            if (targetHasObsolete)
            {
                // 报出自定义错误
                info.Diagnostics.Add(CreateDiagnostic(
                    "SG006",
                    $"欲自动生成UniAction的函数中, 禁止调用[Obsolete]函数 '{targetSymbol.Name}'. 应使用 (如 new Act{targetSymbol.Name.Replace("Async", "")}{{...}}).",
                    invocation.GetLocation()));

                hasError = true;
            }
        }

        if (hasError) return info; // 有错误只返回诊断信息

        // ==================== 代码生成准备 ====================

        string description = obsoleteAttr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "";
        string rawName = symbol.Name.Substring(0, symbol.Name.Length - "Async".Length);
        string recordName = $"Act{rawName}";
        string evtName = $"Evt{rawName}";

        // 参数处理
        var parameters = symbol.Parameters.Take(symbol.Parameters.Length - 1).ToList();
        var fieldDefs = new StringBuilder();
        var toStringParams = new List<string>();

        var callArgStrList = new List<string>();
        List<string> evtDefArgList = [$"{containingType.Name} WhoHasCt"];

        foreach (var param in parameters)
        {
            string propertyName = ToPascalCase(param.Name);
            string typeName = param.Type.ToDisplayString();

            fieldDefs.AppendLine($"\t\t[UnityEngine.HideInInspector]public required {typeName} {propertyName};");
            toStringParams.Add($"{propertyName} = {{{propertyName}}}");

            callArgStrList.Add(propertyName);
            evtDefArgList.Add($"{typeName} {propertyName}");
        }

        List<string> evtArgList = [];
        evtArgList.AddRange(callArgStrList);
        evtArgList.Insert(0, "Self");

        callArgStrList.Add("LinkedCts.Token");

        string namespaceName = containingType.ContainingNamespace.ToDisplayString();
        string className = containingType.Name;
        string toStringArgList = string.Join(", ", toStringParams);

        // ==================== 动态判断类型关键字 ====================
        string typeKeyword = containingType.IsRecord ? "record" : "class";
        if (containingType.IsValueType)
        {
            typeKeyword = containingType.IsRecord ? "record struct" : "struct";
        }

        string classAccess = GetAccessModifier(containingType.DeclaredAccessibility);

        bool muteEvt = symbol.GetAttributes().Any(a => a.AttributeClass?.Name is "MuteActEvt" or "MuteActEvtAttribute");
        string evtInvokeStr = muteEvt ? "" : $"\t\t\tawait new {evtName}({string.Join(", ", evtArgList)});";
        string evtClassStr = muteEvt ? "" : $"""
                                                [EvtName("{description}")][System.Diagnostics.DebuggerStepThrough]
                                                public record {evtName}({string.Join(", ", evtDefArgList)}) : EvtBase<{className}>(WhoHasCt);
                                             """;

        info.SourceCode = $@"// <auto-generated/>
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;
#pragma warning disable CS0618 // 类型或成员已过时
#pragma warning disable CS8669 //使用?应允许nullable


namespace {namespaceName};
{classAccess} partial {typeKeyword} {className}
{{
    [System.Diagnostics.DebuggerStepThrough]
    public record {recordName} : UniAction<{className}>
    {{
        [Newtonsoft.Json.JsonConstructor]
        public {recordName}({className} self): base(self){{}}
        [Sirenix.OdinInspector.ShowInInspector] string Des => ToString();
        public override string ToString() => $""Act {{Self.GetType().GetNiceName()}}.{description}{{{{{toStringArgList}}}}}"";
{fieldDefs}
        [System.Diagnostics.DebuggerStepThrough]
        protected override async UniTask InvokeAsync()
        {{
            await Self.{symbol.Name}({string.Join(", ", callArgStrList)});
            {evtInvokeStr}
        }}
    }}
{evtClassStr}
}}
";
        return info;
    }


    static void Generate(SourceProductionContext context, MethodInfo? info)
    {
        if (info == null) return;

        // 报告错误
        foreach (var diag in info.Diagnostics)
        {
            context.ReportDiagnostic(diag);
        }

        // 如果没有代码（因错误导致），则退出
        if (string.IsNullOrEmpty(info.SourceCode)) return;

        context.AddSource($"{info.FileNameHint}.g.cs", SourceText.From(info.SourceCode, Encoding.UTF8));
    }

    static Diagnostic CreateDiagnostic(string id, string message, Location location)
    {
        return Diagnostic.Create(new DiagnosticDescriptor(id, "UniAction Generator", message, "Gen", DiagnosticSeverity.Error, true), location);
    }

    static string GetAccessModifier(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "public"
        };
    }

    static string ToPascalCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    // ==================== 核心重写器 ====================
    // class BodyRewriter(SemanticModel model, IMethodSymbol methodSymbol, Dictionary<string, string> paramMap)
    //     : CSharpSyntaxRewriter
    // {
    //     public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    //     {
    //         // 获取符号信息
    //         var symbolInfo = model.GetSymbolInfo(node);
    //         var symbol = symbolInfo.Symbol;
    //
    //         if (symbol == null) return base.VisitIdentifierName(node);
    //
    //         // 1. 处理参数 -> 替换为 PascalCase 字段名
    //         if (symbol is IParameterSymbol paramSymbol &&
    //             SymbolEqualityComparer.Default.Equals(paramSymbol.ContainingSymbol, methodSymbol))
    //         {
    //             if (paramMap.TryGetValue(paramSymbol.Name, out string newName))
    //             {
    //                 // 确保不是 member access 的右边 (例如 this.add，虽然参数一般不会在右边，但防御性编程)
    //                 if (!(node.Parent is MemberAccessExpressionSyntax mas && mas.Name == node))
    //                 {
    //                     return SyntaxFactory.IdentifierName(newName).WithTriviaFrom(node);
    //                 }
    //             }
    //         }
    //
    //         // 2. 处理隐式实例成员访问 -> 添加 @this.
    //         // 条件：符号是 ContainingType 的成员，且不是静态的，且当前引用不是 MemberAccess 的右侧
    //         if (IsInstanceMember(symbol))
    //         {
    //             // 检查是否已经是显式调用 (例如 this.Coin)
    //             if (node.Parent is MemberAccessExpressionSyntax parentAccess && parentAccess.Name == node)
    //             {
    //                 // 如果父节点是 this.Coin，在 VisitMemberAccessExpression 里处理
    //                 return base.VisitIdentifierName(node);
    //             }
    //
    //             // 这是一个隐式引用 (例如 Coin += 1)
    //             // 修改关键点：使用 node.WithoutTrivia()，防止成员名自带的注释出现在点号后面
    //             return SyntaxFactory.MemberAccessExpression(
    //                 SyntaxKind.SimpleMemberAccessExpression,
    //                 SyntaxFactory.IdentifierName("@this"),
    //                 node.WithoutTrivia()
    //             ).WithTriviaFrom(node);
    //         }
    //
    //         return base.VisitIdentifierName(node);
    //     }
    //
    //     public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    //     {
    //         // 处理 this.Coin -> @this.Coin
    //         if (node.Expression.IsKind(SyntaxKind.ThisExpression))
    //         {
    //             // 递归访问 Name 部分（处理可能的参数重命名，虽然成员名一般不等于参数名，但保持一致性）
    //             var newNameNode = Visit(node.Name) as SimpleNameSyntax ?? node.Name;
    //             
    //             return SyntaxFactory.MemberAccessExpression(
    //                 SyntaxKind.SimpleMemberAccessExpression,
    //                 SyntaxFactory.IdentifierName("@this"),
    //                 newNameNode
    //             ).WithTriviaFrom(node);
    //         }
    //
    //         return base.VisitMemberAccessExpression(node);
    //     }
    //
    //     public override SyntaxNode? VisitThisExpression(ThisExpressionSyntax node)
    //     {
    //         // 处理单独使用的 this (例如 Pass(this)) -> Pass(@this)
    //         return SyntaxFactory.IdentifierName("@this").WithTriviaFrom(node);
    //     }
    //
    //     bool IsInstanceMember(ISymbol symbol)
    //     {
    //         // 符号必须属于当前类
    //         if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingType, methodSymbol.ContainingType))
    //             return false;
    //
    //         // 必须不是静态的
    //         if (symbol.IsStatic) return false;
    //
    //         // 必须是字段、属性、事件或方法
    //         return symbol.Kind == SymbolKind.Field ||
    //                symbol.Kind == SymbolKind.Property ||
    //                symbol.Kind == SymbolKind.Event ||
    //                symbol.Kind == SymbolKind.Method;
    //     }
    // }
}