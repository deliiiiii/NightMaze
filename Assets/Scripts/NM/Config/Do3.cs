using System;
using System.Collections.Generic;
using System.Linq;
using NM.Data;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#if UNITY_EDITOR
#endif

namespace NM.Config;
[DisableContextMenu]
public abstract class FilterBase
{
    public override string ToString()
    {
        return GetType().GetAttribute<TypeRegistryItemAttribute>()?.Name ?? GetType().Name;
    }
    public abstract bool Filter(GamePlaying ctx, SymbolEtt symbol, object thatEvtArg);
}
public abstract class FilterBase<T> : FilterBase
{
    // (为None代表本行"与逻辑"结束)
    [LabelText("且"), PropertyOrder(999)] public FilterBase<T>? And;
    public sealed override bool Filter(GamePlaying ctx, SymbolEtt symbol, object thatEvtArg) 
            => FilterT(ctx, symbol, (T)thatEvtArg) && (And?.Filter(ctx, symbol, thatEvtArg) ?? true);
    protected abstract bool FilterT(GamePlaying ctx, SymbolEtt symbol, T thatEvtArg);
}
public abstract class FilterSymbolBase : FilterBase<SymbolEtt>;

[TypeRegistryItem("符号自身")]
public class FilterSymbolIsSelf : FilterSymbolBase
{
    protected override bool FilterT(GamePlaying ctx, SymbolEtt symbol, SymbolEtt thatEvtArg) => symbol == thatEvtArg;
}
[TypeRegistryItem("符号在老虎机中出现")]
public class FilterSymbolShown : FilterSymbolBase
{
    protected override bool FilterT(GamePlaying ctx, SymbolEtt symbol, SymbolEtt thatEvtArg) 
        => ctx.InState<PlayingSpin>().Match(some => some.SymbolShownList.Contains(thatEvtArg), () => false);
}

[TypeRegistryItem("符号在角落")]
public class FilterSymbolInCorner : FilterSymbolBase
{
    protected override bool FilterT(GamePlaying ctx, SymbolEtt symbol, SymbolEtt thatEvtArg) 
        => symbol.GetCom<SymbolInSpin>().Match(some =>
        {
            var x = some.Pos.X;
            var y = some.Pos.Y;
            return x == Const.SpinFirstID && y == Const.SpinFirstID ||
                   x == Const.SpinFirstID && y == Const.SpinH ||
                   x == Const.SpinW && y == Const.SpinFirstID ||
                   x == Const.SpinW && y == Const.SpinH;
        }, () => false);
}

[TypeRegistryItem("符号种类与自身相同")]
public class FilterSymbolTypeIsSelf : FilterSymbolBase
{
    protected override bool FilterT(GamePlaying ctx, SymbolEtt symbol, SymbolEtt thatEvtArg) 
        => symbol.Config.ID == thatEvtArg.Config.ID;
}

[TypeRegistryItem("符号种类属于指定某一个")]
public class FilterSymbolTypeIsOne : FilterSymbolBase
{
    [LabelText("选择单个符号种类"), Required] public SymbolConfig One = null!;
    protected override bool FilterT(GamePlaying ctx, SymbolEtt symbol, SymbolEtt thatEvtArg) 
        => thatEvtArg.Config.ID == One.ID;
}
[TypeRegistryItem("符号种类属于指定某一组")]
public class FilterSymbolTypeIsOfSet : FilterSymbolBase
{
    [LabelText("选择一组符号种类"), Required]public SymbolConfigSet Set = null!;
    protected override bool FilterT(GamePlaying ctx, SymbolEtt symbol, SymbolEtt thatEvtArg) 
        => Set.SymbolSet.Any(c => c.ID == thatEvtArg.Config.ID);
}
[TypeRegistryItem("应等于")]
public class FilterNEqual : FilterBase<int>
{
    [LabelText("输入目标值"), Required]public SelectBase<int> IntSelector = new SelectDirectInt();
    protected override bool FilterT(GamePlaying ctx, SymbolEtt symbol, int thatEvtArg) 
        => thatEvtArg == IntSelector.SelectT(ctx, symbol);
}

[DisableContextMenu]
public abstract class SelectBase
{
    public abstract object Select(GamePlaying ctx, SymbolEtt symbol, List<object> evtArgList);
}
public abstract class SelectBase<T> : SelectBase
{
    public override string ToString()
    {
        return GetType().GetAttribute<SelectorAttribute>()?.Text ?? GetType().Name;
    }

    public override object Select(GamePlaying ctx, SymbolEtt symbol, List<object> evtArgList) => SelectT(ctx, symbol)!;
    public abstract T SelectT(GamePlaying ctx, SymbolEtt symbol);
}
public abstract class SelectDirectBase<T> : SelectBase<T>;

public class SelectFromEvtArgNth(int n) : SelectBase
{
    [UnityEngine.HideInInspector]public int N = n;
    public override string ToString() => $"选择第{N}个事件参数";
    public override object Select(GamePlaying ctx, SymbolEtt symbol, List<object> evtArgList) 
        => evtArgList.Count >= N ? evtArgList[N - 1] : throw new Exception($"事件参数不足{N}个");
}
[Selector("直接Int")]
public class SelectDirectInt : SelectDirectBase<int>
{
    public int Value = 1;
    public override int SelectT(GamePlaying ctx, SymbolEtt symbol) => Value;
}
[Selector("符号自身")]
public class SelectSymbolSelf : SelectDirectBase<SymbolEtt>
{
    public override SymbolEtt SelectT(GamePlaying ctx, SymbolEtt symbol) => symbol;
}

// [Selector("所有某个类型的符号")]
// public class SelectDirectOneSymbol : SelectDirectBase<List<SymbolEtt>>
// {
//     [LabelText("指定类型"), Required] public SymbolConfig One = null!;
//     public override List<SymbolEtt> SelectT(GamePlaying ctx, SymbolEtt symbol, List<object> evtArgList)
//     {
//         return ctx.Deck.Where(s => s.Config.ID == One.ID).ToList();
//     }
// }
// [Selector("所有某组类型的符号")]
// public class SelectDirectSetSymbol : SelectDirectBase<List<SymbolEtt>>
// {
//     [LabelText("符号组"), Required] public SymbolConfigSet Set = null!;
//     public override List<SymbolEtt> SelectT(GamePlaying ctx, SymbolEtt symbol, List<object> evtArgList)
//     {
//         return ctx.Deck.Where(s => Set.SymbolSet.Any(c => c.ID == s.Config.ID)).ToList();
//     }
// }

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SelectorAttribute(string text) : TypeRegistryItemAttribute(text)
{
    public readonly string Text = text;
}

