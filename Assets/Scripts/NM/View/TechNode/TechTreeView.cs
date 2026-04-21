using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
using NM.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2Int = UnityEngine.Vector2Int;

// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;
[ExecuteAlways]
public class TechTreeView : ViewBase
{
    [SerializeField] TechNodeView pfbTechNodeView;
    [SerializeField] TechLineView pfbTechLineView;
    [SerializeField, HideInInspector] Trs trsTechNode;
    [SerializeField, HideInInspector] Trs trsTechLine;
    [NonSerialized] List<TechNodeView> techNodeList = [];
    [NonSerialized] List<TechLineView> techLineList = [];
    TechTreeConfig ConfigRT => field ??= ConfigLoader.Acquire<TechTreeConfig>();
    
    UniEvt<GamePlaying.EvtEndSpin> OnEndSpin => new()
    {
        Invoke = (evt, ct) =>
        {
            techNodeList.ForEach(node => node.OnCreateView(node.Data, curId: PlayViewIns.Data.TechTreeData.CurID));
            return UniTask.CompletedTask;
        },
        Des = "结束转动后刷新科技树显示",
    };
    public void LoadFromConfigRT()
    {
        trsTechNode.GetChildren().ForEach(n => Destroy(n.gameObject));
        techNodeList.Clear();
        trsTechLine.GetChildren().ForEach(l => Destroy(l.gameObject));
        techLineList.Clear();
        ConfigRT.NodeList.ForEach(CreateNodeRT);
        ConfigRT.LineList.ForEach(CreateLineRT);
        
        gameObject.SetActiveTrue();
    }
    
    
    void CreateNodeRT(TechNodeConfig nodeConfig)
    {
        var ins = Instantiate(pfbTechNodeView, trsTechNode);
        ins.transform.position = new Vector3(nodeConfig.Pos.x, nodeConfig.Pos.y, 0);
        var techTreeData = PlayViewIns.Data.TechTreeData;
        var loadedNode = techTreeData.NodeList.FirstOrDefault(n => n.ID == nodeConfig.ID) 
                         ?? new TechNodeData(nodeConfig);
        ins.OnCreateView(loadedNode, curId: techTreeData.CurID);
        ins.SetActiveTrue();
        techNodeList.Add(ins);
    }

    void CreateLineRT(TechLineConfig lineConfig)
    {
        var ins = Instantiate(pfbTechLineView, trsTechLine);
        ins.Left = techNodeList.First(n => n.Data.Config.ID == lineConfig.LeftNodeID);
        ins.LeftOutPort = lineConfig.LeftPortID;
        ins.Right = techNodeList.First(n => n.Data.Config.ID == lineConfig.RightNodeID);
        ins.RightInPort = lineConfig.RightPortID;
        ins.OnCreate();
        ins.SetActiveTrue();
        techLineList.Add(ins);
    }
    
#if UNITY_EDITOR
    void OnEnable()
    {
        // MyDebug.Log($"{nameof(TechTreeView)} OnEnable()");
        UnityEditor.EditorApplication.update -= OnEditorUpdate;
        UnityEditor.EditorApplication.update += OnEditorUpdate;
        UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        // OnEndEdit();
    }
    [Header("开始/结束编辑")] 
    [SerializeField, LabelText("科技树配置资产")] 
    TechTreeConfig treeConfig;
    [LabelText("正在编辑"), ReadOnly, PropertyOrder(10)]
    public bool Editing;
    bool IsEditing => Editing && !IsRunning;
    bool NotEditing => !Editing && !IsRunning;
    bool IsRunning => Application.isPlaying;
    [Button, HideIf(nameof(IsRunning)), EnableIf(nameof(NotEditing)), PropertyOrder(20)]
    void StartEdit()
    {
        Editing = true;
        OnHierarchyChanged();
        LockLayer(Const.Layer.TechUI);
        UnlockLayer(Const.Layer.TechUIHandle);
        UnityEditor.Tools.pivotRotation = UnityEditor.PivotRotation.Global;
        UnityEditor.EditorSnapSettings.gridSnapEnabled = true;
        UnityEditor.EditorSnapSettings.gridSize = new Vector3(50, 50, 0);
        UnityEditor.SceneView.lastActiveSceneView.pivot = transform.position;
        UnityEditor.Selection.activeGameObject = null;
        techNodeList.ForEach(t => t.OnStartEdit());
        techLineList.ForEach(t => t.OnStartEdit());
        UnityEditor.Undo.IncrementCurrentGroup();
    }
    [Button, HideIf(nameof(IsRunning)), EnableIf(nameof(IsEditing)), PropertyOrder(30)]
    void EndEdit()
    {
        var overlapNodes = GetOverlapNodes();
        if (overlapNodes.Any())
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "重叠 !", 
                $"检测到 {overlapNodes.Count} 个节点位置几乎重叠, 强制退回到编辑模式(将自动选中第一个重叠的物体).", 
                "返回"
            );
            UnityEditor.Selection.activeGameObject = overlapNodes.First().gameObject;
            return;
        }
        if (CheckHasRepeatID("结束编辑"))
            return;
        OnEndEdit();
        SaveToConfig();
    }
    void OnEndEdit()
    {
        OnHierarchyChanged();
        LockLayer(Const.Layer.TechUI);
        LockLayer(Const.Layer.TechUIHandle);
        UnityEditor.Selection.activeGameObject = null;
        techNodeList.ForEach(t => t.OnEndEdit());
        techLineList.ForEach(t => t.OnEndEdit());
        Editing = false;
    }
    void SaveToConfig()
    {
        treeConfig.NodeList = techNodeList.Select(n => n.ConfigInEditor).ToList();
        treeConfig.LineList = techLineList.Select(l => new TechLineConfig
        {
            LeftNodeID = l.Left.ConfigInEditor.ID,
            LeftPortID = l.LeftOutPort,
            RightNodeID = l.Right.ConfigInEditor.ID,
            RightPortID = l.RightInPort,
        }).ToList();
        UnityEditor.EditorUtility.SetDirty(treeConfig);
    }
    readonly Dictionary<GO, ITechObj> goDic = [];
    readonly List<ITechObj> lastSelected = [];
    double tickInterval = 0.1;
    double lastTickTime;
    void OnEditorUpdate()
    {
        if (!Editing || UnityEditor.EditorApplication.isPlaying)
            return;
        var curTime = UnityEditor.EditorApplication.timeSinceStartup;
        if (curTime - lastTickTime < tickInterval)
            return;
        lastTickTime = curTime;
        lastSelected.Where(n => n != null).ForEach(t => t.OnDeSelect());
        lastSelected.Clear();
        UnityEditor.Selection.gameObjects.ForEach(g =>
        {
            if (!goDic.TryGetValue(g, out var t))
            {
                t = g.GetComponent<ITechObj>();
            }
            if (t != null)
            {
                t.OnSelect();
                goDic[g] = t;
                lastSelected.Add(t);
            }
        });
        foreach (var node in techNodeList.Where(n => n != null))
        { 
            node.ConfigInEditor.Pos = node.transform.position;
            node.OnCreate();
            UnityEditor.EditorUtility.SetDirty(node);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(node);
        }
        foreach (var line in techLineList.Where(l => l != null))
        {
            line.OnCreate();
            UnityEditor.EditorUtility.SetDirty(line);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(line);
        }
        curNodeConfig = CurSelectedNode?.ConfigInEditor;
        UnityEditor.SceneView.RepaintAll();
        UnityEditor.EditorUtility.SetDirty(this);
    } 
    void OnHierarchyChanged()
    {
        if (UnityEditor.EditorApplication.isPlaying || !Editing)
            return;
        techNodeList = trsTechNode.GetComponentsInChildren<TechNodeView>().ToList();
        trsTechLine.GetComponentsInChildren<TechLineView>()
            .Where(line => line.Left == null || line.Right == null)
            .ToList()
            .ForEach(line => UnityEditor.Undo.DestroyObjectImmediate(line.gameObject));

        techLineList = trsTechLine.GetComponentsInChildren<TechLineView>().ToList();
    }
    void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        switch (state)
        {
            case UnityEditor.PlayModeStateChange.EnteredEditMode:
                OnEndEdit();
                break;
            case UnityEditor.PlayModeStateChange.ExitingEditMode when Editing:
                UnityEditor.EditorApplication.isPlaying = false;
                _ = UnityEditor.EditorUtility.DisplayDialog(
                    "你忘了..:",
                    "科技树未结束编辑.不能启动游戏",
                    "继续编辑.");
                break;
        }
    }
    bool TryGetTwoNodeIgnore() => TryGetTwoNode(out _);
    bool TryGetTwoNode(out (TechNodeView l, TechNodeView r) nodePair)
    {
        var gos = UnityEditor.Selection.gameObjects;
        if (gos.Length != 2)
        {
            nodePair = default;
            return false;
        }
        var l = gos[0].GetComponent<TechNodeView>();
        var r = gos[1].GetComponent<TechNodeView>();
        if(l == null || r == null)
        {
            nodePair = default;
            return false;
        }
        if(l.transform.position.x > r.transform.position.x - 1f)
            (l, r) = (r, l);
        nodePair = (l, r);
        return true;
    }
    List<TechNodeView> GetOverlapNodes() => (
        from l in techNodeList
        from r in techNodeList
        where l != r && Vector3.Distance(l.transform.position, r.transform.position) < 1f
        select (List<TechNodeView>)[l, r]
        ).SelectMany(x => x).Distinct().ToList();
    IEnumerable<IGrouping<int, TechNodeView>> GetIDRepeatNodes() => 
        from n in techNodeList
        group n by n.ConfigInEditor.ID into g
        where g.Count() > 1
        select g;

    [Header("节点")]
    const int NodeOrder = 1000;
    TechNodeView? CurSelectedNode =>
        !Editing
            ? null
            : UnityEditor.Selection.gameObjects.Length == 1
                ? UnityEditor.Selection.gameObjects[0].GetComponent<TechNodeView>()
                : null;
    bool CurSelectOneNode => Editing && CurSelectedNode != null;
    [LabelText("当前节点信息"), PropertyOrder(NodeOrder + 14), ShowIf(nameof(CurSelectOneNode))]
    [OdinSerialize, ShowInInspector] TechNodeConfig? curNodeConfig;
    [LabelText("附近位置倍率"), PropertyOrder(NodeOrder + 14), ShowIf(nameof(CurSelectOneNode))]
    public int PosDelta = 100;
    [Button("在当前节点附近创建新节点"), PropertyOrder(NodeOrder + 15), ShowIf(nameof(CurSelectOneNode))]
    void CreateNodeNearCur()
    {
        if (CurSelectedNode == null)
            return;
        var tarPos = CurSelectedNode.transform.position + Vector3.up * PosDelta + Vector3.right * PosDelta;
        NodePos = new Vector2Int((int)tarPos.x, (int)tarPos.y);
        CreateNode();
    }
    [LabelText("创建节点位置"), PropertyOrder(NodeOrder + 18), ShowIf(nameof(Editing))]
    public Vector2Int NodePos;
    [Button("创建新节点"), PropertyOrder(NodeOrder + 20), ShowIf(nameof(Editing))]
    void CreateNode()
    {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(pfbTechNodeView.gameObject, trsTechNode) as GO;
        var ins = go!.GetComponent<TechNodeView>();
        ins.transform.position = new Vector3(NodePos.x, NodePos.y, 0);
        UnityEditor.Selection.activeGameObject = ins.gameObject;
        ins.ConfigInEditor = new TechNodeConfig
        {
            ID = 0,
            Name = "新节点",
            Pos = NodePos,
            ToUnLockItems = [],
            RequireDic = []
        };
        ins.Data = new TechNodeData(ins.ConfigInEditor);
        
        UnityEditor.EditorUtility.SetDirty(ins);
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, nameof(CreateNode));
        UnityEditor.Undo.IncrementCurrentGroup();
        ins.OnCreate();
    }
    const int LineOrder = 2000;
    [Header("线")]
    [LabelText("左输出端口ID"), Range(1, 5), PropertyOrder(LineOrder + 10), ShowIf(nameof(Editing))]
    public int LeftOutPort;
    [LabelText("右输入端口ID"), Range(1, 5), PropertyOrder(LineOrder + 20), ShowIf(nameof(Editing))]
    public int RightInPort;

    bool CheckHasRepeatID(string wantToDo)
    {
        var groups = GetIDRepeatNodes().ToList();
        if (groups.Any())
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "ID重复 !",
                $"存在节点的ID重复, 无法{wantToDo}. 请先解决ID冲突(将自动选中ID重复的第一组物体).",
                "返回"
            );
            UnityEditor.Selection.objects = groups[0].Select(n => n.gameObject as Object).ToArray();
            return true;
        }
        return false;
    }
    [Button("创建连线 (要求:选中两个节点,并在上面指定两个端口)"), EnableIf(nameof(TryGetTwoNodeIgnore)), PropertyOrder(LineOrder + 30), ShowIf(nameof(Editing))]
    void CreateLine()
    {
        if (!TryGetTwoNode(out var pair))
            return;
        if (CheckHasRepeatID("创建连线"))
            return;
        if(techLineList.Any(line => line.Left == pair.l && line.Right == pair.r))
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "已存在 !", 
                $"已存在这两个节点之间的连线.", 
                "返回"
            );
            return;
        }
        if (techLineList.Any(line => line.Left == pair.l && line.LeftOutPort == LeftOutPort))
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "重叠 !", 
                $"左端口与已有的重叠.",
                "返回"
            );
            return;
        }
        if (techLineList.Any(line => line.Right == pair.r && line.RightInPort == RightInPort))
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "重叠 !", 
                $"右端口与已有的重叠.", 
                "返回"
            );
            return;
        }
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(pfbTechLineView.gameObject, trsTechLine) as GO;
        var ins = go!.GetComponent<TechLineView>();
        ins.Left = pair.l;
        ins.LeftOutPort = LeftOutPort;
        ins.Right = pair.r;
        ins.RightInPort = RightInPort;
        
        UnityEditor.EditorUtility.SetDirty(ins);
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, nameof(CreateLine));
        UnityEditor.Undo.IncrementCurrentGroup();
        ins.OnCreate();
    }
    [Button("移除连线 (要求：选中两个节点)"), EnableIf(nameof(TryGetTwoNodeIgnore)), PropertyOrder(LineOrder + 40), ShowIf(nameof(Editing))]
    public void RemoveAllLines()
    {
        if (!TryGetTwoNode(out var pair))
            return;
        if (CheckHasRepeatID("移除连线"))
            return;
        var line = techLineList.FirstOrDefault(line => line.Left == pair.l && line.Right == pair.r);
        if(line == null)
        {
            UnityEditor.EditorUtility.DisplayDialog(
                "不存在 !", 
                $"不存在这两个节点之间的连线.", 
                "返回"
            );
            return;
        }
        UnityEditor.Undo.DestroyObjectImmediate(line.gameObject);
    }
    static void LockLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return;
        UnityEditor.Tools.lockedLayers |= (1 << layer);
    }
    static void UnlockLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return;
        UnityEditor.Tools.lockedLayers &= ~(1 << layer);
    }
#endif
}