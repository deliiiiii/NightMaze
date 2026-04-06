using System;
using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新建筑", menuName = "NM/" + "2_建筑")]
public class BuildingConfig : ItemConfigBase<BuildingConfig>
{
    protected override string PrefixName => "Building";
    public override List<DetailTagInfo> DetailTagInfos =>
        [..base.DetailTagInfos, ..BuildingTag.ToValues().Select(e => Mgr.BuildingDic[e])];

    [Header("—— 建筑配置 ——")]
    [LabelText("建筑标签")]public EBuildingTag BuildingTag;
    [LabelText($"花费{Const.Property.Name1}")]public int Prop1Cost;
    [LabelText($"花费{Const.Property.Name2}")]public int Prop2Cost;
    [LabelText($"花费{Const.Property.Name3}")]public int Prop3Cost;
}
[Flags]
public enum EBuildingTag
{
    [LabelText("庇护所")]Shelter = 1 << 1,
    [LabelText("科研")]Science = 1 << 2,
}