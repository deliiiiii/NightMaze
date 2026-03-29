global using static NM.View.ViewStatic;
using System.Diagnostics;
using General;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

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

    public static void BindAll()
    {
        TitleViewIns.Bind();
        PlayViewIns.Bind();
    }

    public static void UnBindAll()
    {
        TitleViewIns.Unbind();
        PlayViewIns.Unbind();
    }
}