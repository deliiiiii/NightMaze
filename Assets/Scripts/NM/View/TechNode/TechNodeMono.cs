using System;
using System.Collections.Generic;
using System.Linq;
using General;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;
public class TechNodeMono : Singleton<TechNodeMono>
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    static class TechNodeEditor
    {
        static TechNodeEditor()
        {
            if (Instance == null)
                return;
            Instance.OnEndEdit();
            UnityEditor.EditorApplication.update -= Instance.OnEditorUpdate;
            UnityEditor.EditorApplication.update += Instance.OnEditorUpdate;
            UnityEditor.EditorApplication.hierarchyChanged -= Instance.OnHierarchyChanged;
            UnityEditor.EditorApplication.hierarchyChanged += Instance.OnHierarchyChanged;
            UnityEditor.EditorApplication.playModeStateChanged -= Instance.OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += Instance.OnPlayModeStateChanged;
        }
    }
    
    [SerializeField] TechNode pfbTechNode;
    [SerializeField] TechLine pfbTechLine;
    [SerializeField] Trs trsTechNode;
    [SerializeField] Trs trsTechLine;
    [NonSerialized, ShowInInspector, ReadOnly] List<TechNode> techNodeList = [];
    [NonSerialized, ShowInInspector, ReadOnly] List<TechLine> techLineList = [];

    [Header("开始/结束编辑")]
    [LabelText("正在编辑"), ReadOnly, PropertyOrder(10)] public bool Editing;
    bool NotEditing => !Editing;
    [Button, EnableIf(nameof(NotEditing)), PropertyOrder(20)]
    public void StartEdit()
    {
        Editing = true;
        OnHierarchyChanged();
        LockLayer(Const.Layer.TechUI);
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
                $"检测到 {overlapNodes.Count} 个节点位置几乎重叠, 强制回到编辑模式修正.", 
                "返回"
            );
            // UnityEditor.Selection.gameObjects = overlapNodes.Select(t => t.gameObject).ToArray();
            return;
        }
        OnEndEdit();
    }

    public void OnEndEdit()
    {
        OnHierarchyChanged();
        UnlockLayer(Const.Layer.TechUI);
        UnityEditor.Selection.activeGameObject = null;
        techNodeList.ForEach(t => t.OnEndEdit());
        techLineList.ForEach(t => t.OnEndEdit());
        Editing = false;
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
        lastSelected.ForEach(t => t.OnDeSelect());
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
        
        
        foreach (var line in techLineList.Where(l => l != null))
        {
            line.OnCreate();
        }
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
        if (state == UnityEditor.PlayModeStateChange.ExitingEditMode && Editing)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            _ = UnityEditor.EditorUtility.DisplayDialog(
                "你忘了..:",
                "科技树未结束编辑.不能启动游戏",
                "继续编辑.");
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
    List<TechNode> GetOverlapNodes() =>
        (from l in techNodeList
            from r in techNodeList
            where l != r && Vector3.Distance(l.transform.position, r.transform.position) < 1f
            select (List<TechNode>)[l, r]).SelectMany(x => x).Distinct().ToList();

    const int LineOrder = 1000;
    [Header("线")]
    [Label("左输出端口ID"), Range(1, 5), PropertyOrder(LineOrder + 10), ShowIf(nameof(Editing))]
    public int LeftOutPort;
    [Label("右输入端口ID"), Range(1, 5), PropertyOrder(LineOrder + 20), ShowIf(nameof(Editing))]
    public int RightInPort;
    [Button("创建连线 (要求:选中两个节点,并在上面指定两个端口)"), EnableIf(nameof(TryGetTwoNodeIgnore)), PropertyOrder(LineOrder + 30), ShowIf(nameof(Editing))]
    void CreateLine()
    {
        if (!TryGetTwoNode(out var pair))
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
        UnityEditor.Undo.RegisterCreatedObjectUndo(ins.gameObject, "CreateLine");
        ins.Left = pair.l;
        ins.Right = pair.r;
        ins.LeftOutPort = LeftOutPort;
        ins.RightInPort = RightInPort;
        ins.OnCreate();
    }

    [Button("移除连线 (要求：选中两个节点)"), EnableIf(nameof(TryGetTwoNodeIgnore)), PropertyOrder(LineOrder + 40), ShowIf(nameof(Editing))]
    public void RemoveAllLines()
    {
        if (!TryGetTwoNode(out var pair))
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