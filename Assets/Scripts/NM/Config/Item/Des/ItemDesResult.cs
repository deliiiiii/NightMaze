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
    [SerializeReference, LabelText("{0}: 目标物体")] public ItemSelectorBase ItemSelector = new ItemSelectorSelf();
    [LabelText("{1}: 属性类型")] public EPropType PropType;
    [SerializeReference, LabelText("{2}: 属性加算数值")] public IntSelectorBase IntSelector = new IntSelectorConst();

}
[TypeRegistryItem("使物体{0}的属性{1}乘算{2}")][DebuggerStepThrough]
public record ItemDesResultMulXPropX : ItemDesResultBase
{
    [SerializeReference, LabelText("{0}: 目标物体")] public ItemSelectorBase ItemSelector = new ItemSelectorSelf();
    [LabelText("{1}: 属性类型")] public EPropType PropType;
    [SerializeReference, LabelText("{2}: 属性乘算数值")] public IntSelectorBase IntSelector = new IntSelectorConst();
}

[TypeRegistryItem("在位置{0}生成物体{1}")][DebuggerStepThrough]
public record ItemDesResultSpawnXAtX : ItemDesResultBase
{
    [Header("注：{0}会固定筛选出能放置该物体的坐标")]
    [SerializeReference, LabelText("{0}: 生成位置 (数量多于1的部分暂时舍弃.)")] public PosSelectorBase PosSelector = new PosSelectorConst();
    [SerializeReference, LabelText("{1}: 生成物体 (只根据配置生成原型, 数量多于1的部分暂时舍弃.)")] public ItemSelectorBase ItemSelector = new ItemSelectorItem();
}

public enum EPropType
{
    [LabelText(Const.Property.Name1)] Prop1,
    [LabelText(Const.Property.Name2)] Prop2,
    [LabelText(Const.Property.Name3)] Prop3,
}

#region 独特
[TypeRegistryItem("移除物体{0}", "独特")][DebuggerStepThrough]
public record ItemDesResultRemoveItem : ItemDesResultBase
{
    [SerializeReference, LabelText("{0} 指定目标物体 (完全可以为复数个.)")]
    public ItemSelectorBase ItemSelector = new ItemSelectorTag();
}

[TypeRegistryItem("将物体{0}的词条添加到自身", "独特")][DebuggerStepThrough]
public record ItemDesResultAddItemDesToSelf : ItemDesResultBase
{
    [SerializeReference, LabelText("{0} 指定目标物体")] public ItemSelectorBase ItemSelector = new ItemSelectorFromResult();
}
#endregion