using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.Utilities;

namespace GeneralPreview;

internal static class ActFactory
{
     static ActFactory()
     {
          UnityEditor.EditorApplication.playModeStateChanged += state =>
          {
               if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
               {
                    objMethodDic.Clear();
               }
          };
     }
     static readonly Dictionary<object, Dictionary<string, MethodInfo>> objMethodDic = [];

     internal static MyOption<string> GetMethodDes(object obj, string methodName)
     {
          if(!objMethodDic.TryGetValue(obj, out var methodDic))
          {
               MyDebug.LogError($"ActFactory GetMethodDes Failed Because Target {obj} Not Registered");
               return "None";
          }
          if (!methodDic.TryGetValue(methodName, out var methodInfo))
          {
               MyDebug.LogError($"ActFactory GetMethodDes Failed Because Target {obj} Not Found Method {methodName}");
               return "None";
          }
          var attr = methodInfo.GetCustomAttribute<UniActAttribute>();
          if (attr == null)
               return None;
          return attr.Des;
     }
     internal static void RegMethod(object obj)
     {
          objMethodDic.Remove(obj);
          objMethodDic.Add(obj, []);
          var methodDic = objMethodDic[obj];
          obj.GetType()
               .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
               .ForEach(info =>
               {
                    var attr = info.GetCustomAttribute<UniActAttribute>();
                    if (attr == null)
                    {
                         return;
                    }
                    methodDic.Add(attr.Des, info);
               });
     }

     internal static UniTask InvokeAsync(UniActWrap wrap, CancellationToken ct)
     {
          if(!objMethodDic.TryGetValue(wrap.Ctx, out var methodDic))
          {
               MyDebug.LogError($"ActFactory Invoke Failed Because Target {wrap.Ctx} Not Registered");
               return UniTask.CompletedTask;
          }
          if (!methodDic.TryGetValue(wrap.MethodDes, out var methodInfo))
          {
               MyDebug.LogError($"ActFactory Invoke Failed Because Target {wrap.Ctx} Not Found Method {wrap.MethodDes}");
               return UniTask.CompletedTask;
          }
          var result = methodInfo.Invoke(wrap.Ctx, [wrap.ParamArr, ct]);
          if (result is UniTask task)
          {
               return task;
          }
          return UniTask.CompletedTask;
     }

     extension<T>(List<T> self) where T : IDisposable
     {
          public void Test()
          {
               self.ForEach(s => s.Dispose());
          }
     }
}