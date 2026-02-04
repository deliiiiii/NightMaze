using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "NM/" + nameof(SymbolConfig))]
public class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    [LabelText("稀有度")] public ERarity Rarity;
    [LabelText("白值")]public int Payout = 1;
    [LabelText("旋转后, 自发根据事件的行为"), Required] public List<IEvtReceiver> EvtList = [];
}