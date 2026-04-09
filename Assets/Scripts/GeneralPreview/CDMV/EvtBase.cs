using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;
[DebuggerStepThrough]
public static class Bus
{
    [HideInInspector]
    public static bool TryClear
    {
        get;
        set
        {
            field = value;
            if (evtDic.Any())
            {
                MyDebug.LogError("上次运行时注册的事件未清除，已自动清除。请检查是否报错，或有事件未正确注销...");
                evtDic.Clear();
            }
        }
    }
    [HideInInspector]
    static readonly Dictionary<Type, List<IUniEvt>> evtDic = [];

    [ShowInInspector]
    static Dictionary<string, List<string>> NonViewDic
        => evtDic
            .Where(pair => !pair.Key.Namespace?.Contains("View") ?? false)
            .ToDictionary(
                pair => pair.Key.GetNiceName(),
                pair => pair.Value.Select(dele => dele.Des).ToList()
            );

    internal static void FireAndForget<T>(T evt, bool debug = true) where T : IEvtBase
        => FireAsync(evt, CancellationToken.None, debug).Forget();
    [DebuggerStepThrough]
    internal static async UniTask FireAsync<T>(T evt, CancellationToken ct, bool debug = true) where T : IEvtBase
    {
        var evtType = evt.GetType();
        if (BusDisposable.IsMute(evtType.FullName))
            return;
        if (debug)
        {
            var attr = evtType.GetCustomAttribute<EvtNameAttribute>();
            var typeName = attr != null ? $"{attr.Name}" : evtType.GetNiceName();
            typeName = typeName.Replace("Node<TThis>.", string.Empty);
            if (typeName.StartsWith("Evt"))
                typeName = typeName[3..];
            var details = evt.ToString();
            details = details.Replace("OnEnter", "进入状态");
            details = details.Replace("OnExit", "退出状态");
            var leftBracketIndex = details.IndexOf('{');
            var rightBracketIndex = details.LastIndexOf('}');
            details = details.Substring(leftBracketIndex, rightBracketIndex - leftBracketIndex + 1);
            details = FormatRecordDetails(details);
            MyDebug.Log($"Fired - {typeName}{details}");
        }
        if (!evtDic.TryGetValue(evtType, out var list)) 
            return;
        foreach (var dele in list.Where(_ => !ct.IsCancellationRequested).ToList())
        {
            await dele.InvokeAsync(evt, ct);
        }
    }
    internal static void Register<T>(UniEvt<T> act) where T : IEvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    internal static void UnRegister<T>(UniEvt<T> func) where T : IEvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
            return;
        var index = list.FindIndex(h => (UniEvt<T>)h == func);
        if (index == -1) 
            return;
        list.RemoveAt(index);
        if (list.Count == 0)
        {
            evtDic.Remove(typeof(T));
        }
    }
    
    static string FormatRecordDetails(string text)
    {                                       
        var sb = new StringBuilder();
        int indent = 0;
        int inParentheses = 0; // 记录当前是否在小括号内
        string indentStr = "    ";
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '(')
            {
                inParentheses++;
                sb.Append(c);
            }
            else if (c == ')')
            {
                inParentheses = Math.Max(0, inParentheses - 1);
                sb.Append(c);
            }
            else if (c == '{')
            {
                sb.Append(c);
                sb.AppendLine();
                indent++;
                for (int j = 0; j < indent; j++) sb.Append(indentStr);
                if (i + 1 < text.Length && text[i + 1] == ' ') i++;
            }
            else if (c == '}')
            {
                sb.AppendLine();
                indent = Math.Max(0, indent - 1);
                for (int j = 0; j < indent; j++) sb.Append(indentStr);
                sb.Append(c);
            }
            // 只有当 inParentheses == 0 时，逗号才会引起换行
            else if (c == ',' && inParentheses == 0)
            {
                sb.Append(c);
                sb.AppendLine();
                for (int j = 0; j < indent; j++) sb.Append(indentStr);
                if (i + 1 < text.Length && text[i + 1] == ' ') i++;
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

}
[DebuggerStepThrough]
public abstract record EvtBase<THasCt>(THasCt WhoHasCt)
    : IEvtBase, ICanAwait
    where THasCt : IHasCt
{
    bool getDebug = true;
    public EvtBase<THasCt> Debug(bool debug) { getDebug = debug; return this; }
    [HideInInspector] public THasCt WhoHasCt = WhoHasCt;
    [ShowInInspector] string EvtDes => ToString();
    public UniTask.Awaiter GetAwaiter() 
        => WhoHasCt.CurCt.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : Bus.FireAsync(this, WhoHasCt.CurCt, getDebug).GetAwaiter();
}
[DebuggerStepThrough]
public abstract record EvtForgetBase : IEvtBase
{
    public void Forget(bool debug = true) => Bus.FireAndForget(this, debug);
}

public interface IHasCt
{
    CancellationToken CurCt { get; }
}
public interface IEvtBase;