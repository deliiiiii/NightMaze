using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

public abstract class ItemDesConditionBase
{
    [Header("且满足.."), HideLabel]
    [SerializeReference, PropertyOrder(9999)] public ItemDesConditionBase? Next;
}
[TypeRegistryItem("若物体{0}满足个数{1}")]
public class ItemDesConditionCollectXItem : ItemDesConditionBase
{
    [Required, SerializeReference, LabelText("{0}: 目标物体"), OnValueChanged(nameof(OnChanged))]
    public ItemSelectorBase ItemSelector = new ItemSelectorTag()
    {
        ItemFilter = new ItemFilterNotSelf()
    };
    [Required, SerializeReference, LabelText("{1}: 满足条件的最少个数")]public IntSelectorBase MinValueSelector = new IntSelectorConst { Value = 1 };
    
    void OnChanged()
    {
        ItemSelector ??= new ItemSelectorTag()
        {
            ItemFilter = new ItemFilterNotSelf()
        };
        MinValueSelector ??= new IntSelectorConst { Value = 1 };
    }
}