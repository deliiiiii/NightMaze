using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新物体组", menuName = "NM/物体组")]

public class ItemConfigSet : ConfigMulti<ItemConfigSet>
{
    public override string PrefixName => "ItemSet";
    [LabelText("物体列表(不可重复)"), ValidateInput(nameof(CheckItem), "不可留空值")] public HashSet<ItemConfig> ItemList = [];
    bool CheckItem() => ItemList.All(x => x != null);
}