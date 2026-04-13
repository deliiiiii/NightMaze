global using static NM.View.ViewStatic;
using System.Diagnostics;
using System.Threading;
using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;
// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;
[DebuggerStepThrough]
public class ViewStatic : Singleton<ViewStatic>
{ 
    [SerializeField] TitleView titleView;
    public static TitleView TitleViewIns => Instance.titleView;
    [SerializeField] SLView sLView;
    public static SLView SLViewIns => Instance.sLView;
    [SerializeField] PlayView playView;
    public static PlayView PlayViewIns => Instance.playView;
    [SerializeField] SettingView settingView;
    public static SettingView SettingViewIns => Instance.settingView;
    
    public static MyOption<GamePlaying> GamePlayData => 
        from play in GameRoot.GetStateOptional<GamePlaying>()
        select play;
    public static MyOption<PlayIdle> PlayIdleData =>
        from play in GamePlayData
        from idle in play.GetStateOptional<PlayIdle>()
        select idle;
    public static MyOption<PlaySpin> PlaySpinData =>
        from play in GamePlayData
        from spin in play.GetStateOptional<PlaySpin>()
        select spin;

    public static void BindAll(CancellationToken ct)
    {
        TitleViewIns.Bind(ct);
        PlayViewIns.Bind(ct);
        SLViewIns.Bind(ct);
        SettingViewIns.Bind(ct);
    }
}