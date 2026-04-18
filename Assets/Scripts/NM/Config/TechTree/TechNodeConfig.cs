using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
public class TechNodeConfig
{
    public required int ID;
    public required string Name;
    [ReadOnly] public required Vector2 Pos;
    public List<ItemConfig?>? ToUnLockItems = [];
    public Dictionary<EPropType, long>? RequireDic = [];
}