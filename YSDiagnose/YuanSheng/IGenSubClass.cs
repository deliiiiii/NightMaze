using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace YuanSheng;

[Generator]
// ReSharper disable once InconsistentNaming
public class IGenSubClassGenerator : IIncrementalGenerator
{
    public record struct ClassHierarchyInfo(string BaseClassName, string DerivedClassName);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { BaseList: not null },
                transform: static (ctx, _) => GetClassHierarchyInfo(ctx))
            .Where(static m => !m.IsDefaultOrEmpty);

        var compilationAndClasses = context.CompilationProvider.Combine(classProvider.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute([..source.Right.SelectMany(x => x)], spc));
    }

    public static ImmutableArray<ClassHierarchyInfo> GetClassHierarchyInfo(GeneratorSyntaxContext context)
    {
        // 1. 基本筛选：必须是类声明
        if (context.Node is not ClassDeclarationSyntax classDecl)
        {
            return ImmutableArray<ClassHierarchyInfo>.Empty;
        }

        // 2. 获取符号
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
        if (symbol == null)
        {
            return ImmutableArray<ClassHierarchyInfo>.Empty;
        }

        // 3. 需求明确：排除抽象类 (DataBase<> 等泛型基类通常是抽象的)
        if (symbol.IsAbstract)
        {
            return ImmutableArray<ClassHierarchyInfo>.Empty;
        }

        // 4. 获取 IGenSubClass 接口符号
        // 必须使用完全限定名，它是判断的基准
        var targetInterface = context.SemanticModel.Compilation.GetTypeByMetadataName("GeneralPreview.IGenSubClass");
        if (targetInterface == null)
        {
            // 如果无法解析接口（如引用丢失），则中止
            return ImmutableArray<ClassHierarchyInfo>.Empty;
        }

        // 5. 核心判断逻辑
        // 由于存在 DataBase<T>.ICom 这种泛型嵌套接口，使用递归检查比 AllInterfaces 更稳健
        if (IsInheriting(symbol, targetInterface))
        {
            // 6. 构造结果
            // 假设 ClassHierarchyInfo 接受 INamedTypeSymbol
            return
            [
                new ClassHierarchyInfo(
                    BaseClassName: symbol.BaseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining)),
                    DerivedClassName: symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining)
                    ))
            ];
        }

        return ImmutableArray<ClassHierarchyInfo>.Empty;
    }

    /// <summary>
    /// 递归检查类型是否实现了目标接口，能够穿透复杂的泛型层级
    /// </summary>
    static bool IsInheriting(INamedTypeSymbol? type, INamedTypeSymbol targetInterface)
    {
        // 递归终止条件
        if (type == null) return false;

        // 检查直接实现的接口
        foreach (var i in type.Interfaces)
        {
            // 使用 SymbolEqualityComparer.Default 来处理引用等价性
            // 即使是泛型类中的接口（如 DataBase<DataUnit>.ICom），它继承的 IGenSubClass 
            // 在 Symbol 层面应该与我们查找到的 targetInterface 相等。
            if (SymbolEqualityComparer.Default.Equals(i, targetInterface)) 
                return true;

            // 递归检查接口的父接口 (例如: ICom : IGenSubClass)
            if (IsInheriting(i, targetInterface)) 
                return true;
        }

        // 检查基类 (例如: GamePlaying -> GameRoot.Com<GamePlaying>)
        // 这里会自动处理泛型基类，因为 type.BaseType 会返回构造好的具体类型
        if (type.BaseType != null)
        {
            if (IsInheriting(type.BaseType, targetInterface)) 
                return true;
        }

        return false;
    }
    static void Execute(ImmutableArray<ClassHierarchyInfo> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        // 按基类进行分组 (BaseClass -> [SubClass1, SubClass2])
        var groupedClasses = classes
            .GroupBy(c => c.BaseClassName)
            .ToDictionary(g => g.Key, g => g.Select(c => c.DerivedClassName).Distinct().ToList());
        
        StringBuilder sb = new StringBuilder();
        foreach (var group in groupedClasses)
        {
            sb.AppendLine($"\t\t\tcase {{ }} t when t == typeof({group.Key}):");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\treturn");
            sb.AppendLine("\t\t\t\t[");
            foreach (var derived in group.Value)
            {
                sb.AppendLine($"\t\t\t\t\ttypeof({derived}),");
            }
            sb.AppendLine("\t\t\t\t];");
            sb.AppendLine("\t\t\t}");
        }
        string source = $$"""
                          using System;
                          using System.Collections.Generic;
                          namespace GeneralPreview;

                          public static partial class SubClass
                          {
                              public static List<Type> GetList<T>() where T : IGenSubClass
                              {
                                  List<Type> ret = [];
                                  switch (typeof(T))
                                  {
                          {{sb}}
                                  }
                                  return ret;
                              }
                          }
                          """;

        context.AddSource("SubClass.g.cs", source);
    }
}


// public static partial class SubClass
// {
//     public static List<Type> GetList<T>() where T : IGenSubClass
//     {
//         switch (typeof(T))
//         {
//             case { } t when t == typeof(global::GeneralPreview.DataBase<global::GeneralPreview.DataUnit>):
//             {
//                 return
//                 [
//                     typeof(global::GeneralPreview.DataUnit),
//                 ];
//             }
//             case { } t when t == typeof(global::GeneralPreview.BaseClass1):
//             {
//                 return
//                 [
//                     typeof(global::GeneralPreview.SubClass11),
//                     typeof(global::GeneralPreview.SubClass12),
//                     typeof(global::GeneralPreview.SubClass13),
//                 ];
//             }
//         }
//         return [];
//     }
// }

// public abstract class DataBase<TThis> : DataBase<DataUnit>.ICom
//     where TThis : DataBase<TThis>
// {
//     // ...
//     public interface ICom : IGenSubClass{...}
//     public abstract class ComBase<TSub> : ICom;
// }
//
// public class TestData : DataBase<TestData>;
//
// public class TestClass : TestData.ComBase<TestClass>;