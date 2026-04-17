using System;
using System.Collections.Generic;   
using JetBrains.Annotations;
using NM.Config;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace NM.View;

public class TechNode : MonoBehaviour, ITechObj
{
    [SerializeReference] public TechNodeConfig Config;
    
    [SerializeField] Trs trsInPort;
    [SerializeField] Trs trsOutPort;
    public UICircle? ImgHandle;

    public Trs? GetOutPortTrs(int id) => trsOutPort.GetChild(id - 1);
    public Trs? GetInPortTrs(int id) => trsInPort.GetChild(id - 1);

    public void OnCreate()
    {
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
    public TechNode Left;
    public int LeftOutPortID;
    public TechNode Right;
    public int RightInPortID;
}