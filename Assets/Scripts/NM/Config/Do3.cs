using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace NM.Config;

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
public abstract class EvtReceiverBase
{
    [LabelText("可执行次数"),Required] public required DoCountBase DoCount = new DoCountInfinite();
    [LabelText("就依次执行"), Required] public List<EvtSenderBase> EvtList = [];
}
public abstract class EvtReceiver1<T1> : EvtReceiverBase
{
    [LabelText("且事件的参数1满足任一.."), PropertyOrder(-1)] public List<FilterBase<T1>>? Filter1OrList = [];
}
public abstract class EvtReceiver2<T1, T2> : EvtReceiverBase
{   
    [LabelText("且事件的参数1满足任一..(空列表/None视为直接满足)"), PropertyOrder(-2)] public List<FilterBase<T1>>? Filter1OrList = [];
    [LabelText("且事件的参数2满足任一..(空列表/None视为直接满足)"), PropertyOrder(-1)] public List<FilterBase<T2>>? Filter2OrList = [];
}
[TypeRegistryItem("直接审视某符号\t(SymbolConfig)")]
public class REvtCheckSymbol : EvtReceiver1<SymbolConfig>;
[TypeRegistryItem("某符号与某符号相邻时\t(SymbolConfig, SymbolConfig)")]
public class REvtAdjacentToSymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
[TypeRegistryItem("某符号每旋转N次\t(SymbolConfig, Int)")]
public class REvtSymbolEverySpinN : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("某符号添加某符号时\t(SymbolConfig, SymbolConfig)")]
public class REvtSymbolAddSymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
[TypeRegistryItem("某符号消除某符号时\t(SymbolConfig, SymbolConfig)")]
public class REvtSymbolDestroySymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
[TypeRegistryItem("某符号移除某符号时\t(SymbolConfig, SymbolConfig)")]
public class REvtSymbolRemoveSymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
[TypeRegistryItem("某符号临时加算时\t(SymbolConfig, int)")]
public class REvtSymbolPayoutAddTemp : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("某符号临时乘算时\t(SymbolConfig, int)")]
public class REvtSymbolPayoutMulTemp : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("某符号永久加算时\t(SymbolConfig, int)")]
public class REvtSymbolPayoutAddPermanent : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("某符号积攒X时\t(SymbolConfig, int)")]
public class REvtSymbolStock : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("玩家移除某符号时\t(SymbolConfig)")]
public class REvtPlayerRemoveSymbol : EvtReceiver1<SymbolConfig>;
public abstract class EvtSenderBase
{
#if UNITY_EDITOR
    static Dictionary<int, Type> GetSourceTypes(InspectorProperty property)
    {
        Dictionary<int, Type> ret = [];
        // 向上查找最近的 EvtReceiver<>
        var parent = property.Parent;
        while (parent != null)
        {
            if (parent.ValueEntry is { WeakSmartValue: EvtReceiverBase })
            {
                var receiverType = parent.ValueEntry.WeakSmartValue.GetType();
                if (receiverType.ImplementsOpenGenericClass(typeof(EvtReceiver1<>)))
                {
                    ret.Add(0, receiverType.GetArgumentsOfInheritedOpenGenericClass(typeof(EvtReceiver1<>))[0]);
                }
                if (receiverType.ImplementsOpenGenericClass(typeof(EvtReceiver2<,>)))
                {
                    var args = receiverType.GetArgumentsOfInheritedOpenGenericClass(typeof(EvtReceiver2<,>));
                    ret.Add(0, args[0]);
                    ret.Add(1, args[1]);
                }
                break;
            }
            parent = parent.Parent;
        }
        return ret;
    }
    
    protected static List<ValueDropdownItem<SelectBase<T>>> GetSelectorList<T>(InspectorProperty property)
    {
        var sourceTypes = GetSourceTypes(property);
        var ret = typeof(SelectBase<T>).GetSubTypes()
            .Where(t =>
            {
                if (!t.ImplementsOpenGenericClass(typeof(SelectCustomBase<>)))
                    return true;
                return sourceTypes.Count switch
                {
                    1 => t.ImplementsOpenGenericClass(typeof(SelectCustomBase<,>))
                            && t.GetArgumentsOfInheritedOpenGenericClass(typeof(SelectCustomBase<,>))[0] == sourceTypes[0],
                    2 => t.ImplementsOpenGenericClass(typeof(SelectCustomBase<,,>))
                            && t.GetArgumentsOfInheritedOpenGenericClass(typeof(SelectCustomBase<,,>))[0] == sourceTypes[0]
                            && t.GetArgumentsOfInheritedOpenGenericClass(typeof(SelectCustomBase<,,>))[1] == sourceTypes[1],
                    _ => true
                };
            })
            .Select(t =>
            {
                var instance = (SelectBase<T>)Activator.CreateInstance(t);
                var label = t.GetAttribute<SelectorAttribute>()?.Text ?? t.Name;
                return new ValueDropdownItem<SelectBase<T>>() { Text = label, Value = instance };
            })
            .ToList();
        sourceTypes.ForEach(kv =>
        {
            var nth = kv.Key + 1;
            if (kv.Value == typeof(T))
            {
                ret.Add(new ValueDropdownItem<SelectBase<T>>()
                {
                    Text = $"选择第{nth}个事件参数",
                    Value = new SelectFromEvtArgNth<T>(nth)
                });
            }
        });
        return ret;
    }
#endif
}
public abstract class EvtSender1<T1, TEvt1> : EvtSenderBase where TEvt1 : EvtReceiver1<T1>
{
#if UNITY_EDITOR
    [LabelText("选择参数1"), Required, ValueDropdown(nameof(GetSelectorsT1))] 
#endif
    public SelectBase<T1> Arg1Selector = null!;
#if UNITY_EDITOR
    List<ValueDropdownItem<SelectBase<T1>>> GetSelectorsT1(InspectorProperty property) => GetSelectorList<T1>(property);
#endif
}
public abstract class EvtSender2<T1, T2, TEvt2> : EvtSenderBase where TEvt2 : EvtReceiver2<T1, T2>
{
#if UNITY_EDITOR
    [LabelText("选择参数1"), Required, ValueDropdown(nameof(GetSelectorsT1))] 
#endif
    public SelectBase<T1> Arg1Selector = null!;
#if UNITY_EDITOR
    [LabelText("选择参数2"), Required, ValueDropdown(nameof(GetSelectorsT2))] 
#endif
    public SelectBase<T2> Arg2Selector = null!;
#if UNITY_EDITOR
    List<ValueDropdownItem<SelectBase<T1>>> GetSelectorsT1(InspectorProperty property) => GetSelectorList<T1>(property);
    List<ValueDropdownItem<SelectBase<T2>>> GetSelectorsT2(InspectorProperty property) => GetSelectorList<T2>(property);
#endif
}
[TypeRegistryItem("使某符号临时加算若干\t(SymbolConfig, Int)")]
public class SEvtSymbolPayoutAddTemp : EvtSender2<SymbolConfig, int, REvtSymbolPayoutAddTemp>;
[TypeRegistryItem("使某符号临时乘算若干\t(SymbolConfig, Int)")]
public class SEvtSymbolPayoutMulTemp : EvtSender2<SymbolConfig, int, REvtSymbolPayoutMulTemp>;
[TypeRegistryItem("使某符号添加某符号\t(SymbolConfig, SymbolConfig)")]
public class SEvtSymbolAddSymbol : EvtSender2<SymbolConfig, SymbolConfig, REvtSymbolAddSymbol>;
[TypeRegistryItem("使某符号消除某符号\t(SymbolConfig, SymbolConfig)")]
public class SEvtSymbolDestroySymbol : EvtSender2<SymbolConfig, SymbolConfig, REvtSymbolDestroySymbol>;
[TypeRegistryItem("使某符号积攒N\t(SymbolConfig, int)")]
public class SEvtSymbolStock : EvtSender2<SymbolConfig, int, REvtSymbolStock>;

public abstract class FilterBase<T>
{
    // [InfoBox("下面的\"且\"可为None, None代表本行的\"且\"逻辑结束")]
    [LabelText("且")] public FilterBase<T>? And;
}
public abstract class FilterSymbolBase : FilterBase<SymbolConfig>;
[TypeRegistryItem("符号为自身")]
public class FilterSymbolIsSelf : FilterSymbolBase;
[TypeRegistryItem("符号在老虎机中出现")]
public class FilterSymbolShown : FilterSymbolBase;
[TypeRegistryItem("符号在角落")]
public class FilterSymbolInCorner : FilterSymbolBase;
[TypeRegistryItem("符号属于指定一个")]
public class FilterSymbolIsOne : FilterSymbolBase
{
    [LabelText("选择单个符号"), Required]public SelectBase<SymbolConfig> OneSymbolSelector = new SelectDirectOneSymbol();
}
[TypeRegistryItem("符号属于指定一组")]
public class FilterSymbolIsOfList : FilterSymbolBase
{
    [LabelText("选择一组符号"), Required]public SelectBase<List<SymbolConfig>> ListSymbolSelector = new SelectDirectSetSymbol();
}
[TypeRegistryItem("N等于")]
public class FilterNEqual : FilterBase<int>
{
    [LabelText("输入目标值"), Required]public SelectBase<int> IntSelector = new SelectDirectInt();
}

public abstract class SelectBase<T>
{
    public override string ToString()
    {
        return GetType().GetAttribute<SelectorAttribute>()?.Text ?? GetType().Name;
    }
}

public abstract class SelectCustomBase<T> : SelectBase<T>;
public abstract class SelectCustomBase<T1, T> : SelectCustomBase<T>;
public abstract class SelectCustomBase<T1, T2, T> : SelectCustomBase<T>;
[TypeRegistryItem("xjbs")]
public class SelectXxx : SelectCustomBase<SymbolConfig, SymbolConfig, int>;
public abstract class SelectNotDirectBase<T> : SelectBase<T>
{
    [LabelText("且满足任一..(空列表/None视为直接满足)"), PropertyOrder(-1)] public List<FilterBase<T>>? FilterOrList = [];
}
public abstract class SelectDirectBase<T> : SelectBase<T>;

public class SelectFromEvtArgNth<T>(int n) : SelectNotDirectBase<T>
{
    [HideInInspector]public int N = n;
    public override string ToString() => $"选择第{N}个事件参数";
}
[Selector("直接Int")]
public class SelectDirectInt : SelectDirectBase<int>
{
    public int Value = 1;
}
public interface ISelectSymbol;
[Selector("符号自身")]
public class SelectSymbolSelf : SelectNotDirectBase<SymbolConfig>, ISelectSymbol;
[Selector("指定一个符号")]
public class SelectDirectOneSymbol : SelectDirectBase<SymbolConfig>, ISelectSymbol
{
    [LabelText("符号"), Required] public SymbolConfig One = null!;
}
[Selector("指定一组符号")]
public class SelectDirectSetSymbol : SelectDirectBase<List<SymbolConfig>>, ISelectSymbol
{
    [LabelText("符号组"), Required] public SymbolConfigSet Set = null!;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SelectorAttribute(string text) : TypeRegistryItemAttribute(text)
{
    public readonly string Text = text;
}

