using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class TitleView : ViewBase
{
    [SerializeField]Btn btnOpenSL;
    [SerializeField]Btn btnExit;
    CancellationTokenSource slCts = new();
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnOpenSL.onClick.EvtBindTo(() =>
        {
            this.SetActiveFalse();
            slCts = new();
            SLViewIns.OnOpenAsync(slCts.Token).Forget();
        });
        yield return btnExit.onClick.EvtBindTo(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }
    UniEvt<GamePlaying.EvtOnExit> OnExitPlay => new()
    {
        Invoke = (evt, ct) =>
        {
            this.SetActiveTrue();
            return UniTask.CompletedTask;
        },
        Des = "(退出Play状态) 显示标题界面",
    };
}