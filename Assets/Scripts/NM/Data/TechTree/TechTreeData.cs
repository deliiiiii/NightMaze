using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;
[Serializable]
public class TechTreeData
{
    public static TechTreeConfig Config => field ??= ConfigLoader.Acquire<TechTreeConfig>();

    [JsonProperty(IsReference = false, ItemIsReference = false)]
    public List<TechNodeData> NodeList = [];
    public bool IsItemLocked(ItemConfig itemConfig)
        => NodeList.Any(node => !node.Unlocked &&
            (Config.NodeList.FirstOrDefault(n => n.ID == node.ID)
                ?.ToUnLockItems.Contains(itemConfig) 
                ?? true));

    public void OnLoad()
    {
        // 移除NodeList中不在Config.NodeList中的节点
        var configNodeIDs = Config.NodeList.Select(n => n.ID).ToHashSet();
        NodeList.RemoveAll(node => !configNodeIDs.Contains(node.ID));
        // 添加Config.NodeList中不在NodeList中的节点
        var nodeIDs = NodeList.Select(n => n.ID).ToHashSet();
        foreach (var configNode in Config.NodeList.Where(configNode => !nodeIDs.Contains(configNode.ID)))
        {
            NodeList.Add(new TechNodeData(configNode));
        }
    }
}

[Serializable]
public class TechNodeData
{
    [JsonConstructor] TechNodeData() {}
    public TechNodeData(TechNodeConfig config) 
    {
        ID = config.ID;
        Config = config;
    }
    public int ID;
    public TechNodeConfig Config => field ??= TechTreeData.Config.NodeList.First(c => c.ID == ID);
    [JsonProperty(IsReference = false, ItemIsReference = false)]
    public Dictionary<EPropType, long> CarValueDic = 
        EPropType.GetValues().ToDictionary(propType => propType, _ => 0L);
    
    public bool Unlocked => Config?.RequireDic.All(line => 
        line.Value >= CarValueDic.GetValueOrDefault(line.Key, 0)) ?? true;
}