using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;
// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace NM.View;

public class NewSlotParamView : ViewBase
{
    [SerializeField] Btn btnClose;
    [SerializeField] Btn btnStart;
    [SerializeField] Ipt iptName;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnClose.onClick.EvtBindTo(gameObject.SetActiveFalse);
        yield return btnStart.onClick.EvtBindTo(() =>
        {
            SLViewIns.SetActiveFalse();
            this.SetActiveFalse();
            var data = new GamePlaying(playerName: iptName.text);
            GameRoot.ChangeStateAsync(data, false).Forget();
        });
    }

    void OnEnable()
    {
        iptName.text = string.Empty;
    }
}