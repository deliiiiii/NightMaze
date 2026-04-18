using System;
using System.Collections.Generic;
using General;
using GeneralPreview;
using JetBrains.Annotations;
using NM.Config;
using NM.Data;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace NM.View;

public class TechNodeView : MonoBehaviour, ITechObj
{
    [SerializeReference, ReadOnly] public TechNodeData Data;
    [SerializeField] Txt txtName;
    [SerializeField] Trs trsBuildingPreView;
    [SerializeField] TechNodeBuildingPreView pfbBuildingPreView;
    [SerializeField] Trs trsRequire;
    [SerializeField] TechNodeLineView pfbLineView;

    
    [SerializeField] Trs trsInPort;
    [SerializeField] Trs trsOutPort;
    public UICircle? ImgHandle;
    
    public void OnCreateView(TechNodeData data)
    {
        Data = data;
        OnEndEdit();
        
        txtName.text = Data.Config.Name;
        trsBuildingPreView.ClearActiveChildren();
        foreach (var itemConfig in Data.Config.ToUnLockItems)
        {
            var img = Instantiate(pfbBuildingPreView, trsBuildingPreView);
            img.Img.sprite = ItemResLoader.Acquire(itemConfig.ID);
            img.SetActiveTrue();
        }
        trsRequire.ClearActiveChildren();
        foreach (var lineConfig in Data.Config.RequireDic)
        {
            var lineIns = Instantiate(pfbLineView, trsRequire);
            var curValue = Data.CarValueDic.GetValueOrDefault(lineConfig.Key, 0);
            var tarValue = lineConfig.Value;
            lineIns.TxtCurValue.text = curValue.ToString();
            lineIns.TxtTarValue.text = tarValue.ToString();
            lineIns.ImgFill.fillAmount = tarValue <= 0 ? 1 : Mathf.Clamp01((float)curValue / tarValue);
            // TODO 根据属性来决定颜色
            lineIns.ImgFill.color = curValue >= tarValue ? Color.green : Color.red;
            lineIns.TxtPropType.text = lineConfig.Key.GetLabelText();
            lineIns.SetActiveTrue();
        }
    }
    public Trs? GetOutPortTrs(int id) => trsOutPort.GetChild(id - 1);
    public Trs? GetInPortTrs(int id) => trsInPort.GetChild(id - 1);
    /// <summary>
    /// 编辑器创建的数据.
    /// </summary>
    [SerializeReference] public TechNodeConfig ConfigInEditor;

    public void OnCreate()
    {
        ImgHandle?.SetActiveTrue();
    }
    public void OnStartEdit()
    {
        ImgHandle?.enabled = true;
        OnDeSelect();
    }
    public void OnEndEdit()
    {
        if(ImgHandle == null)
            return;
        ImgHandle?.enabled = false;
    }
    public void OnSelect()
    {
        ImgHandle?.color = Color.blue;
    }
    public void OnDeSelect()
    {
        if(ImgHandle == null)
            return;
        ImgHandle?.color = Color.white;
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