
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class TextExtensionData
{
    public bool Started;
    public float CurValue;
    public float TarValue;
    public float DeltaPerSecond;
    public string Format;
    public CancellationTokenSource CancelTokenSource;
}

public static class TextExt
{
    static Dictionary<Text, TextExtensionData> dic = new();
    static int staticCount = 0;
    const float UPDATE_INTERVAL = 0.02f;

    public static void DoFluent(this Text txt, float tarValue, float deltaPerSecond, string format)
    {
        
#if UNITY_EDITOR
        if (staticCount == 0)
        {
            staticCount++;
            EditorApplication.playModeStateChanged += (s) =>
            {
                if (s != PlayModeStateChange.ExitingPlayMode)
                    return;
                foreach (var it in dic.Keys)
                {
                    dic[it].CancelTokenSource.Cancel();
                }
                dic.Clear();
            };
        }
#endif
        if (!dic.ContainsKey(txt))
        {
            dic.Add(txt, new TextExtensionData
            {
                CurValue = float.TryParse(txt.text, out float cur) ? cur : 0f,
                DeltaPerSecond = deltaPerSecond,
                Format = format,
                CancelTokenSource = new CancellationTokenSource(),
            });
        }
        dic[txt].TarValue = tarValue;
        if (!dic[txt].Started)
        {
            Fluent(txt);
        }
    }

    static async void Fluent(Text txt)
    {
        dic[txt].Started = true;
        while (!Mathf.Approximately(dic[txt].CurValue, dic[txt].TarValue))
        {
            bool positive = dic[txt].TarValue - dic[txt].CurValue > 0;
            if (positive)
                dic[txt].CurValue = Math.Clamp(
                    dic[txt].CurValue + UPDATE_INTERVAL * dic[txt].DeltaPerSecond,
                    dic[txt].CurValue,
                    dic[txt].TarValue
                    );
            else
                dic[txt].CurValue = Math.Clamp(
                    dic[txt].CurValue - UPDATE_INTERVAL * dic[txt].DeltaPerSecond,
                    dic[txt].TarValue,
                    dic[txt].CurValue
                    );
            txt.text = dic[txt].CurValue.ToString(dic[txt].Format);
            await UniTask.WaitForSeconds(UPDATE_INTERVAL, cancellationToken: dic[txt].CancelTokenSource.Token);
        }
        txt.text = dic[txt].TarValue.ToString(dic[txt].Format);
        dic.Remove(txt);

    }
    
}