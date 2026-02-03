
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class ImageExtensionData
{
    public bool Started;
    public float CurValue;
    public float TarValue;
    public float DeltaPerSecond;
    public CancellationTokenSource CancelTokenSource;
}

public static class ImageExt
{
    static Dictionary<Image, ImageExtensionData> dic = new();
    static int staticCount = 0;
    const float UPDATE_INTERVAL = 0.02f; 
    public static void DoFluentFill(this Image img, float tarValue, float deltaPerSecond)
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
        if (!dic.ContainsKey(img))
        {
            dic.Add(img, new ImageExtensionData
            {
                CurValue = img.fillAmount,
                DeltaPerSecond = deltaPerSecond,
                CancelTokenSource = new CancellationTokenSource(),
            });
        }
        dic[img].TarValue = tarValue;
        if (!dic[img].Started)
        {
            FluentFill(img);
        }
    }

    static async void FluentFill(Image img)
    {
        dic[img].Started = true;
        while (!Mathf.Approximately(dic[img].CurValue, dic[img].TarValue))
        {
            bool positive = dic[img].TarValue - dic[img].CurValue > 0;
            if (positive)
                dic[img].CurValue = Math.Clamp(
                    dic[img].CurValue + UPDATE_INTERVAL * dic[img].DeltaPerSecond,
                    dic[img].CurValue,
                    dic[img].TarValue
                    );
            else
                dic[img].CurValue = Math.Clamp(
                    dic[img].CurValue - UPDATE_INTERVAL * dic[img].DeltaPerSecond,
                    dic[img].TarValue,
                    dic[img].CurValue
                    );
            img.fillAmount = dic[img].CurValue;
            await UniTask.WaitForSeconds(UPDATE_INTERVAL, cancellationToken: dic[img].CancelTokenSource.Token);
        }
        img.fillAmount = dic[img].TarValue;
        dic.Remove(img);

    }
}