using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新物体组", menuName = "NM/物体组")]

public class ItemConfigSet : ConfigMulti<ItemConfigSet>
{
    protected override string PrefixName => "ItemSet";
    [LabelText("0_地块列表(不可重复)"), ValidateInput(nameof(CheckGrid), "不可留空值")] public HashSet<GridConfig> GridList = [];
    [LabelText("1_棋子列表(不可重复)"), ValidateInput(nameof(CheckSymbol), "不可留空值")] public HashSet<SymbolConfig> SymbolList = [];
    [LabelText("2_建筑列表(不可重复)"), ValidateInput(nameof(CheckBuilding), "不可留空值")] public HashSet<BuildingConfig> BuildingList = [];
    [LabelText("3_资源列表(不可重复)"), ValidateInput(nameof(CheckResource), "不可留空值")] public HashSet<ResourceConfig> ResourceList = [];

    bool CheckGrid() => GridList.All(x => x != null);
    bool CheckSymbol() => SymbolList.All(x => x != null);
    bool CheckBuilding() => BuildingList.All(x => x != null);
    bool CheckResource() => ResourceList.All(x => x != null);
}