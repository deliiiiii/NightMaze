using System;
using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace NM.Config;

[CreateAssetMenu(fileName = nameof(ItemTypeResourceMgr), menuName = "NM/Mgr/" + nameof(ItemTypeResourceMgr))]
public class ItemTypeResourceMgr : ConfigSingle<ItemTypeResourceMgr>
{
    [LabelText("通用Tag图标配置")]public Dictionary<EItemTag, DetailTagInfo> ItemDic = [];
    
    [LabelText("地形Tag图标配置")]public Dictionary<EGridTag, DetailTagInfo> GridDic = [];
    [LabelText("棋子Tag图标配置")]public Dictionary<ESymbolTag, DetailTagInfo> SymbolDic = [];
    [LabelText("资源Tag图标配置")]public Dictionary<EResourceTag, DetailTagInfo> ResourceDic = [];
    [LabelText("建筑Tag图标配置")]public Dictionary<EBuildingTag, DetailTagInfo> BuildingDic = [];
    [LabelText("事件Tag图标配置")]public Dictionary<EEventTag, DetailTagInfo> EventDic = [];

    void OnEnable()
    {
        RefillAndDeleteDic(ItemDic);
        RefillAndDeleteDic(GridDic);
        RefillAndDeleteDic(SymbolDic);
        RefillAndDeleteDic(ResourceDic);
        RefillAndDeleteDic(BuildingDic);
        RefillAndDeleteDic(EventDic);
    }
    static void RefillAndDeleteDic<TEnum>(Dictionary<TEnum, DetailTagInfo> dic) where TEnum : Enum
    {
        var enums = GeneralPreview.EnumExt.GetValues<TEnum>().ToList();
        var toAddKey = enums.Where(e => !dic.ContainsKey(e));
        var toRemoveKey = dic.Keys.Where(e => !enums.Contains(e));
        toRemoveKey.ToList().ForEach(e => dic.Remove(e));
        toAddKey.ForEach(e => dic.Add(e, new DetailTagInfo(e.GetLabelText(), Color.white, null!)));
    }
}

public class DetailInfo
{
    public required string Type;
    public required string Name;
    public required List<DetailTagInfo> TagInfoList;
    public required string Detail;
    public required List<string> InSpinLineList;
}

public struct DetailTagInfo(string tagName, Color backColor, Sprite icon)
{
    [ReadOnly][HideInInspector] public string TagName = tagName;
    [LabelText("背景颜色")]public Color BackColor = backColor;
    [LabelText("图标")]public Sprite Icon = icon;
}