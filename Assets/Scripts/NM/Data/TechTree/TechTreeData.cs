using System;
using System.Collections.Generic;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.OdinInspector;

namespace NM.Data;
[Serializable]
public partial class TechTreeData
{
    public static TechTreeConfig Config => field ??= ConfigLoader.Acquire<TechTreeConfig>();

    [JsonProperty(IsReference = false, ItemIsReference = false)]
    public List<TechNodeData> NodeList = [];
    [JsonProperty, ShowInInspector, EvtChanged] public partial int? CurID { get; set; }
    public bool IsItemLocked(ItemConfig itemConfig)
        => NodeList.Any(node => !node.Unlocked &&
            (Config.NodeList.FirstOrDefault(n => n.ID == node.ID)
                ?.ToUnLockItems?.Contains(itemConfig) 
                ?? true));
    public List<(int Distance, List<TechNodeData> Nodes)> GetCurNodesGroupByDis()
    {
        if (!CurID.HasValue) 
            return [];
        var startNode = NodeList
            .FirstOrDefault(n => n.ID == CurID.Value);
        if (startNode == null) 
            return [];
        var lineConfigs = Config.LineList;
        Dictionary<TechNodeData, int> distanceDict = new()
        {
            [startNode] = 0  
        };
        Queue<(TechNodeData Node, int Distance)> queue = [];
        queue.Enqueue((startNode, 0));
        while (queue.Count > 0)
        {
            var (currentNode, currentDist) = queue.Dequeue();
            var predecessorIDs = lineConfigs
                .Where(l => l.RightNodeID == currentNode.ID)
                .Select(l => l.LeftNodeID);
            foreach (var predID in predecessorIDs)
            {
                var predNode = NodeList.FirstOrDefault(n => n.ID == predID);
                if (predNode == null) 
                    continue;
                if (distanceDict.TryAdd(predNode, currentDist + 1))
                    queue.Enqueue((predNode, currentDist + 1));
            }
        }
        return[..
            from kvp in distanceDict
            where !kvp.Key.Unlocked
            orderby kvp.Value descending, kvp.Key.Config.Pos.y descending, kvp.Key.Config.Pos.x
            group kvp by kvp.Value into g
            select (Distance: g.Key, Nodes: g.Select(kvp => kvp.Key).ToList())];
    }
    
    

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
    [JsonConstructor] TechNodeData() {CarValueDic = [];}
    public TechNodeData(TechNodeConfig config) 
    {
        ID = config.ID;
        Config = config;
        CarValueDic = EPropType.GetValues().ToDictionary(propType => propType, _ => 0L);
    }
    public int ID;
    [JsonProperty(IsReference = false, ItemIsReference = false)] public Dictionary<EPropType, long> CarValueDic;
    [field: JsonIgnore]public TechNodeConfig Config => field ??= TechTreeData.Config.NodeList.First(c => c.ID == ID);

    public bool Unlocked => Config.RequireDic?.All(line =>
        line.Value <= CarValueDic.GetValueOrDefault(line.Key, 0)) ?? true;
}