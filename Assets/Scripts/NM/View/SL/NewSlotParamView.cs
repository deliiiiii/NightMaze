using System.Collections.Generic;
using General;
using GeneralPreview;
using NM.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class NewSlotParamView : ViewBase
{
    [SerializeField] Button btnClose;
    [SerializeField] Button btnStart;
    [SerializeField] TMP_InputField iptName;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return Binder.FromEvt(btnClose.onClick).To(() => gameObject.SetActive(false));
        yield return Binder.FromEvt(btnStart.onClick).To(() => Bus.FireAndForget(new SLView.EvtClickLoad(new GamePlaying()
        {
            PlayerName = iptName.text
        })));
    }
}