using GeneralPreview;

namespace NM.Config;
[UnityEngine.CreateAssetMenu(fileName = "New Resource", menuName = "NM/" + nameof(ResourceConfig))]

public class ResourceConfig : ConfigMulti<ResourceConfig>
{
    protected override string PrefixName => "Resource";
}