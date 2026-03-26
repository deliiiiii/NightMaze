using System.Collections.Generic;
using General;
using GeneralPreview;
using UnityEngine;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class NewSlotParamView : ViewBase
{
    [SerializeField] Btn btnClose;
    [SerializeField] Btn btnStart;
    [SerializeField] Ipt iptName;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnClose.onClick.EvtBindTo(gameObject.SetActiveFalse);
        yield return btnStart.onClick.EvtBindTo(() => new EvtCLickStartNew(iptName.text).Forget());
    }
    public record EvtCLickStartNew(string PlayerName) : EvtForgetBase;
}