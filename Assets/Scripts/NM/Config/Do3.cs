using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace NM.Config;
[TypeRegistryItem("立即")]
public record EvtSpinImmediateDoSymbol : EvtBase;
[TypeRegistryItem("某符号每旋转N次\t(SymbolEtt, Int)")]
public record EvtSymbolEverySpinN(SymbolEtt Symbol, int N) 
    : EvtBase2<SymbolEtt, int>(Symbol, N);
[TypeRegistryItem("某符号添加某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolAddSymbol(SymbolEtt Symbol, SymbolEtt Added) 
    : EvtBase2<SymbolEtt, SymbolEtt>(Symbol, Added);
[TypeRegistryItem("某符号消除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolDestroySymbol(SymbolEtt Symbol, SymbolEtt Destroyed) 
    : EvtBase2<SymbolEtt, SymbolEtt>(Symbol, Destroyed);
[TypeRegistryItem("某符号移除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolRemoveSymbol(SymbolEtt Symbol, SymbolEtt Removed) 
    : EvtBase2<SymbolEtt, SymbolEtt>(Symbol, Removed);
[TypeRegistryItem("某符号临时加算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutAddTemp(SymbolEtt Symbol, int Add) 
    : EvtBase2<SymbolEtt, int>(Symbol, Add);
[TypeRegistryItem("某符号临时乘算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutMulTemp(SymbolEtt Symbol, int Mul) 
    : EvtBase2<SymbolEtt, int>(Symbol, Mul);
[TypeRegistryItem("某符号永久加算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutAddPermanent(SymbolEtt Symbol, int Add) 
    : EvtBase2<SymbolEtt, int>(Symbol, Add);
[TypeRegistryItem("某符号积攒X时\t(SymbolEtt, int)")]
public record EvtSymbolStock(SymbolEtt Symbol, int Stock) 
    : EvtBase2<SymbolEtt, int>(Symbol, Stock);
[TypeRegistryItem("玩家移除某符号时\t(SymbolEtt)")]
public record EvtPlayerRemoveSymbol(SymbolEtt Symbol) 
    : EvtBase1<SymbolEtt>(Symbol);
[TypeRegistryItem("某符号与某符号相邻时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolAdjacentSymbol(SymbolEtt Symbol, SymbolEtt AdjacentSymbol)
    : EvtBase2<SymbolEtt, SymbolEtt>(Symbol, AdjacentSymbol);
[TypeRegistryItem("某符号旋转到某位置时\t(SymbolEtt, Vector2Int)")]
public record EvtSpinSymbolAt(SymbolEtt Symbol, Vector2Int Pos) 
    : EvtBase2<SymbolEtt, Vector2Int>(Symbol, Pos);


#region DoCount
public abstract class DoCountBase;
[TypeRegistryItem("无限次")]
public class DoCountInfinite : DoCountBase;
[TypeRegistryItem("限N次")]
public class DoCountNumber : DoCountBase
{
    [MinValue(1)]public int N = 1;
}
#endregion
[DisableContextMenu]
public class EvtReceiver
{
    [LabelText("当发生"), Required, PropertyOrder(0), OnValueChanged(nameof(OnREvtTypeChanged)), ValueDropdown(nameof(REvtTypeList)), DisableContextMenu] 
    [MaybeNull]public Type REvtType = null!;
    [LabelText(BesidesArg1SatisfyInfo), PropertyOrder(1), ShowIf(nameof(HasArg1)), ValueDropdown(nameof(FilterList1)), HideReferenceObjectPicker, DisableContextMenu]
    public List<FilterBase?>? Filter1OrList = [];
    [LabelText(BesidesArg2SatisfyInfo), PropertyOrder(2), ShowIf(nameof(HasArg2)), ValueDropdown(nameof(FilterList2)), HideReferenceObjectPicker, DisableContextMenu]
    public List<FilterBase?>? Filter2OrList = [];
    [LabelText("可执行次数"), Required, PropertyOrder(10), DisableContextMenu]
    public DoCountBase DoCount = new DoCountInfinite();
    [LabelText("就依次执行"), Required, PropertyOrder(11), ListDrawerSettings(CustomAddFunction = nameof(CreateNewSender)), HideReferenceObjectPicker, DisableContextMenu]
    public List<EvtSender> EvtSenderList = [];
    const string BesidesArg1SatisfyInfo = "且事件的参数1满足任意一条\"与逻辑\"..(为空视为直接满足)";
    const string BesidesArg2SatisfyInfo = "且事件的参数2满足任意一条\"与逻辑\"..(为空视为直接满足)";
    
    [field: AllowNull, MaybeNull]
    static List<ValueDropdownItem<Type>> REvtTypeList => field ??= GetEvtTypeList();
    public static List<ValueDropdownItem<Type>> GetEvtTypeList()
    {
        var ret = typeof(EvtBase).SubTypeList()
            .Select(t => new ValueDropdownItem<Type>
            {
                Value = t,
                Text = t.GetAttribute<TypeRegistryItemAttribute>()?.Name ?? t.Name
            })
            .ToList();
        return ret;
    }
    void OnREvtTypeChanged()
    {
        REvtArgTypes = null!;
        FilterList1 = null!;
        FilterList2 = null!;
    }
    bool HasArg1 => REvtArgTypes.ContainsKey(0);
    bool HasArg2 => REvtArgTypes.ContainsKey(1);
    [field: AllowNull, MaybeNull]
    public Dictionary<int, Type> REvtArgTypes
    {
        get => field ??= GetREvtArgTypes();
        set => field = value;
    }
    Dictionary<int, Type> GetREvtArgTypes()
    {
        Dictionary<int, Type> ret = [];
        var type = REvtType;
        if (type == null)
            return ret;
        if (type.ImplementsOpenGenericClass(typeof(EvtBase1<>)))
        {
            var args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(EvtBase1<>));
            ret[0] = args[0];
        }
        else if (type.ImplementsOpenGenericClass(typeof(EvtBase2<,>)))
        {
            var args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(EvtBase2<,>));
            ret[0] = args[0];
            ret[1] = args[1];
        }
        return ret;
    }

    List<ValueDropdownItem<FilterBase>> GetFilterListN(int n) =>
        !REvtArgTypes.ContainsKey(n) ? [] :
        typeof(FilterBase<>).MakeGenericType(REvtArgTypes[n]).SubTypeList()
            .Select(t => new ValueDropdownItem<FilterBase>
            {
                Value = (FilterBase)Activator.CreateInstance(t),
                Text = t.GetAttribute<TypeRegistryItemAttribute>()?.Name ?? t.Name
            })
            .ToList();
    [field: AllowNull, MaybeNull]
    List<ValueDropdownItem<FilterBase>> FilterList1
    {
        get => field ??= GetFilterListN(0);
        set => field = value;
    }
    [field: AllowNull, MaybeNull]
    List<ValueDropdownItem<FilterBase>> FilterList2
    {
        get => field ??= GetFilterListN(1);
        set => field = value;
    }

    EvtSender CreateNewSender() => new();


    // public IEnumerable<IFuncWrap> CreateBinding()
    // {
    //     yield return EvtBus.Bind(REvt?.GetType() ?? typeof(EvtCheckSelf), evt =>
    //     {
    //         if ((Filter1OrList ?? []).Any(filter1 => !filter1.FilterFunc(evt.Arg1)))
    //         {
    //             return UniTask.CompletedTask;
    //         }
    //         // EvtList.ForEach(eSender =>
    //         // {
    //         //     EvtBus.FireAsync(eSender.)
    //         // });
    //         return UniTask.CompletedTask;
    //     });
    // }
}
[DisableContextMenu]
public class EvtSender
{
    [/*LabelText("发送事件"),*/Required, OnValueChanged(nameof(OnSEvtTypeChanged)), ValueDropdown(nameof(SEvtTypeList)), DisableContextMenu] 
    [MaybeNull]public Type SEvtType = null!;
#if UNITY_EDITOR
    [LabelText("选择参数1"), Required, ShowIf(nameof(HasArg1)), ValueDropdown(nameof(GetSelector1List)), DisableContextMenu] 
#endif
    public SelectBase Arg1Selector = null!;
#if UNITY_EDITOR
    [LabelText("选择参数2"), Required, ShowIf(nameof(HasArg2)), ValueDropdown(nameof(GetSelector2List)), DisableContextMenu] 
#endif
    public SelectBase Arg2Selector = null!;
#if UNITY_EDITOR
    
    bool HasArg1 => SEvtArgTypes.ContainsKey(0);
    bool HasArg2 => SEvtArgTypes.ContainsKey(1);
    void OnSEvtTypeChanged()
    {
        SEvtArgTypes = null!;
        Arg1Selector = null!;
        Arg2Selector = null!;
    }
    [field: AllowNull, MaybeNull]
    static List<ValueDropdownItem<Type>> SEvtTypeList => field ??= GetEvtTypeList();
    static List<ValueDropdownItem<Type>> GetEvtTypeList()
    {
        var ret = typeof(EvtBase).SubTypeList()
            .Select(t => new ValueDropdownItem<Type>
            {
                Value = t,
                Text = t.GetAttribute<TypeRegistryItemAttribute>()?.Name ?? t.Name
            })
            .ToList();
        return ret;
    }
    
    [field: AllowNull, MaybeNull]
    public Dictionary<int, Type> SEvtArgTypes
    {
        get => field ??= GetSEvtArgTypes();
        set => field = value;
    }
    Dictionary<int, Type> GetSEvtArgTypes()
    {
        Dictionary<int, Type> ret = [];
        var type = SEvtType;
        if(type == null)
            return ret;
        if (type.ImplementsOpenGenericClass(typeof(EvtBase1<>)))
        {
            var args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(EvtBase1<>));
            ret[0] = args[0];
        }
        else if (type.ImplementsOpenGenericClass(typeof(EvtBase2<,>)))
        {
            var args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(EvtBase2<,>));
            ret[0] = args[0];
            ret[1] = args[1];
        }
        return ret;
    }


    List<ValueDropdownItem<SelectBase>> GetSelector1List(InspectorProperty property)
        => SEvtArgTypes.ContainsKey(0) ? GetSelectorList(SEvtArgTypes[0], property) : [];
    List<ValueDropdownItem<SelectBase>> GetSelector2List(InspectorProperty property)
        => SEvtArgTypes.ContainsKey(1) ? GetSelectorList(SEvtArgTypes[1], property) : [];
    List<ValueDropdownItem<SelectBase>> GetSelectorList(Type requireType, InspectorProperty property)
    {
        var parent = property.Parent;
        EvtReceiver evtReceiver = null!;
        while (parent != null)
        {
            if (parent.ValueEntry is { WeakSmartValue: EvtReceiver })
            {
                evtReceiver = (EvtReceiver)parent.ValueEntry.WeakSmartValue;
                break;
            }
            parent = parent.Parent;
        }
        var ret = typeof(SelectBase<>).MakeGenericType(requireType).SubTypeList()
            // .Where(t =>
            // {
            //     if (!t.ImplementsOpenGenericClass(typeof(SelectCustomBase<>)))
            //         return true;
            //     return evtReceiverBase.REvtArgTypes.Count switch
            //     {
            //         1 => t.ImplementsOpenGenericClass(typeof(SelectCustomBase<,>))
            //                 && t.GetArgumentsOfInheritedOpenGenericClass(typeof(SelectCustomBase<,>))[0] == evtReceiverBase.REvtArgTypes[0],
            //         2 => t.ImplementsOpenGenericClass(typeof(SelectCustomBase<,,>))
            //                 && t.GetArgumentsOfInheritedOpenGenericClass(typeof(SelectCustomBase<,,>))[0] == evtReceiverBase.REvtArgTypes[0]
            //                 && t.GetArgumentsOfInheritedOpenGenericClass(typeof(SelectCustomBase<,,>))[1] == evtReceiverBase.REvtArgTypes[1],
            //         _ => true
            //     };
            // })
            .Where(t => !t.IsAbstract)
            .Select(t =>
            {
                var instance = (SelectBase)Activator.CreateInstance(t);
                var label = t.GetAttribute<SelectorAttribute>()?.Text ?? t.Name;
                return new ValueDropdownItem<SelectBase> { Text = label, Value = instance };
            })
            .ToList();
        evtReceiver.REvtArgTypes.ForEach(rEntArgType =>
        {
            var nth = rEntArgType.Key + 1;
            if (rEntArgType.Value == requireType)
            {
                ret.Add(new ValueDropdownItem<SelectBase>
                {
                    Text = $"选择第{nth}个事件参数",
                    Value = new SelectFromEvtArgNth(nth)
                });
            }
        });
        return ret;
    }
#endif
}
[DisableContextMenu]
public abstract class FilterBase
{
    public override string ToString()
    {
        return GetType().GetAttribute<TypeRegistryItemAttribute>()?.Name ?? GetType().Name;
    }
}
public abstract class FilterBase<T> : FilterBase
{
    // (为None代表本行"与逻辑"结束)
    [LabelText("且"), PropertyOrder(999)] public FilterBase<T>? And;
    public bool FilterFunc(T value) => true;
}
public abstract class FilterSymbolBase : FilterBase<SymbolEtt>;
[TypeRegistryItem("符号为自身")]
public class FilterSymbolIsSelf : FilterSymbolBase;
[TypeRegistryItem("符号在老虎机中出现")]
public class FilterSymbolShown : FilterSymbolBase;
[TypeRegistryItem("符号在角落")]
public class FilterSymbolInCorner : FilterSymbolBase;
[TypeRegistryItem("符号种类为指定某一个")]
public class FilterSymbolIsOne : FilterSymbolBase
{
    [LabelText("选择单个符号"), Required]public SelectBase<SymbolEtt> OneSymbolSelector = new SelectDirectOneSymbol();
}
[TypeRegistryItem("符号种类属于指定一组")]
public class FilterSymbolIsOfList : FilterSymbolBase
{
    [LabelText("选择一组符号"), Required]public SelectBase<List<SymbolEtt>> ListSymbolSelector = new SelectDirectSetSymbol();
}
[TypeRegistryItem("N等于")]
public class FilterNEqual : FilterBase<int>
{
    [LabelText("输入目标值"), Required]public SelectBase<int> IntSelector = new SelectDirectInt();
}
[DisableContextMenu]
public abstract class SelectBase;
public abstract class SelectBase<T> : SelectBase
{
    public override string ToString()
    {
        return GetType().GetAttribute<SelectorAttribute>()?.Text ?? GetType().Name;
    }
}

public abstract class SelectCustomBase<T> : SelectBase<T>;
public abstract class SelectCustomBase<T1, T> : SelectCustomBase<T>;
public abstract class SelectCustomBase<T1, T2, T> : SelectCustomBase<T>;
[Selector("xjbs")]
public class SelectXxx : SelectCustomBase<SymbolEtt, SymbolEtt, int>;
// public abstract class SelectNotDirectBase<T> : SelectBase<T>
// {
//     [LabelText("且满足任一..(空列表/None视为直接满足)"), PropertyOrder(-1)] public List<FilterBase<T>>? FilterOrList = [];
// }
public abstract class SelectDirectBase<T> : SelectBase<T>;

public class SelectFromEvtArgNth(int n) : SelectBase
{
    [UnityEngine.HideInInspector]public int N = n;
    public override string ToString() => $"选择第{N}个事件参数";
}
[Selector("直接Int")]
public class SelectDirectInt : SelectDirectBase<int>
{
    public int Value = 1;
}
[Selector("符号自身")]
public class SelectSymbolSelf : SelectDirectBase<SymbolEtt>;
[Selector("指定一个符号类型")]
public class SelectDirectOneSymbol : SelectDirectBase<SymbolEtt>
{
    [LabelText("符号"), Required] public SymbolConfig One = null!;
}
[Selector("指定一组符号类型")]
public class SelectDirectSetSymbol : SelectDirectBase<List<SymbolEtt>>
{
    [LabelText("符号组"), Required] public SymbolConfigSet Set = null!;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SelectorAttribute(string text) : TypeRegistryItemAttribute(text)
{
    public readonly string Text = text;
}

