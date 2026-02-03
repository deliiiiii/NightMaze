using System;
using System.Collections.Generic;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace NM;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "NM/" + nameof(SymbolConfig))]
public class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    public int Payout = 1;
    [NonSerialized, OdinSerialize, Required] public EffectBase Eff = EffectNone.One;
}

#region Event
public interface IEvt<out T>;
public class EvtImmediate : IEvt<ValueTuple>
{
    public static readonly EvtImmediate One = new ();
}
public class EvtOnSelf : IEvt<SymbolConfig>
{
    [Header("自身满足条件")]
    [Required] public SymbolFilterBase SymbolFilter = SymbolFilterTrue.One;
}
public class EvtOnSymbol : IEvt<SymbolConfig>
{
    [Header("选择符号范围")]
    [Required] public SymbolSelectorBase SymbolSelector = SymbolSelectSelf.One;
    [Header("这些符号满足条件")]
    [Required] public SymbolFilterBase SymbolFilter = SymbolFilterTrue.One;
}
#endregion

#region Symbol Selector/Filter
[Serializable]
public abstract class SymbolSelectorBase;
public class SymbolSelectSelf : SymbolSelectorBase
{
    public static readonly SymbolSelectSelf One = new ();
}
public class SymbolSelectSet : SymbolSelectorBase
{
    [Required] public SymbolConfigSet Set = null!;
}

[Serializable]
public abstract class SymbolFilterBase;
public class SymbolFilterTrue : SymbolFilterBase
{
    public static readonly SymbolFilterTrue One = new ();
}
public class SymbolFilterAdjacent : SymbolFilterBase;
public class SymbolFilterEveryNTurn : SymbolFilterBase
{
    [Header("每旋转?次")]
    public int Threshold;
}
#endregion

#region EffectCount
[Serializable]
public abstract class EffectCountBase;
public class EffectCountInfinite : EffectCountBase
{
    public static readonly EffectCountInfinite One = new ();
}
public class EffectCountNumber : EffectCountBase
{
    public int Count = 1;
}
#endregion

#region Effect

public abstract class EffectBase;
[Serializable]
public abstract class EffectBase<TIn, TEvt> : EffectBase
    where TEvt : IEvt<TIn>, new()
{
    [Header("触发条件")]
    [Required] public TEvt Evt = new();
    [Header("触发次数(默认无限)")]
    [Required] public EffectCountBase EffectCount = EffectCountInfinite.One;
    [Required] public List<DoBase> DoList = [];
}
public class EffectNone : EffectBase<ValueTuple, EvtImmediate>
{
    public static readonly EffectNone One = new ();
}
public class EffectOnSymbol : EffectBase<SymbolConfig, EvtOnSymbol>;
public class EffectOnSelf : EffectBase<SymbolConfig, EvtOnSelf>;

#endregion
//
// public abstract class DoBase;
[Serializable]
public abstract class DoBase //: DoBase
{
    [PropertyOrder(999)]public List<EffectBase> ThenEffList = [];
}
public abstract class DoNone : DoBase;
public abstract class DoOnSymbol : DoBase;
public class DoRanPayout : DoOnSymbol
{
    public int MinPayout;
    public int MaxPayout;
}
public class DoAddGiveTemp : DoOnSymbol
{
    public int Add;
}
public class DoAddGivePermanent : DoOnSymbol
{
    public int Add;
}
[Serializable]
public abstract class DoOnWorld : DoBase;
public class DoAddCoin : DoOnWorld
{
    public int Add;
}