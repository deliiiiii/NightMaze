using System;
using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新资源", menuName = "NM/" + "3_资源")]
public class ResourceConfig : ItemConfigBase<ResourceConfig>
{
    protected override string PrefixName => "Resource";
    public override List<DetailTagInfo> DetailTagInfos =>
        [..base.DetailTagInfos, ..ResourceTag.ToValues().Select(e => Mgr.ResourceDic[e])];
    public override int Order => 3;
    
    [Header("—— 资源配置 ——")]
    [LabelText("资源标签")]public EResourceTag ResourceTag;
}
[Flags]
public enum EResourceTag
{
    [LabelText("作物")]Crops = 1 << 1,
    [LabelText("生物质")]Biomass = 1 << 2,
}