using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
public abstract record ItemDesResultBase
{
    [Header("若满足条件"), HideLabel]
    [SerializeReference] public ItemDesConditionBase? Condition;
    [Header("成功后执行")]
    [SerializeReference, HideLabel, PropertyOrder(9999)]public ItemDesResultBase? Next;
}
[TypeRegistryItem("使物体{0}的属性{1}加算{2}")][DebuggerStepThrough]
public record ItemDesResultAddXPropX : ItemDesResultBase
{
    [SerializeReference, LabelText("{0}: 目标物体")] public ItemSelectorBase? ItemSelector = new ItemSelectorAtPresentSelf();
    [LabelText("{1}: 属性类型")] public EPropType PropType;
    [SerializeReference, LabelText("{2}: 属性加算数值")] public IntSelectorBase? IntSelector = new IntSelectorConst();

}
[TypeRegistryItem("使物体{0}的属性{1}乘算{2}")][DebuggerStepThrough]
public record ItemDesResultMulXPropX : ItemDesResultBase
{
    [SerializeReference, LabelText("{0}: 目标物体")] public ItemSelectorBase? ItemSelector = new ItemSelectorAtPresentSelf();
    [LabelText("{1}: 属性类型")] public EPropType PropType;
    [SerializeReference, LabelText("{2}: 属性乘算数值")] public IntSelectorBase? IntSelector = new IntSelectorConst();
}

[TypeRegistryItem("在位置{0}生成某一个物体{1}的原型")][DebuggerStepThrough]
public record ItemDesResultSpawnXAtX : ItemDesResultBase
{
    [Header("注：{0}会固定筛选出能放置该物体的坐标")]
    [SerializeReference, LabelText("{0}: 生成位置(可多个)")] public PosSelectorBase? PosSelector = new PosSelectorConst();
    [SerializeReference, LabelText("{1}: 生成物体(无论如何, 最终只会选择一种)")] public ItemSelectorBase? ItemSelector = new ItemSelectorFromConfigCustom();
}

public enum EPropType
{
    [LabelText(Const.Property.Name1)] Prop1 = 1,
    [LabelText(Const.Property.Name2)] Prop2 = 2,
    [LabelText(Const.Property.Name3)] Prop3 = 3,
    [LabelText(Const.Property.NameA1)] PropA1 = 10,
    [LabelText(Const.Property.NameA2)] PropA2 = 11,
}

#region 独特
[TypeRegistryItem("移除物体{0}", "独特")][DebuggerStepThrough]
public record ItemDesResultRemoveItem : ItemDesResultBase
{
    [SerializeReference, LabelText("{0} 指定目标物体(可以为复数个.)")]
    public ItemSelectorBase? ItemSelector = new ItemSelectorFromConfigCustom();
}

[TypeRegistryItem("将物体{0}的词条添加到自身", "独特")][DebuggerStepThrough]
public record ItemDesResultAddItemDesToSelf : ItemDesResultBase
{
    [SerializeReference, LabelText("{0} 指定目标物体")] public ItemSelectorBase? ItemSelector = new ItemSelectorFromResult();
}
#endregion