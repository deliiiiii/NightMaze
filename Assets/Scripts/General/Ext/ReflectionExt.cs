// using System;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace General
// {
//     public static class ReflectionExt
//     {
//         public static bool HasAttribute<T>(this Type type) where T : Attribute
//         {
//             return type.GetCustomAttributes(typeof(T), true).Length > 0;
//         }
//     }
// }