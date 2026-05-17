using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace YSDiagnose;


[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StaticAbstractCodeFixProvider)), Shared]
public class StaticAbstractCodeFixProvider : CodeFixProvider
{
    // 绑定你需要修复的诊断 ID
    public override ImmutableArray<string> FixableDiagnosticIds => ["SA003"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // 推荐直接使用 context.Span，因为当前上下文的所有 Diagnostics 都共享该 Span
        var diagnosticSpan = context.Span;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        
        var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (declaration == null) return;

        // 创建修复动作
        var action = CodeAction.Create(
            title: "实现缺失的静态抽象函数",
            createChangedDocument: c => ImplementMissingMethodsAsync(context.Document, declaration, c),
            // EquivalenceKey 必须提供一个唯一标识，它是 IDE 合并修复和实现 FixAll (批量修复) 的依据
            equivalenceKey: nameof(StaticAbstractCodeFixProvider));

        // 核心修改点：传入 context.Diagnostics 集合，而非 context.Diagnostics.First()
        context.RegisterCodeFix(action, context.Diagnostics);
    }

    async Task<Document> ImplementMissingMethodsAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (semanticModel == null) return document;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, cancellationToken);
        if (classSymbol == null) return document;

        var missingMethods = new System.Collections.Generic.List<IMethodSymbol>();

        // 1. 寻找所有带有 [StaticAbstract] 且当前类未实现的接口函数
        foreach (var interfaceSymbol in classSymbol.AllInterfaces)
        {
            // 注意：特性存在于泛型接口的原始定义上
            var interfaceMethods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.OriginalDefinition.GetAttributes().Any(a => a.AttributeClass?.Name == "StaticAbstractAttribute"));

            foreach (var method in interfaceMethods)
            {
                bool hasMatchingMethod = classSymbol.GetMembers().OfType<IMethodSymbol>().Any(m =>
                    m.Name == method.Name &&
                    m.IsStatic &&
                    m.DeclaredAccessibility == Accessibility.Public &&
                    m.Parameters.Length == method.Parameters.Length &&
                    SymbolEqualityComparer.Default.Equals(m.ReturnType, method.ReturnType) &&
                    m.Parameters.Select(p => p.Type).SequenceEqual(method.Parameters.Select(p => p.Type), SymbolEqualityComparer.Default));

                if (!hasMatchingMethod)
                {
                    missingMethods.Add(method);
                }
            }
        }

        if (missingMethods.Count == 0) return document;

        var newClassDecl = classDecl;

        // 2. 为每个缺失的函数生成 public static 的语法节点
        foreach (var method in missingMethods)
        {
            // 获取在当前上下文中被泛型替换过后的最简类型字符串（例如直接变成 TestIns2 而不是 T）
            var returnType = SyntaxFactory.ParseTypeName(method.ReturnType.ToMinimalDisplayString(semanticModel, classDecl.SpanStart));
            
            var parameters = method.Parameters.Select(p => 
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                    .WithType(SyntaxFactory.ParseTypeName(p.Type.ToMinimalDisplayString(semanticModel, classDecl.SpanStart))));

            var parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));

            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, method.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .WithParameterList(parameterList)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("throw new System.NotImplementedException();")
                ))
                .WithAdditionalAnnotations(Formatter.Annotation); // 添加格式化注解，确保生成的代码缩进正确

            newClassDecl = newClassDecl.AddMembers(methodDecl);
        }

        // 3. 替换语法树并返回新文档
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = root!.ReplaceNode(classDecl, newClassDecl);

        return document.WithSyntaxRoot(newRoot);
    }
}