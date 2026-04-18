using System;
using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using JetBrains.Annotations;
using NM.Config;
using NM.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace NM.View;

public class TechNodeView : SerializedMonoBehaviour, ITechObj
{
    [Sirenix.OdinInspector.ReadOnly] public TechNodeData Data;
    [SerializeField] Txt txtName;
    [SerializeField] Trs trsBuildingPreView;
    [SerializeField] TechNodeBuildingPreView pfbBuildingPreView;
    [SerializeField] Trs trsRequire;
    [SerializeField] TechNodeLineView pfbLineView;

    
    [SerializeField] Trs trsInPort;
    [SerializeField] Trs trsOutPort;
    public UICircle? UICircle;
    public Txt? TxtID;
    
    public void OnCreateView(TechNodeData data, TechNodeConfig? configInEditor = null)
    {
        var isEditor = configInEditor != null;
        TechNodeConfig tarConfig;
        Data = data;
        if (!isEditor)
        {
            tarConfig = data.Config;
            OnEndEdit();
        }
        else
            tarConfig = configInEditor!;
        txtName.text = tarConfig.Name;
        TxtID?.text = tarConfig.ID.ToString();
        trsBuildingPreView.ClearActiveChildren(isEditor);
        if(tarConfig.ToUnLockItems != null)
            foreach (var itemConfig in tarConfig.ToUnLockItems.Where(itemConfig => itemConfig != null))
            {
                var img = Instantiate(pfbBuildingPreView, trsBuildingPreView);
                img.Img.sprite = ItemResLoader.Acquire(itemConfig!.ID);
                img.SetActiveTrue();
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