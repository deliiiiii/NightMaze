using System;
using System.Collections.Generic;
using System.Linq;
using General;
using NM.Config;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2Int = UnityEngine.Vector2Int;

// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;
public class TechTreeEditor : Singleton<TechTreeEditor>
{
#if UNITY_EDITOR
    public static TechNode? GetNodeByID(int id) => Instance.techNodeList.FirstOrDefault(node => node.Config.ID == id);
    [SerializeField, HideInInspector] TechNode pfbTechNode;
    [SerializeField, HideInInspector] TechLine pfbTechLine;
    [SerializeField, HideInInspector] Trs trsTechNode;
    [SerializeField, HideInInspector] Trs trsTechLine;
    [NonSerialized] List<TechNode> techNodeList = [];
    [NonSerialized] List<TechLine> techLineList = [];

     
    [UnityEditor.InitializeOnLoadMethod]
    static void InitOnStart() 
        => UnityEditor.EditorApplication.delayCall += () => FindObjectOfType<TechTreeEditor>()?.OnDelayCall();

    void OnDelayCall()
    {
        // MyDebug.Log($"{nameof(TechTreeEditor)} OnDelayCall()");
        UnityEditor.EditorApplication.update -= OnEditorUpdate;
        UnityEditor.EditorApplication.update += OnEditorUpdate;
        UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        OnEndEdit();
    } 
    
    [LabelText("正在编辑"), ReadOnly, PropertyOrder(10)] public bool Editing;

    
    [Header("开始/结束编辑")] 
    [SerializeField] TechTreeConfig treeConfig;
    bool NotEditing => !Editing;
    [Button, EnableIf(nameof(NotEditing)), PropertyOrder(20)]
    public void StartEdit()
    {
        Editing = true;
        OnHierarchyChanged();
        LockLayer(Const.Layer.TechUI);
        UnlockLayer(Const.Layer.TechUIHandle);
        UnityEditor.Selection.activeGameObject = null;
        techNodeList.ForEach(t => t.OnStartEdit());
        techLineList.ForEach(t => t.OnStartEdit());
    }
    [Button, EnableIf(nameof(Editing)), PropertyOrder(30)]
    public void EndEdit()
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

    public void OnEndEdit()
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
        treeConfig.NodeList = techNodeList.Select(n => n.Config).ToList();
        treeConfig.LineList = techLineList.Select(l => new TechLineConfig
        {
            LeftNodeID = l.Left.Config.ID,
            LeftPortID = l.LeftOutPort,
            RightNodeID = l.Right.Config.ID,
            RightPortID = l.RightInPort,
        }).ToList();
        UnityEditor.EditorUtility.SetDirty(treeConfig);
    }

    void LoadFromConfig()
    {
        
    }

    readonly Dictionary<GO, ITechObj> goDic = [];
    readonly List<ITechObj> lastSelected = [];
    double tickInterval = 0.1;
    double lastTickTime;
    public void OnEditorUpdate()
    {
        if (!Editing || UnityEditor.EditorApplication.isPlaying)
            return;
        // 1. 时间拦截：如果距离上次执行还没经过指定的时间间隔，直接返回
        var curTime = UnityEditor.EditorApplication.timeSinceStartup;
        if (curTime - lastTickTime < tickInterval)
        {
            return;
        }

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
            node.Config.Pos = node.transform.position;
        }
        foreach (var line in techLineList.Where(l => l != null))
        {
            line.OnCreate();
        }

        RefreshCurSelectedNode();
        UnityEditor.SceneView.RepaintAll();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    public void OnHierarchyChanged()
    {
        if (UnityEditor.EditorApplication.isPlaying || !Editing)
            return;
        techNodeList = trsTechNode.GetComponentsInChildren<TechNode>().ToList();
        techLineList = trsTechLine.GetComponentsInChildren<TechLine>().ToList();
    }

    public void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
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
    bool TryGetTwoNode(out (TechNode l, TechNode r) nodePair)
    {
        var gos = UnityEditor.Selection.gameObjects;
        if (gos.Length != 2)
        {
            nodePair = default;
            return false;
        }
        var l = gos[0].GetComponent<TechNode>();
        var r = gos[1].GetComponent<TechNode>();
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
    // bool CanTwoNodeAddEdge()
    // {
        // if (!TryGetTwoNode(out var pair))
            // return false;
        // return !techLineList.Any(line => line.Left == pair.l && line.Right == pair.r);
    // }
    // bool CanTwoNodeRemoveEdge()
    // {
        // if (!TryGetTwoNode(out var pair))
            // return false;
        // return techLineList.Any(line => line.Left == pair.l && line.Right == pair.r);
    // }
    List<TechNode> GetOverlapNodes() => (
        from l in techNodeList
        from r in techNodeList
        where l != r && Vector3.Distance(l.transform.position, r.transform.position) < 1f
        select (List<TechNode>)[l, r]
        ).SelectMany(x => x).Distinct().ToList();
    
    IEnumerable<IGrouping<int, TechNode>> GetIDRepeatNodes() => 
        from n in techNodeList
        group n by n.Config.ID into g
        where g.Count() > 1
        select g;

    [Header("节点")]
    const int NodeOrder = 1000;
    TechNode? CurSelectedNode
    {
        get
        {
            if (!Editing)
                return null;
            return UnityEditor.Selection.gameObjects.Length == 1
                ? UnityEditor.Selection.gameObjects[0].GetComponent<TechNode>()
                : null;
        }
    }

    bool CurSelectOneNode => Editing && CurSelectedNode != null;

    [Label("当前节点信息"), PropertyOrder(NodeOrder + 14), ShowIf(nameof(CurSelectOneNode))]
    [SerializeReference] TechNodeConfig? curNodeConfig;
    void RefreshCurSelectedNode()
    {
        if (CurSelectedNode == null)
            return;
        var newConfig = CurSelectedNode.Config;
        if (curNodeConfig != null && newConfig == curNodeConfig)
            return;
        curNodeConfig = newConfig;
    }
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
        var ins = Instantiate(pfbTechNode, trsTechNode);
        UnityEditor.Undo.RegisterCreatedObjectUndo(ins.gameObject, nameof(CreateNode));
        ins.transform.position = new Vector3(NodePos.x, NodePos.y, 0);
        UnityEditor.Selection.activeGameObject = ins.gameObject;
        ins.Config = new TechNodeConfig
        {
            ID = 0,
            Name = "新节点",
            Pos = NodePos,
            ToUnLockItems = [],
            RequireLineList = []
        };
    }
    const int LineOrder = 2000;
    [Header("线")]
    [Label("左输出端口ID"), Range(1, 5), PropertyOrder(LineOrder + 10), ShowIf(nameof(Editing))]
    public int LeftOutPort;
    [Label("右输入端口ID"), Range(1, 5), PropertyOrder(LineOrder + 20), ShowIf(nameof(Editing))]
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
        
        var ins = Instantiate(pfbTechLine, trsTechLine);
        UnityEditor.Undo.RegisterCreatedObjectUndo(ins.gameObject, nameof(CreateLine));
        // ins.Config = new TechLineConfig
        // {
        //     LeftNodeID = pair.l.Config.ID,
        //     LeftPortID = LeftOutPort,
        //     RightNodeID = pair.r.Config.ID,
        //     RightPortID = RightInPort,
        // };
        ins.Left = pair.l;
        ins.LeftOutPort = LeftOutPort;
        ins.Right = pair.r;
        ins.RightInPort = RightInPort;
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

    
    
    // [Button]
    // public void CreateNode(Vector2 tarPos)
    // {
    //     var ins = Instantiate(pfbTechNode, nodeParent.transform);
    //     ins.OnCreate();
    //     UnityEditor.Selection.activeGameObject = ins.gameObject;
    //     techNodeList.Add(ins);
    //     ins.OnDestroyEvt += () =>
    //     {
    //         techNodeList.Remove(ins);
    //         UnityEditor.Selection.activeGameObject = null;
    //     };
    //     UnityEditor.Undo.RegisterCreatedObjectUndo(ins, "Create Node");
    // }
    //
    
    public static void LockLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return;
        UnityEditor.Tools.lockedLayers |= (1 << layer);
    }
    public static void UnlockLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return;
        UnityEditor.Tools.lockedLayers &= ~(1 << layer);
    }
#endif
}