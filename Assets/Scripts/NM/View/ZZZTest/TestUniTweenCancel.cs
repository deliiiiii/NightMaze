using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View.ZZZTest;

public class TestUniTweenCancel : MonoBehaviour
{
    CancellationTokenSource cts => field ??= new CancellationTokenSource();
    // [Button]
    // public void Cancel()
    // {
    //     cts.Cancel();
    // }

    [Button]
    async UniTask PlayAsync()
    {
        MyDebug.Log("tween start...");
        await GetComponent<DOTweenSequence>().PlayAsync(cts.Token);
        MyDebug.Log("tween end...");
        
    }

    // object o;
    //
    // [Button]
    // public void Test1()
    // {
    //     o = TestRet().First();
    // }
    //
    // [Button]
    // public void Test2()
    // {
    //     MyDebug.Log($"TEST2 -- {TestRet().First() == (UniAction)o}");
    // }
    //
    // static IEnumerable<UniAction> TestRet()
    // {
    //     yield return new UniAction()
    //     {
    //         DoAsync = async ct =>
    //         {
    //             await UniTask.Delay(1000, cancellationToken: ct);
    //             MyDebug.Log("tween TEST...");
    //         },
    //         Des = "Test"
    //     };
    // }
}