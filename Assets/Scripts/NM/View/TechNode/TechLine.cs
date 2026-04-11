using GeneralPreview;
using NM.Config;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace NM.View;

public class TechLine : MonoBehaviour, ITechObj
{
    [SerializeReference, ReadOnly] public TechLineConfig Config;
    UILineRenderer lineRenderer;
    
    public TechNode? Left => TechTreeEditor.GetNodeByID(Config.LeftNodeID);
    public int LeftOutPort => Config.LeftPortID;
    public TechNode? Right => TechTreeEditor.GetNodeByID(Config.RightNodeID);
    public int RightInPort => Config.RightPortID;

    public void OnCreate()
    {
        lineRenderer = this.GetOrAddCom<UILineRenderer>();
        var trsLeft = Left?.GetOutPortTrs(LeftOutPort);
        var trsRight = Right?.GetInPortTrs(RightInPort);
        if (trsLeft == null || trsRight == null) 
            return;
        Vector2 startLocal = transform.InverseTransformPoint(trsLeft.position);
        Vector2 endLocal = transform.InverseTransformPoint(trsRight.position);
        float midX = (startLocal.x + endLocal.x) / 2f;

        // 生成4个关键点（横 -> 竖 -> 横）
        // 点1：起点位置
        // 点2：向右延伸到一半
        Vector2 corner1 = new Vector2(midX, startLocal.y);
        // 点3：上下平移到终点的高度
        Vector2 corner2 = new Vector2(midX, endLocal.y);
        // 点4：终点位置
        lineRenderer.Points =
        [
            startLocal, 
            corner1, 
            corner2, 
            endLocal
        ];

        // 标记所有顶点脏了，以便 UGUI 在渲染时重新生成网格
        lineRenderer.SetVerticesDirty();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    public void OnStartEdit()
    {
    }

    public void OnEndEdit()
    {
    }

    public void OnSelect()
    {
    }

    public void OnDeSelect()
    {
    }
}