using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class TechNodeEditor : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] GO nodeParent;
    [Button]
    public void EnableSelection()
    {
        nodeParent.transform.GetChildren().ForEach(t =>
        {
            UnityEditor.SceneVisibilityManager.instance.DisablePicking(nodeParent, true);
        });
    }
    [Button]
    public void DisableSelection()
    {
        
    }
#endif
}