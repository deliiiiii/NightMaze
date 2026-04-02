using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class ResourceView : MonoBehaviour
{
    [field:SerializeReference, ReadOnly] public GamePlaying.Resource Data { get; set; }
    
    public SpriteRenderer Sr;
}