using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace NM;

#region DoCount
public abstract class DoCountBase;
public class DoCountInfinite : DoCountBase;
public class DoCountNumber : DoCountBase
{
    [MinValue(1)]public int N = 1;
}
#endregion

public interface IEvtReceiver;

public abstract class EvtReceiver1<T1> : IEvtReceiver
{
    [LabelText("且事件的参数1满足任一.."), Required] public List<FilterBase<T1>> Filter1OrList = [];
    [LabelText("可执行次数"),Required] public required DoCountBase DoCount = new DoCountInfinite();
    [LabelText("就依次执行"), Required] public List<IEvtSender> EvtList = [];
}
public abstract class EvtReceiver2<T1, T2> : IEvtReceiver
{
    [LabelText("且事件的参数1满足任一.."), Required] public List<FilterBase<T1>> Filter1OrList = [];
    [LabelText("且事件的参数2满足任一.."), Required] public List<FilterBase<T2>> Filter2OrList = [];
}
public class REvtCheckSelf : EvtReceiver1<SymbolConfig>;
public class REvtSymbolEverySpinN : EvtReceiver2<SymbolConfig, int>;
public class REvtSymbolAddSymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
public class REvtSymbolDestroySymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
// public class REvtSymbolRemoved : Evt1<SymbolConfig>;
public class REvtSymbolRemoveSymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
public class REvtSymbolPayoutAddTemp : EvtReceiver2<SymbolConfig, int>;
public class REvtSymbolPayoutAddPermanent : EvtReceiver2<SymbolConfig, int>;
public class REvtSymbolStock : EvtReceiver2<SymbolConfig, int>;
public class REvtPlayerRemoveSymbol : EvtReceiver1<SymbolConfig>;

public interface IEvtSender;

public abstract class EvtSender1<TSendBy, T1, TEvt1> : IEvtSender where TEvt1 : EvtReceiver1<T1>
{
    [LabelText("选择参数1"), Required] public SelectBase<T1> Arg1Selector = null!;
}
public class SEvtSymbolPayoutAddTemp : EvtSender2<SymbolConfig, int, REvtSymbolPayoutAddTemp>;
public abstract class EvtSender2<T1, T2, TEvt2> : IEvtSender
    where TEvt2 : EvtReceiver2<T1, T2>
{
    [LabelText("选择参数1"), Required] public SelectBase<T1> Arg1Selector = null!;
    [LabelText("选择参数2"), Required] public SelectBase<T2> Arg2Selector = null!;
}


public abstract class FilterBase<T>
{
    [LabelText("且")] public FilterBase<T>? And;
}
public abstract class FilterSymbolBase : FilterBase<SymbolConfig>;
public class FilterSymbolInCorner : FilterSymbolBase;
public class FilterSymbolIsOne : FilterSymbolBase
{
    [LabelText("选择单个符号"), Required]public SelectBase<SymbolConfig> OneSymbolSelector = new SelectSymbolOne();
}

public class FilterSymbolIsOfList : FilterSymbolBase
{
    [LabelText("选择符号列表"), Required]public SelectBase<List<SymbolConfig>> ListSymbolSelector = new SelectSymbolSet();
}

public abstract class SelectBase<T>;
public class SelectFromEvtArg<T> : SelectBase<T>;
public class SelectFromDirectValue<T> : SelectBase<T>
{
    [Required] public T Value = default!;
}
public class SelectSymbolShown : SelectBase<List<SymbolConfig>>;
public class SelectSymbolSelf : SelectBase<SymbolConfig>;
public class SelectSymbolOne : SelectBase<SymbolConfig>
{
    [LabelText("符号"), Required] public SymbolConfig One = null!;
}

public class SelectSymbolSet : SelectBase<List<SymbolConfig>>
{
    [LabelText("符号组"), Required] public SymbolConfigSet Set = null!;
}