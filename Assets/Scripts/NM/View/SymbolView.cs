using System.Collections.Generic;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SymbolView : ViewBase
{
    [SerializeReference] public SymbolEtt SymbolEtt;
    
    public TextMeshProUGUI TxtName;
    public TextMeshProUGUI TxtAdd;
    public TextMeshProUGUI TxtMulti;
    [SerializeField] Transform tranImgBuff;
    [ShowInInspector, ReadOnly] List<ImgBuff> buffList = [];

    protected override void Bind()
    {
    }
}