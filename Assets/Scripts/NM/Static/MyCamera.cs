using Cinemachine;
using General;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM;

public class MyCamera : Singleton<MyCamera>
{
    [SerializeField] CinemachineVirtualCamera mainIns;
    public static CinemachineVirtualCamera MainV => Instance.mainIns;
    public static Camera Main => field ??= Camera.main!;

    public static Vector3 ScreenDeltaToWorldDelta(Vector3 screenDelta)
    {
        var curScreenPos = Input.mousePosition;
        var preScreenPos = curScreenPos - screenDelta;
        float zDepth = -Main.transform.position.z; 
        curScreenPos.z = zDepth;
        preScreenPos.z = zDepth;
        Vector3 curWorldPos = Main.ScreenToWorldPoint(curScreenPos);
        Vector3 prevWorldPos = Main.ScreenToWorldPoint(preScreenPos);
        return curWorldPos - prevWorldPos;
    }
}
