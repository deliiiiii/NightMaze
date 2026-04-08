using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;
// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SymbolView : ItemViewBase<SymbolView, GamePlaying.Symbol>
{
    public SpriteRenderer Sr;
    public DOTweenSequence OnSpinTween;

    UniEvt<PlaySpin.EvtBeforeCheckSymbol> OnBeforeCheckSymbol => new()
    {
        Invoke = async (evt, ct) =>
        {
            if (evt.Item.BelongEtt != Data.BelongEtt)
                return;
            await OnSpinTween.PlayAsync(ct);
        },
        Des = "符号结算前播放动画.",
    };

    public override void OnCreateView()
    {
        Sr.SetActiveTrue();
    }
}