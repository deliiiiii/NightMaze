using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;
public abstract class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    [LabelText("稀有度")] public ERarity Rarity;
    [LabelText("白值")]public int Payout = 1;
}