using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

public abstract record ItemDesConditionBase
{
    [SerializeReference, LabelText("且满足.."), PropertyOrder(9999)] public ItemDesConditionBase? Next;
}
[TypeRegistryItem("若物体{0}满足个数{1}")]
public record ItemDesConditionCollectXItem : ItemDesConditionBase
{
    [Required, SerializeReference, LabelText("{0}: 目标物体"), OnValueChanged(nameof(OnChanged))]
    public ItemSelectorBase ItemSelector = new ItemSelectorFromConfigCustom();
    [Required, SerializeReference, LabelText("{1}: 满足条件的最少个数")]public IntSelectorBase MinValueSelector = new IntSelectorConst { Value = 1 };
    
    void OnChanged()
    {
        ItemSelector ??= new ItemSelectorFromConfigCustom();
        MinValueSelector ??= new IntSelectorConst { Value = 1 };
    }
}

[TypeRegistryItem("恒为false")]
public record ItemDesConditionAlwaysFalse : ItemDesConditionBase;