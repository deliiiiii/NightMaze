using Cinemachine;
using General;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM;

public class MyCamera : Singleton<MyCamera>
{
    [SerializeField] Camera main;
    [SerializeField] CinemachineVirtualCamera mainV;
    [SerializeField] Camera ui;
    [SerializeField] CinemachineVirtualCamera uiV;
    public static CinemachineVirtualCamera MainV => Instance.mainV;
    // ReSharper disable once InconsistentNaming
    public static CinemachineVirtualCamera UIV => Instance.uiV;
    public static Camera Main => Instance.main;
    public static Camera UI => Instance.ui;

    public static Vector3 ScreenDeltaToWorldDelta(Camera camera, Vector3 screenDelta)
    {
        var curScreenPos = Input.mousePosition;
        var preScreenPos = curScreenPos - screenDelta;
        float zDepth = -camera.transform.position.z; 
        curScreenPos.z = zDepth;
        preScreenPos.z = zDepth;
        Vector3 curWorldPos = camera.ScreenToWorldPoint(curScreenPos);
        Vector3 prevWorldPos = camera.ScreenToWorldPoint(preScreenPos);
        return curWorldPos - prevWorldPos;
    }
}
