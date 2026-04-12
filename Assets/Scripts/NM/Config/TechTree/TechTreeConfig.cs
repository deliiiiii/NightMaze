using System;
using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新科技树", menuName = "NM/科技树")]
public class TechTreeConfig : ConfigSingle<TechTreeConfig>
{
    [ReadOnly] public required List<TechNodeConfig> NodeList;
    [ReadOnly] public required List<TechLineConfig> LineList;
}

[Serializable]
public class TechNodeConfig
{
    public required int ID;
    public required string Name;
    [ReadOnly] public required Vector2 Pos;
    public required List<IItemConfig> ToLockItems;
    public required List<TechRequireLine> RequireLineList;
}

[Serializable]
public class TechLineConfig
{
    public required int LeftNodeID;
    public required int LeftPortID;
    public required int RightNodeID;
    public required int RightPortID;
}

[Serializable]
public class TechRequireLine
{
    public required EPropType PropType;
    public int Value;
}