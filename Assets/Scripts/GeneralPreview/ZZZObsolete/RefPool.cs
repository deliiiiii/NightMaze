// using System;
// using System.Collections.Generic;
// using System.Linq;
// using General;
//
// namespace GeneralPreview;
//
// public interface IRefSingle;
// public interface IRefMulti;
//
// public static class RefPoolSingle<T> where T : class, IRefSingle
// {
//     static readonly Dictionary<Type, T> oneDataDic = [];
//     public static T Register(Func<T> ctor)
//     {
//         var type = typeof(T);
//         if (!oneDataDic.ContainsKey(type))
//             return oneDataDic[type] = ctor();
//         MyDebug.LogError("DataBase: RegisterOne<" + typeof(T).Name + "> Duplicated");
//         return oneDataDic[type];
//     }
//     public static T Acquire()
//     {
//         var type = typeof(T);
//         if (oneDataDic.TryGetValue(type, out var data))
//         {
//             return data;
//         }
//         MyDebug.LogError("DataBase: AcquireOne<" + typeof(T).Name + "> Not Exist, Auto Register");
//         return null!;
//     }
//     public static void Release()
//     {
//         var type = typeof(T);
//         if (oneDataDic.Remove(type))
//         {
//             // toRemove = null!;
//             return;
//         }
//         MyDebug.LogError("DataBase: ReleaseOne<" + typeof(T).Name + "> Not Exist");
//     }
// }
//
// public static class RefPoolMulti<T> where T : class, IRefMulti
// {
//     static readonly Dictionary<Type, List<T>> listDataDic = [];
//     public static T RegisterOne(Func<T> ctor)
//     {
//         RegisterSome(ctor, 1, out var added, out _);
//         return added.First();
//     }
//     public static void RegisterSome(Func<T> ctor, int duplicateCount)
//         => RegisterSome(ctor, duplicateCount, out _, out _);
//     public static void RegisterSome(Func<T> ctor, int duplicateCount, out List<T> addedList, out List<T> allInList)
//     {
//         var type = typeof(T);
//         if (!listDataDic.ContainsKey(type))
//         {
//             listDataDic[type] = [];
//         }
//         addedList = [];
//         for (var i = 0; i < duplicateCount; i++)
//         {
//             var added = ctor();
//             listDataDic[type].Add(added);
//             addedList.Add(added);
//         }
//         allInList = listDataDic[type].ToList();
//     }
//     public static List<T> AcquireAll() => listDataDic.Where(p => p.Key.IsAssignableFrom(typeof(T))).SelectMany(p => p.Value).ToList();
//     public static T? AcquireOne(Func<T, bool> pred) => AcquireAll().FirstOrDefault(pred);
//     public static MyOption<T> AcquireOneOptional(Func<T, bool> pred) => AcquireAll().FirstOptional(pred);
//     public static T? AcquireFirst() => AcquireAll().FirstOrDefault();
//
//     public static void ReleaseOne(T toRemove)
//     {
//         var type = typeof(T);
//         if (listDataDic.TryGetValue(type, out var data)) 
//         {
//             if (data.Remove(toRemove))
//             {
//                 // toRemove = null!;
//                 return;
//             }
//             MyDebug.LogError("DataBase: ReleaseListOne<" + typeof(T).Name + "> Not Exist In List");
//             return;
//         }
//         MyDebug.LogError("DataBase: ReleaseListOne<" + typeof(T).Name + "> Not Exist");
//     }
//     
//     public static void ReleaseSome(List<T> toRemove)
//     {
//         var type = typeof(T);
//         if (listDataDic.TryGetValue(type, out var data)) 
//         {
//             toRemove.ForEach(r =>
//             {
//                 if (!data.Remove(r))
//                     MyDebug.LogError("DataBase: ReleaseListSome<" + typeof(T).Name + "> Not Exist In List");
//             });
//             // toRemove = null!;
//             return;
//         }
//         MyDebug.LogError("DataBase: ReleaseListSome<" + typeof(T).Name + "> Not Exist");
//     }
//     
//     public static void ReleaseAll()
//     {
//         var type = typeof(T);
//         if (listDataDic.Remove(type))
//             return;
//         MyDebug.LogError("DataBase: ReleaseListAll<" + typeof(T).Name + "> Not Exist");
//     }
// }