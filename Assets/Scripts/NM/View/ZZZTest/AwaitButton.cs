using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class AwaitButton : MonoBehaviour
{
    [SerializeField] Btn btn;

    [Button]
    async UniTask TestAsync()
    {
        MyDebug.Log("111");
        await Bus.WaitForAsync<TestEvt>("测试", destroyCancellationToken);
        MyDebug.Log("222");
    }
    [Button]
    void Fire()
    {
        new TestEvt().Forget();
    }
}

public record TestEvt : EvtForgetBase;