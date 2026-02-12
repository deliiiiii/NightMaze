using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.ZZZTest;

public class UniTasCancel : MonoBehaviour
{
    CancellationTokenSource cts => field ??= new CancellationTokenSource();
    [Button]
    public void Cancel()
    {
        cts.Cancel();
    }

    async UniTask Awake()
    {
        await GetComponent<DOTweenSequence>().PlayAsync(cts.Token);
        MyDebug.Log("tween end...");
        
        try
        {
           
        }
        catch (Exception e)
        {
            // MyDebug.LogError(e);
        }
    }
}