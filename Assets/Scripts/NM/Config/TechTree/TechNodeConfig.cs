using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
public class TechNodeConfig
{
    public required int ID;
    public required string Name;
    [LabelText("位置"), ReadOnly] public required Vector2 Pos;
    [LabelText("解锁物体")]public List<ItemConfig?>? ToUnLockItems = [];
    [LabelText("属性需求")]public Dictionary<EPropType, long>? RequireDic = [];
}