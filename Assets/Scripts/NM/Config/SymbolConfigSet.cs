using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;

[UnityEngine.CreateAssetMenu(fileName = "NewSymbolSet", menuName = "NM/" + nameof(SymbolConfigSet))]
public class SymbolConfigSet : ConfigMulti<SymbolConfigSet>
{
    protected override string PrefixName => "SymbolSet";
    [Required] public HashSet<SymbolConfig> SymbolSet = [];
}