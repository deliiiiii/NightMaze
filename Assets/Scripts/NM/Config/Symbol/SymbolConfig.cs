using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;
[UnityEngine.CreateAssetMenu(fileName = "New Symbol", menuName = "NM/" + nameof(SymbolConfig))]
public partial class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    [LabelText("稀有度")] public ERarity Rarity;

    [LabelText($"属性1 {Const.Property.Name1}白值")] public int Prop1;
    [LabelText($"属性2 {Const.Property.Name2}白值")] public int Prop2;
    [LabelText($"属性3 {Const.Property.Name3}白值")] public int Prop3;

    [LabelText("词条列表")]public List<EvtConfig> EvtList = [];
}