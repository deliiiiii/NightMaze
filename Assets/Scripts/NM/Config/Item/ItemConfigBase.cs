using System;
using System.Collections.Generic;
using GeneralPreview;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Config;
public abstract class ItemConfigBase<T> : ConfigMulti<T>, IItemConfig
    where T : ItemConfigBase<T>, new()
{
    // ReSharper disable once StaticMemberInGenericType
    protected static ItemTypeResourceMgr Mgr => field ??= RefPoolSingle<ItemTypeResourceMgr>.Acquire();
    [Header("—— 通用配置 ——")]
    [LabelText("风味文本")] public string FlavorDes = string.Empty;
    [LabelText("稀有度")] public ERarity Rarity;
    [Required, LabelText("占据位置")] public ItemPos Pos = new ItemPosRectangle();
    [Required, LabelText("通用标签")] public EItemTag Tag;
    [Required, LabelText("词条列表")] public List<ItemDesConfig> DesList = [];
    public virtual List<DetailTagInfo> DetailTagInfos => Tag.ToValues().Select(t => Mgr.ItemDic[t]).ToList();
    public virtual int Order => 0;
    
    string IItemConfig.Name => Name;
    string IItemConfig.PrefixName => PrefixName;
    string IItemConfig.FlavorDes => FlavorDes;
    ERarity IItemConfig.Rarity => Rarity;
    ItemPos IItemConfig.Pos => Pos;
    EItemTag IItemConfig.Tag => Tag;
    List<ItemDesConfig> IItemConfig.DesList => DesList;
    List<DetailTagInfo> IItemConfig.DetailTagInfos => DetailTagInfos;
    int IItemConfig.Order => Order;
}

public interface IItemConfig
{
    string Name { get; }
    string PrefixName { get; }
    string FlavorDes { get; }
    ERarity Rarity { get; }
    ItemPos Pos { get; }
    EItemTag Tag { get; }
    List<ItemDesConfig> DesList { get; }
    List<DetailTagInfo> DetailTagInfos { get; }
    int Order { get; }
}
public enum ERarity
{
    [LabelText("普通")] Common,
    [LabelText("罕见")] UnCommon,
    [LabelText("稀有")] Rare,
    [LabelText("非常稀有")] VeryRare,
}
[Flags]
public enum EItemTag
{
    [LabelText("自然")]Natural = 1 << 1,
    [LabelText("异象")]Anomaly = 1 << 2,
}

public abstract class ItemPos
{
    public abstract IEnumerable<Vector2Int> GetDeltaPos();
}
[PublicAPI, TypeRegistryItem("矩形")]
public class ItemPosRectangle : ItemPos
{
    [LabelText("宽")]public int Length = 1;
    [LabelText("高")]public int Height = 1;
    public override IEnumerable<Vector2Int> GetDeltaPos()
    {
        return from i in Enumerable.Range(0, Length)
                from j in Enumerable.Range(0, Height)
                select new Vector2Int(i, j);
    }
}
[PublicAPI, TypeRegistryItem("自定义")]
public class ItemPosCustom : ItemPos
{
    [LabelText("相对坐标列表"), OnValueChanged(nameof(OnChanged))]public List<Vector2Int> DeltaPosList = [new(0,0)];
    public override IEnumerable<Vector2Int> GetDeltaPos() => DeltaPosList;

    void OnChanged()
    {
        DeltaPosList ??= [];
    }
}