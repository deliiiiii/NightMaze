using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace NM.Config;

[CreateAssetMenu(fileName = nameof(ItemTypeResourceMgr), menuName = "NM/Mgr/" + nameof(ItemTypeResourceMgr))]
public class ItemTypeResourceMgr : ConfigSingle<ItemTypeResourceMgr>
{
    [LabelText("通用Tag图标配置")]public Dictionary<TagEntry, DetailTagInfo> ItemDic = [];
    
    [LabelText("地形Tag图标配置")]public Dictionary<TagEntry, DetailTagInfo> GridDic = [];
    [LabelText("棋子Tag图标配置")]public Dictionary<TagEntry, DetailTagInfo> SymbolDic = [];
    [LabelText("资源Tag图标配置")]public Dictionary<TagEntry, DetailTagInfo> ResourceDic = [];
    [LabelText("建筑Tag图标配置")]public Dictionary<TagEntry, DetailTagInfo> BuildingDic = [];
    [LabelText("事件Tag图标配置")]public Dictionary<TagEntry, DetailTagInfo> EventDic = [];

#if UNITY_EDITOR
    void OnValidate()
    {
        RefillAndDeleteDic(ItemDic, Const.Res.Config.ItemTag);
        RefillAndDeleteDic(GridDic, Const.Res.Config.GridTag);
        RefillAndDeleteDic(SymbolDic, Const.Res.Config.SymbolTag);
        RefillAndDeleteDic(ResourceDic, Const.Res.Config.ResourceTag);
        RefillAndDeleteDic(BuildingDic, Const.Res.Config.BuildingTag);
        RefillAndDeleteDic(EventDic, Const.Res.Config.EventTag);
    }
    static void RefillAndDeleteDic(Dictionary<TagEntry, DetailTagInfo> dic, string assetName)
    { 
        var tagList = Editor.MgrEditor.LoadTagMgr(assetName)?.TagList;
        if (tagList == null)
            return;
        var toAddKey = tagList.Where(e => !dic.ContainsKey(e));
        var toRemoveKey = dic.Keys.Where(e => !tagList.Contains(e));
        toRemoveKey.ToList().ForEach(e => dic.Remove(e));
        toAddKey.ForEach(tagEntry => dic.Add(tagEntry, new DetailTagInfo(tagEntry.Tag, Color.white, null!)));
    }
#endif
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