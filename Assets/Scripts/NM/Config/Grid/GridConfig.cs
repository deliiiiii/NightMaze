using System;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;
[UnityEngine.CreateAssetMenu(fileName = "New Grid", menuName = "NM/" + nameof(GridConfig))]
public class GridConfig : ConfigMulti<GridConfig>
{
    protected override string PrefixName => "Grid";
    [LabelText("标签")]public EItemType Type;
}

