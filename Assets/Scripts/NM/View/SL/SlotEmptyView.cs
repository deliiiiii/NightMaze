using System;
using System.Collections.Generic;
using General;
using GeneralPreview;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SlotEmptyView : ViewBase
{
    [SerializeField]Button btnAdd;
    public event Action? OnClick;
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return Binder.FromEvt(btnAdd.onClick).To(() => OnClick?.Invoke());
    }
}