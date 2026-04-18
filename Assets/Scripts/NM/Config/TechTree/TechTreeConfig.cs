using System;
using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新科技树", menuName = "NM/科技树")]
public class TechTreeConfig : ConfigSingle<TechTreeConfig>
{
    [ReadOnly] public required List<TechNodeConfig> NodeList = [];
    [ReadOnly] public required List<TechLineConfig> LineList = [];
}
public class TechNodeConfig
{
    public required int ID;
    public required string Name;
    [ReadOnly] public required Vector2 Pos;
    public required List<ItemConfig> ToUnLockItems;
    public required Dictionary<EPropType, long> RequireDic;
}
public class TechLineConfig
{
    public required int LeftNodeID;
    public required int LeftPortID;
    public required int RightNodeID;
    public required int RightPortID;
}