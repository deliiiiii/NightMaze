// using System.Collections.Generic;
// using Sirenix.OdinInspector;
// using Sirenix.Serialization;
// using Sirenix.Utilities;
// using UnityEngine;

// namespace NM;

// #region DoCount
// public abstract class DoCountBase;
// public class DoCountInfinite : DoCountBase;
// public class DoCountNumber : DoCountBase
// {
//     [MinValue(1)]public int N = 1;
// }
// #endregion

// public abstract class DoBase;
// public abstract class DoSome : DoBase
// {
//     [LabelText("可执行次数"),Required] public required DoCountBase DoCount = new DoCountInfinite();
//     [LabelText("当变量..."), Required] public required ISelector Selector;
//
//     public interface ISelector;
//     public abstract class SelectBase<T> : ITransformBase<T>, ISelector
//     {
//         public abstract class FilterBase;
//         [LabelText("且满足...时"), Required, PropertyOrder(999)] public List<FilterBase> FilterList = [];
//     }
//
//     public abstract class ITransformBase<TRet>;
//     public class DirectInt(int value) : ITransformBase<int>
//     {
//         [LabelText("值")] public int Value = value;
//     }
// }
// public class DoNone : DoBase;

// public abstract class DoSymbolBase : DoSome
// {
//     [LabelText("主语"), Required] public ITransformBase<SymbolConfig> OnSymbol = new TransformSymbolFromSelect();
// }
// public abstract class SelectSymbolBase : DoSymbolBase.SelectBase<SymbolConfig>;
// public class SelectSymbolShown : SelectSymbolBase;
// public class SelectSymbolSelf : SelectSymbolBase;
// public class SelectSymbolOne : SelectSymbolBase
// {
//     [LabelText("符号"), Required] public SymbolConfig One = null!;
// }
// public class SelectSymbolSet : SelectSymbolBase
// {
//     [LabelText("符号组"), Required] public SymbolConfigSet Set = null!;
// }
// public abstract class FilterSymbolBase : SelectSymbolBase.FilterBase;
// public class FilterSymbolAdjacent : FilterSymbolBase;
// public class FilterSymbolEveryNSpin : FilterSymbolBase
// {
//     [Required] public int N = 1;
// }
// public class FilterSymbolInCorner : FilterSymbolBase;
// public class FilterSymbolDestroyed : FilterSymbolBase;
// public class FilterSymbolRemoved : FilterSymbolBase;
// public class FilterSymbolRemoveSymbol : FilterSymbolBase
// {
    // [LabelText("已移除的符号"), Required] public DoSymbolDestroySymbol.ITransformBase<SymbolConfig> SymbolRemoved = new TransformSymbolFromSelect();
// }
// public class TransformSymbolFromSelect : DoSymbolBase.ITransformBase<SymbolConfig>;
//
// public class DoSymbolGiveAddTemp : DoSymbolBase
// {
//     [LabelText("临时加成值"), Required] public ITransformBase<int> AddTemp = new DirectInt(1);
// }
// public class DoSymbolGiveAddPermanent : DoSymbolBase
// {
//     [LabelText("永久加成值"), Required] public ITransformBase<int> AddPermanent = new DirectInt(1);
// }
// public class DoSymbolStock : DoSymbolBase
// {
//     [Required] public ITransformBase<int> N = new DirectInt(1);
// }
// public class DoSymbolAddSymbol : DoSymbolBase
// {
//     [LabelText("欲添加的符号"), Required] public ITransformBase<SymbolConfig> SymbolToAdd = new TransformSymbolFromSelect();
// }
// public class DoSymbolDestroySymbol : DoSymbolBase
// {
//     [LabelText("欲消除的符号"), Required] public ITransformBase<SymbolConfig> SymbolToDestroy = new TransformSymbolFromSelect();
// }

// public abstract class DoDeckBase : DoSome;
// public class SelectDeck : DoDeckBase.SelectBase<Deck>;
// public abstract class FilterDeckBase : SelectDeck.FilterBase;
// public class FilterDeckCoinOverX : FilterDeckBase
// {
//     public int X;
// }
// public class FilterDeckHasEssence : FilterDeckBase;
// public class DoDeckAddCoinX : DoDeckBase
// {
//     public int X;
// }
//
// public class Deck
// {
//     public int Remove;
//     public int Refresh;
//     public int Essence;
// }


// Banana Peel:
// global.Select(() => s["thief"])
//        .Where(isAdjacent)
//        .Do(s => Destroy(this, s))
//        .NextDo(destroy(t, s) => Remove(t, t))
