using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class MainView : ViewBase
{
    [SerializeField]Btn btnOpenSL;
    [SerializeField]Btn btnExit;
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnOpenSL.onClick.EvtBindTo(() =>
        {
            gameObject.SetActive(false);
            new EvtClickBtnOpenSL().Forget();
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
    
    public record EvtClickBtnOpenSL : EvtForgetBase;

    UniEvt<GamePlaying.EvtOnExit> OnExitPlay => new()
    {
        Invoke = (evt, ct) =>
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        },
        Des = "(退出Play状态) 显示标题界面",
    };
    
    UniEvt<SLView.EvtClickReturn> OnEvtClickReturn => new()
    {
        Invoke = (evt, ct) =>
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        },
        Des = "(SLView点击了返回按钮) 尝试返回标题"
    };
}