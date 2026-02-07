using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

[CreateAssetMenu(fileName = "NewSymbolSet", menuName = "NM/" + nameof(SymbolConfigSet))]
public class SymbolConfigSet : ConfigMulti<SymbolConfigSet>
{
    protected override string PrefixName => "SymbolSet";
    [Required] public HashSet<SymbolConfig> SymbolSet = [];
}