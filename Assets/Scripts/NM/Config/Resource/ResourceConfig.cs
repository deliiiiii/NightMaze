using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;
[UnityEngine.CreateAssetMenu(fileName = "New Resource", menuName = "NM/" + nameof(ResourceConfig))]

public class ResourceConfig : ConfigMulti<ResourceConfig>
{
    protected override string PrefixName => "Resource";
    [LabelText("标签")]public EItemType Type;
}