// using System;
// using System.Collections.Generic;
// using System.Reflection;
//      
// public static class MyEvent
// {
//     private static Dictionary<Type, Delegate> eventHandlers = new Dictionary<Type, Delegate>();
//
//     public static void ClearAll()
//     {
//         eventHandlers = new();
//     }
//     // 添加事件监听
//     public static void AddListener<T>(Action<T> handler)
//     {
//         var type = typeof(T);
//         
//         if (!eventHandlers.ContainsKey(type))
//         {
//             eventHandlers[type] = handler;
//         }
//         else
//         {
//             eventHandlers[type] = Delegate.Combine(eventHandlers[type], handler);
//         }
//     }
//
//     // 移除事件监听
//     public static void RemoveListener<T>(Action<T> handler)
//     {
//         var type = typeof(T);
//         
//         if (eventHandlers.ContainsKey(type))
//         {
//             eventHandlers[type] = Delegate.Remove(eventHandlers[type], handler);
//             
//             if (eventHandlers[type] == null)
//             {
//                 eventHandlers.Remove(type);
//             }
//         }
//     }
//
//     // 触发事件
//     public static void Fire<T>(T eventData)
//     {
//         var type = typeof(T);
//         
//         if (eventHandlers.TryGetValue(type, out var handler))
//         {
//             (handler as Action<T>)?.Invoke(eventData);
//         }
//     }
//
//     
//     
// }
