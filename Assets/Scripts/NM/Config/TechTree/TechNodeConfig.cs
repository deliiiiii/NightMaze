using System;
using System.Collections.Generic;
using General.Dictionary;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[Serializable]
public class TechNodeConfig
{
    public required int ID;
    public required string Name;
    [LabelText("位置"), ReadOnly]
    public required Vector2 Pos;
    [LabelText("解锁物体")]
    public List<ItemConfig?>? ToUnLockItems = [];
    [LabelText("属性需求")]
    public SerializableDictionary<EPropType, long>? RequireDic = [];
}