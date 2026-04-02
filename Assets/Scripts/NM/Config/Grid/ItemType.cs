using System;
using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace NM.Config;

[Flags]
public enum EItemType
{
    [LabelText("Tag1")]SomeType1  = 1 << 1,
    [LabelText("Tag2")]SomeType2  = 1 << 2,
    [LabelText("Tag3")]SomeType3  = 1 << 3,
    [LabelText("Tag4")]SomeType4  = 1 << 4,
} 

[CreateAssetMenu(fileName = "ItemTypeResourceMgr", menuName = "NM/Mgr/" + nameof(ItemTypeResourceMgr))]
public class ItemTypeResourceMgr : ConfigSingle<ItemTypeResourceMgr>
{
    public Dictionary<EItemType, (Color backColor, Sprite icon)> Dic = [];

    [Button]
    void OnEnable()
    {
        var enums = EItemType.GetValues().ToList();
        var toAddKey = enums
            .Where(e => !Dic.ContainsKey(e));
        var toRemoveKey = Dic.Keys.Where(e => !enums.Contains(e));
        toRemoveKey.ForEach(e => Dic.Remove(e));
        toAddKey.ForEach(e => Dic.Add(e, (Color.white, null!)));
    }
    
}