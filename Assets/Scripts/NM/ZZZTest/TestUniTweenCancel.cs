using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.ZZZTest;

public class TestUniTweenCancel : MonoBehaviour
{
    CancellationTokenSource cts => field ??= new CancellationTokenSource();
    [Button]
    public void Cancel()
    {
        cts.Cancel();
    }

    [Button]
    async UniTask PlayAsync()
    {
        MyDebug.Log("tween start...");
        await GetComponent<DOTweenSequence>().PlayAsync(cts.Token);
        MyDebug.Log("tween end...");
    }
}