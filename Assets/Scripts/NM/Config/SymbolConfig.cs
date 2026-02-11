using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;

[UnityEngine.CreateAssetMenu(fileName = "NewSymbol", menuName = "NM/" + nameof(SymbolConfig))]
public class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    [LabelText("稀有度")] public ERarity Rarity;
    [LabelText("白值")]public int Payout = 1;
}