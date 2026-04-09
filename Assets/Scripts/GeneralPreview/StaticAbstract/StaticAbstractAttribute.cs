using System;
namespace GeneralPreview;
[AttributeUsage(AttributeTargets.Method)]
// ReSharper disable once UnusedType.Global
public class StaticAbstractAttribute : Attribute;
// ReSharper disable once UnusedType.Global
// ReSharper disable once UnusedTypeParameter
public static class Factory<T>;

// Example:
// public interface ITestInterface<out T>
// {
//     [StaticAbstract] static T Create() => default!;
//     [StaticAbstract] static int Create2() => 0;
//     [StaticAbstract] static T Create3(List<T> list) => default!;
// }
// public class TestIns : ITestInterface<TestIns>
// {
//     public int Int;
//     public override string ToString()
//     {
//         return $"TestIns {{ Int = {Int} }}";
//     }
//     // public static TestIns Create() => new();
//     // public static int Create2() => 42;
//     // public static TestIns Create3(List<TestIns> list) 
//         // => new() { Int = list.Sum(x => x.Int) };
// }
//
// public interface I2<T>;
// public class I2Ins<T> : I2<T>;
//
//
// class TestStaticAbstract
// {
//     [Button]
//     public static void CallTest() => Test<TestIns>();
//     [Button]
//     public static void CallTest3()
//     {
//         List<TestIns> l = [new (){Int = 1}, new (){Int = 2}, new (){Int = 3}];
//         Test3(l);
//     }
//
//     static void Test<T>() where T : ITestInterface<T>
//     {
//         var x = Factory<T>.Create();
//         var x2 = Factory<T>.Create2();
//         MyDebug.Log(x2);
//     }
//
//     static void Test3<T>(List<T> list) where T : ITestInterface<T>
//     {
//         var x3 = Factory<T>.Create3(list);
//         MyDebug.Log(x3);
//     }
// }