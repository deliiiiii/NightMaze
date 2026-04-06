using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;

// [UnityEngine.CreateAssetMenu(fileName = "新棋子组", menuName = "NM/棋子组")]
public class SymbolConfigSet : ConfigMulti<SymbolConfigSet>
{
    protected override string PrefixName => "SymbolSet";
    [Required] public HashSet<SymbolConfig> SymbolSet = [];
}