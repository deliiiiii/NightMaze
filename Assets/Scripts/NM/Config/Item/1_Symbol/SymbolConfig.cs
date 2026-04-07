using System;
using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = "新棋子", menuName = "NM/" + "1_棋子")]
public partial class SymbolConfig : ItemConfigBase<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    public override List<DetailTagInfo> DetailTagInfos =>
        [..base.DetailTagInfos, ..SymbolTag.ToValues().Select(e => Mgr.SymbolDic[e])];
    public override int Order => 1;
    [Header("—— 棋子配置 ——")]

    [LabelText("棋子标签")]public ESymbolTag SymbolTag;
    [LabelText($"属性1 {Const.Property.Name1}白值")] public int Prop1;
    [LabelText($"属性2 {Const.Property.Name2}白值")] public int Prop2;
    [LabelText($"属性3 {Const.Property.Name3}白值")] public int Prop3;
}

[Flags]
public enum ESymbolTag
{
    [LabelText("人类")]People    = 1 << 1,
    [LabelText("机械")]Mechanics = 1 << 2,
}