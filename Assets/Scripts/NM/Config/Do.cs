using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace NM;
public abstract class EffBase;
public class EffNone : EffBase;
public abstract class EffOn<T> : EffBase
{
    [Required] public required SelectDiscardSymbol<T> Selector;
    [Required] public required FilterBase<T> Filter;
    [Required] public required DoCountBase DoCount = new DoCountInfinite();
    [Required] public List<DoBase<T>> DoList = [];
}
public class EffOnSymbol : EffOn<SymbolConfig>
{
    public EffOnSymbol()
    {
        Selector = new SelectSymbolSelf();
        Filter = new FilterTrue<SymbolConfig>();
    }
}
public class EffOnDo<T, TDo> : EffBase where TDo : DoBase<T>
{
    [Required] public FilterBase<TDo> Filter = new FilterTrue<TDo>();
    [Required] public List<DoBase<TDo>> DoList = [];
}

public abstract class SelectorBase<TIn, TOut>;
public class SelectFilteredSymbol : SelectorBase<SymbolConfig, SymbolConfig>;
public abstract class SelectDiscardSymbol<TOut> : SelectorBase<SymbolConfig, TOut>;

public class SelectSymbolAll : SelectDiscardSymbol<SymbolConfig>;
public class SelectSymbolSelf : SelectDiscardSymbol<SymbolConfig>;
public class SelectSymbolOne : SelectDiscardSymbol<SymbolConfig>
{
    [Required] public SymbolConfig One = null!;
}
public class SelectSymbolSet : SelectDiscardSymbol<SymbolConfig>
{
    [Required] public SymbolConfigSet Set = null!;
}


public abstract class FilterBase<T>;
public class FilterTrue<T> : FilterBase<T>;
public abstract class FilterSymbolBase : FilterBase<SymbolConfig>;
public class FilterSymbolAdjacent : FilterSymbolBase;
public class FilterSymbolEveryNSpin : FilterSymbolBase
{
    [MinValue(1)]public int N = 1;
}
public class FilterSymbolInCorner : FilterSymbolBase;
public class FilterSymbolDestroyed : FilterSymbolBase;
public class FilterSymbolRemoved : FilterSymbolBase;


#region DoCount
[Serializable]
public abstract class DoCountBase;
public class DoCountInfinite : DoCountBase;
public class DoCountNumber : DoCountBase
{
    [MinValue(1)]public int N = 1;
}
#endregion


public abstract class DoBase<T>
{
    [PropertyOrder(999), Required] public List<EffOnDo<T, DoBase<T>>> EffList = [];
}
public class DoSymbolGiveAddTemp : DoBase<SymbolConfig>
{
    [MinValue(1)]public int AddTemp = 1;
}
public class DoSymbolGiveAddPermanent : DoBase<SymbolConfig>
{
    [MinValue(1)]public int AddPermanent = 1;
}
public class DoSymbolStock : DoBase<SymbolConfig>
{
    [MinValue(1)]public int N = 1;
}
public class DoSymbolAddSymbol : DoBase<SymbolConfig>
{
    [Required] public SelectorBase<SymbolConfig, SymbolConfig> ToAddSelector = null!;
}
public class DoSymbolDestroySymbol : DoBase<SymbolConfig>
{
    [Required] public SelectorBase<SymbolConfig, SymbolConfig> ToAddSelector = null!;
}
// void Test()
// {
//     global
//         .Select(() => selfSymbol)
//         .Where()
//         .Do(s => addSymbol(s => sa));
// }