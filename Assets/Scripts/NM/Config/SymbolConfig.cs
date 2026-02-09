using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;

[UnityEngine.CreateAssetMenu(fileName = "NewSymbol", menuName = "NM/" + nameof(SymbolConfig))]
public class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    [LabelText("稀有度")] public ERarity Rarity;
    [LabelText("白值")]public int Payout = 1;
    [InfoBox("下面的列表元素各自有各自的触发条件, 可触发任意多个, 多个同时触发时按列表顺序执行")]
    [LabelText("旋转: 事件列表")]
    public List<EvtReceiverBase>? EvtList = [];
}