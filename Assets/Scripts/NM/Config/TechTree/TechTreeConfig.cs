using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新科技树", menuName = "NM/科技树")]
public class TechTreeConfig : ConfigSingle<TechTreeConfig>
{
    [ReadOnly] public required List<TechNodeConfig> NodeList = [];
    [ReadOnly] public required List<TechLineConfig> LineList = [];
}