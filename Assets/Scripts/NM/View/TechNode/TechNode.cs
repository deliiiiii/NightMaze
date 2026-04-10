using System;
using System.Collections.Generic;   
using JetBrains.Annotations;
using Newtonsoft.Json;
using NM.Config;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace NM.View;

public class TechNode : MonoBehaviour, ITechObj
{
    [NonSerialized, JsonIgnore] int @int;

    [SerializeField] Trs trsInPort;
    [SerializeField] Trs trsOutPort;
    public UICircle? ImgHandle;
    // [Sirenix.OdinInspector.ReadOnly] public List<GO> InPortHandleList;
    // [Sirenix.OdinInspector.ReadOnly] public List<GO> OutPortHandleList;

    public Trs? GetOutPortTrs(int id) => trsOutPort.GetChild(id - 1);
    public Trs? GetInPortTrs(int id) => trsInPort.GetChild(id - 1);

    public void OnCreate()
    {
        // InPortHandleList = trsInPort.GetChildren().Select(t => t.gameObject).ToList();
        // OutPortHandleList = trsOutPort.GetChildren().Select(t => t.gameObject).ToList();
    }

    public void OnStartEdit()
    {
        ImgHandle?.enabled = true;
        OnDeSelect();
    }

    public void OnEndEdit()
    {
        ImgHandle?.enabled = false;
    }

    public void OnSelect()
    {
        ImgHandle?.color = Color.blue;
    }

    public void OnDeSelect()
    {
        ImgHandle?.color = Color.white;
    }
}

public class TechNodeInfo
{
    public List<(EPropType, int)> PropRequireList;
    public List<(EPropType, int)> PropEarnedList;
    public bool UnLocked;
}


[PublicAPI] [Serializable]
public class NodeLineInfo
{
    public TechNode Left;
    public int LeftOutPortID;
    public TechNode Right;
    public int RightInPortID;
}