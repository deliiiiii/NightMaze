using System;
using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新地形", menuName = "NM/" + "0_地形")]
public class GridConfig : ItemConfigBase<GridConfig>
{
    protected override string PrefixName => "Grid";

    public override List<DetailTagInfo> DetailTagInfos =>
        [..base.DetailTagInfos, ..GridTag.ToValues().Select(e => Mgr.GridDic[e])];

    [Header("—— 地形配置 ——")]
    [LabelText("地形标签")]public EGridTag GridTag;
}
[Flags]
public enum EGridTag
{
    [LabelText("肥沃")]Rich = 1 << 1,
}