// using System;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using GeneralPreview;
// using static GeneralPreview.MyPrelude;
//
// namespace NM.ZZZTest;
//
// public class Operator : MonoBehaviour
// {
//     [Button]
//     public void Test()
//     {
//         MyOption<int> none = new MySome<int>(42);
//         none = None;
//         var none1 = none | 42;
//         var f = (int x) => x.ToString();
//         var g = (string x) => string.Join(" ", x.ToCharArray()) + "aaa";
//         var ret = g * f >> 114514;
//         Console.WriteLine(ret);
//         var f2 = (int x, int y) => x + y;
//         var f2_42 = f2 >> 42;
//         var f2_42_42 = f2 >> (42, 42);
//         Console.WriteLine(f2_42_42);
//     }
// }