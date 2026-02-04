using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace NM;

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
    [LabelText("且事件的参数1满足任一.."), Required, PropertyOrder(-1)] public List<FilterBase<T1>> Filter1OrList = [];
}
public abstract class EvtReceiver2<T1, T2> : EvtReceiverBase
{   
    [LabelText("且事件的参数1满足任一..(空列表视为直接满足)"), Required, PropertyOrder(-2)] public List<FilterBase<T1>> Filter1OrList = [];
    [LabelText("且事件的参数2满足任一..(空列表视为直接满足)"), Required, PropertyOrder(-1)] public List<FilterBase<T2>> Filter2OrList = [];
}
[TypeRegistryItem("旋转后的审视自身\t(SymbolConfig)")]
public class REvtCheckSelf : EvtReceiver1<SymbolConfig>;
[TypeRegistryItem("符号每旋转N次\t(SymbolConfig, Int)")]
public class REvtSymbolEverySpinN : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("符号添加符号时\t(SymbolConfig, SymbolConfig)")]
public class REvtSymbolAddSymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
[TypeRegistryItem("符号消除符号时\t(SymbolConfig, SymbolConfig)")]
public class REvtSymbolDestroySymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
[TypeRegistryItem("符号移除符号时\t(SymbolConfig, SymbolConfig)")]
public class REvtSymbolRemoveSymbol : EvtReceiver2<SymbolConfig, SymbolConfig>;
[TypeRegistryItem("符号临时加成时\t(SymbolConfig, int)")]
public class REvtSymbolPayoutAddTemp : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("符号永久加成时\t(SymbolConfig, int)")]
public class REvtSymbolPayoutAddPermanent : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("符号积攒X时\t(SymbolConfig, int)")]
public class REvtSymbolStock : EvtReceiver2<SymbolConfig, int>;
[TypeRegistryItem("玩家移除符号时\t(SymbolConfig)")]
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
        var ret = typeof(SelectBase<T>).GetSubTypes()
            .Select(t =>
            {
                var instance = (SelectBase<T>)Activator.CreateInstance(t);
                var label = t.GetAttribute<SelectorAttribute>()?.Text ?? t.Name;
                return new ValueDropdownItem<SelectBase<T>>() { Text = label, Value = instance };
            })
            .ToList();
        GetSourceTypes(property).ForEach(kv =>
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
[TypeRegistryItem("使符号临时加成\t(SymbolConfig, Int)")]
public class SEvtSymbolPayoutAddTemp : EvtSender2<SymbolConfig, int, REvtSymbolPayoutAddTemp>;


public abstract class FilterBase<T>
{
    [InfoBox("下面的\"且\"可为None, None代表本行的\"且\"逻辑结束")]
    [LabelText("且")] public FilterBase<T>? And;
}
public abstract class FilterSymbolBase : FilterBase<SymbolConfig>;
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
    [LabelText("选择符号组"), Required]public SelectBase<List<SymbolConfig>> ListSymbolSelector = new SelectDirectSetSymbol();
}

public abstract class SelectBase<T>
{
    public override string ToString()
    {
        return GetType().GetAttribute<SelectorAttribute>()?.Text ?? GetType().Name;
    }
}
public class SelectFromEvtArgNth<T>(int n) : SelectBase<T>
{
    [HideInInspector]public int N = n;
    public override string ToString() => $"选择第{N}个事件参数";
}
// public class SelectSymbolFromEvtArg : SelectFromEvtArg<SymbolConfig>;
// public class SelectIntFromEvtArg : SelectFromEvtArg<int>;
[Selector("直接Int")]
public class SelectDirectInt : SelectBase<int>
{
    public int Value = 1;
}

[Selector("老虎机中已出现符号")]
public class SelectSymbolShown : SelectBase<List<SymbolConfig>>;
[Selector("符号自身")]
public class SelectSymbolSelf : SelectBase<SymbolConfig>;
[Selector("指定一个符号")]
public class SelectDirectOneSymbol : SelectBase<SymbolConfig>
{
    [LabelText("符号"), Required] public SymbolConfig One = null!;
}
[Selector("指定一组符号")]
public class SelectDirectSetSymbol : SelectBase<List<SymbolConfig>>
{
    [LabelText("符号组"), Required] public SymbolConfigSet Set = null!;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SelectorAttribute(string text) : Attribute
{
    public readonly string Text = text;
}