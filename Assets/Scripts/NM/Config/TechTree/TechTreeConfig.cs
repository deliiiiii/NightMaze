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
    [ReadOnly, NonSerialized, OdinSerialize] public required List<TechNodeConfig> NodeList = [];
    [ReadOnly, NonSerialized, OdinSerialize] public required List<TechLineConfig> LineList = [];
}