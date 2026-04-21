using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using JetBrains.Annotations;
using NM.Config;
using NM.Data;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace NM.View;

public class TechNodeView : ViewBase, ITechObj
{
    [Sirenix.OdinInspector.ReadOnly] public TechNodeData Data;
    [SerializeField] Img imgCur;
    [SerializeField] Btn btnCur;
    [SerializeField] Outline outline;
    [SerializeField] Txt txtName;
    [SerializeField] Trs trsBuildingPreView;
    [SerializeField] TechNodeBuildingPreView pfbBuildingPreView;
    [SerializeField] Trs trsRequire;
    [SerializeField] TechNodeLineView pfbLineView;

    
    [SerializeField] Trs trsInPort;
    [SerializeField] Trs trsOutPort;
    public UICircle? UICircle;
    public Txt? TxtID;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnCur.onClick.EvtBindTo(() => PlayViewIns.Data.TechTreeData.CurID = Data.ID);
    }
    UniEvt<TechTreeData.EvtCurIDChanged> OnCurIdChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            RefreshImgCur(IsCurIDIncludeThis(evt.NewValue));
            return UniTask.CompletedTask;
        },
        Des = "刷新显示",
    };
    bool IsCurIDIncludeThis(int? curID)
    {
        if (!curID.HasValue)
            return false;
        if (Data.ID == curID.Value)
            return true;
        var lineConfigs = TechTreeData.Config.LineList;
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(curID.Value);
        visited.Add(curID.Value);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var line in lineConfigs.Where(line => line.RightNodeID == current))
            {
                if (line.LeftNodeID == Data.ID) 
                    return true;
                if (visited.Add(line.LeftNodeID)) 
                    queue.Enqueue(line.LeftNodeID);
            }
        }
        return false;
    }
    void RefreshImgCur(bool isCurIncludeThis)
    {
        if (Data.Unlocked)
        {
            imgCur.color = Color.green.SetAlpha(0.5f);
            return;
        }
        imgCur.color = !isCurIncludeThis ? Color.gray.SetAlpha(0.5f) : new Color(0.9f, 0.6f, 0f, 0.5f);
    }
    public void OnCreateView(TechNodeData data, TechNodeConfig? configInEditor = null, int? curId = null)
    {
        var isEditor = configInEditor != null;
        TechNodeConfig tarConfig;
        Data = data;
        if (!isEditor)
        {
            tarConfig = data.Config;
            OnEndEdit();
            RefreshImgCur(IsCurIDIncludeThis(curId));
        }
        else
            tarConfig = configInEditor!;
        txtName.text = tarConfig.Name;
        TxtID?.text = tarConfig.ID.ToString();
        trsBuildingPreView.ClearActiveChildren(isEditor);
        if(tarConfig.ToUnLockItems != null)
            foreach (var itemConfig in tarConfig.ToUnLockItems)
            {
                if(itemConfig == null)
                    continue;
                var buildingPreView = Instantiate(pfbBuildingPreView, trsBuildingPreView);
                buildingPreView.OnCreateView(itemConfig);
                buildingPreView.SetActiveTrue();
            }
        trsRequire.ClearActiveChildren(isEditor);
        if(tarConfig.RequireDic != null)
            foreach (var lineConfig in tarConfig.RequireDic)
            {
                var lineIns = Instantiate(pfbLineView, trsRequire);
                var curValue = Data.CarValueDic.GetValueOrDefault(lineConfig.Key, 0);
                var tarValue = lineConfig.Value;
                lineIns.TxtCurValue.text = curValue.ToString();
                lineIns.TxtTarValue.text = tarValue.ToString();
                lineIns.ImgFill.fillAmount = tarValue <= 0 ? 1 : Mathf.Clamp01((float)curValue / tarValue);
                lineIns.ImgFill.color = curValue >= tarValue ? Color.green : Color.red;
                lineIns.TxtPropType.text = lineConfig.Key.GetLabelText();
                lineIns.SetActiveTrue();
            }   
    }
    public Trs? GetOutPortTrs(int id) => trsOutPort.GetChild(id - 1);
    public Trs? GetInPortTrs(int id) => trsInPort.GetChild(id - 1);
    /// 编辑器创建的数据.
    public TechNodeConfig ConfigInEditor;

    public void OnCreate()
    {
        if(UICircle != null)
            UICircle.enabled = true;
        if(TxtID != null)
            TxtID.enabled = true;
        OnCreateView(Data, ConfigInEditor);
    }
    public void OnStartEdit()
    {
        if(UICircle != null)
            UICircle.enabled = true;
        if(TxtID != null)
            TxtID.enabled = true;
        OnDeSelect();
    }
    public void OnEndEdit()
    {
        if(UICircle != null)
            UICircle.enabled = false;
        if(TxtID != null)
            TxtID.enabled = false;
    }
    public void OnSelect()
    {
        if(UICircle != null)
            UICircle.color = Color.blue;
    }
    public void OnDeSelect()
    {
        if(UICircle != null)
            UICircle.color = Color.white;
    }
}

[PublicAPI] [Serializable]
public class NodeLineInfo
{
    public TechNodeView Left;
    public int LeftOutPortID;
    public TechNodeView Right;
    public int RightInPortID;
}