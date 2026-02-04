using System;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace NM;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "NM/" + nameof(SymbolConfig))]
public class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    public ERarity Rarity;
    public int Payout = 1;
    [NonSerialized, OdinSerialize, Required] public EffBase Eff = new EffNone();
}
//
// #region Event
// public abstract class EvtBase<T>;
// public class EvtImmediate : EvtBase<ValueTuple>
// {
//     public static readonly EvtImmediate One = new ();
// }
// public class EvtOnSelf : EvtBase<SymbolConfig>
// {
//     [Header("自身满足条件")]
//     [Required] public SymbolFilterBase SymbolFilter = SymbolFilterTrue.One;
// }
// public class EvtOnSymbol : EvtBase<SymbolConfig>
// {
//     [Header("选择符号范围")]
//     [Required] public SymbolSelectorBase SymbolSelector = SymbolSelectSelf.One;
//     [Header("这些符号满足条件")]
//     [Required] public SymbolFilterBase SymbolFilter = SymbolFilterTrue.One;
// }
// public class EvtOnDo : EvtBase<DoBase>
// {
//     [Header("上一步的Do满足条件")]
//     [Required] public DoFilterBase DoFilter = DoFilterTrue.One;
// }
// #endregion
//
// #region Symbol Selector/Filter
// [Serializable]
// public abstract class SymbolSelectorBase;
// public class SymbolSelectSelf : SymbolSelectorBase
// {
//     public static readonly SymbolSelectSelf One = new ();
// }
// public class SymbolSelectSet : SymbolSelectorBase
// {
//     [Required] public SymbolConfigSet Set = null!;
// }
//
// [Serializable]
// public abstract class SymbolFilterBase;
// public class SymbolFilterTrue : SymbolFilterBase
// {
//     public static readonly SymbolFilterTrue One = new ();
// }
// public class SymbolFilterAdjacent : SymbolFilterBase;
// public class SymbolFilterEveryNTurn : SymbolFilterBase
// {
//     [Header("每旋转?次")]
//     public int Threshold;
// }
// #endregion
// #region Do Filter
// [Serializable]
// public abstract class DoFilterBase;
//
// public sealed class DoFilterTrue : DoFilterBase
// {
//     public static readonly DoFilterTrue One = new();
// }
// #endregion
//
// #region EffectCount
// [Serializable]
// public abstract class EffectCountBase;
// public class EffectCountInfinite : EffectCountBase
// {
//     public static readonly EffectCountInfinite One = new ();
// }
// public class EffectCountNumber : EffectCountBase
// {
//     public int Count = 1;
// }
// #endregion
//
// #region Effect
//
// [Serializable]
// public abstract class EffectBase<T>
//     where TEvt : EvtBase<T>, new()
// {
//     [Header("触发条件")]
//     [Required] public TEvt Evt = new();
//     [Header("触发次数(默认无限)")]
//     [Required] public EffectCountBase EffectCount = EffectCountInfinite.One;
//     [Required] public List<TDo> DoList = [];
// }
// public class EffectNone : EffectBase<ValueTuple, EvtImmediate, DoNone>
// {
//     public static readonly EffectNone One = new ();
// }
// public class EffectOnSymbol : EffectBase<SymbolConfig, EvtOnSymbol, DoOnSymbol>;
// public class EffectOnSelf : EffectBase<SymbolConfig, EvtOnSelf, DoOnSymbol>;
//
// #endregion
//
// [Serializable]
// public abstract class DoBase<TIn, TEvt>
//     where TEvt : EvtBase<TIn>, new()
// {
//     [PropertyOrder(999)]public List<TEvt> ThenEffList = [];
// }
// public abstract class DoNone : DoBase<ValueTuple, EvtImmediate>;
// public abstract class DoOnSymbol : DoBase<SymbolConfig, EvtOnSymbol>;
// public class DoRanPayout : DoOnSymbol
// {
//     public int MinPayout;
//     public int MaxPayout;
// }
// public class DoAddGiveTemp : DoOnSymbol
// {
//     public int Add;
// }
// public class DoAddGivePermanent : DoOnSymbol
// {
//     public int Add;
// }
// [Serializable]
// public abstract class DoOnWorld : DoBase;
// public class DoAddCoin : DoOnWorld
// {
//     public int Add;
// }
//
// public class EffectOnDo : EffectBase<DoBase, EvtOnDo>;

