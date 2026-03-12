using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SymbolView : ViewBase
{
    [SerializeField, Required] DOTweenSequence onSpinTween;

    [ShowInInspector, ReadOnly]
    public SymbolData Data
    {
        get;
        set
        {
            field = value;
            TxtName.text = field.IsEmpty ? string.Empty : field.Name;
            TxtAdd.text = string.Empty;
            TxtMulti.text = string.Empty;
            TxtCoin.text = field.GetUltimateGive().ToString();
            buffList.ForEach(Destroy);
            buffList.Clear();
        }
    } = null!;

    public Txt TxtName;
    public Txt TxtAdd;
    public Txt TxtMulti;
    public Txt TxtCoin;
    [SerializeField] Trs tranImgBuff;
    [ShowInInspector, ReadOnly] List<ImgBuff> buffList = [];


    UniEvt<PlayingSpin.EvtImmediateDoSymbol> OnSpinEvtImmediateDoSymbol => new()
    {
        Invoke = async (evt, ct) =>
        {
            if (evt.Symbol != Data || Data.IsEmpty)
                return;
            await onSpinTween.PlayAsync(ct);
        },
        Des = "放变红动画"
    };
    UniEvt<SymbolData.EvtUltimateGiveChanged> OnSymbolEvtUltimateGiveChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            if (evt.Symbol == Data)
                TxtCoin.text = evt.UltimateGive.ToString();
            return UniTask.CompletedTask;
        },
        Des = "UltimateGive 变化时更新文本"
    };
}