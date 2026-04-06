using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新物体组", menuName = "NM/物体组")]

public class ItemConfigSet : ConfigMulti<ItemConfigSet>
{
    protected override string PrefixName => "ItemSet";
    [LabelText("0_地块列表")] public List<GridConfig> GridList = [];
    [LabelText("1_棋子列表")] public List<GridConfig> SymbolList = [];
    [LabelText("2_建筑列表")] public List<BuildingConfig> BuildingList = [];
    [LabelText("3_资源列表")] public List<ResourceConfig> ResourceList = [];
}