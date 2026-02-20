using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SymbolView : MonoBehaviour
{
    [SerializeField, Required] DOTweenSequence onSpinTween;

    [ShowInInspector, ReadOnly]
    public SymbolEtt SymbolEtt
    {
        get;
        set
        {
            field = value;
            TxtName.text = field.IsEmpty ? string.Empty : field.Config.Name;
            TxtAdd.text = string.Empty;
            TxtMulti.text = string.Empty;
            TxtCoin.text = field.GetUltimateGive().ToString();
            buffList.ForEach(Destroy);
            buffList.Clear();
        }
    } = null!;

    public TextMeshProUGUI TxtName;
    public TextMeshProUGUI TxtAdd;
    public TextMeshProUGUI TxtMulti;
    public TextMeshProUGUI TxtCoin;
    [SerializeField] Transform tranImgBuff;
    [ShowInInspector, ReadOnly] List<ImgBuff> buffList = [];

    void Awake()
    {
        Bus.Register(OnEvtSpinImmediateDoSymbolAsync);
        Bus.Register(OnEvtSpinSymbolUltimateGiveChangedAsync);
    }

    void OnDestroy()
    {
        Bus.UnRegister(OnEvtSpinImmediateDoSymbolAsync);
        Bus.UnRegister(OnEvtSpinSymbolUltimateGiveChangedAsync);
    }

    [UniEvtDes("放变红动画")]
    UniEvt<EvtSpinImmediateDoSymbol> OnEvtSpinImmediateDoSymbolAsync => async (evt, ct) =>
    {
        if (evt.Symbol != SymbolEtt || SymbolEtt.IsEmpty)
            return;
        await onSpinTween.PlayAsync(ct);
    };

    [UniEvtDes("UltimateGive 变化时更新文本")]
    UniEvt<EvtSpinSymbolUltimateGiveChanged> OnEvtSpinSymbolUltimateGiveChangedAsync => (evt, ct) =>
    {
        if (evt.Symbol == SymbolEtt)
            TxtCoin.text = evt.UltimateGive.ToString();
        return UniTask.CompletedTask;
    };
}