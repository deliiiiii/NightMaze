using System;
using System.Collections.Generic;   
using System.Linq;
using GeneralPreview;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class TechNode : MonoBehaviour
{
    [NonSerialized, JsonIgnore] int @int;
    [ValidateInput(nameof(CheckAll))]
    public List<AdjNodeInfo> PreNodeList = [];
    public List<AdjNodeInfo> NextNodeList = [];

    [Button]
    bool CheckAll(List<AdjNodeInfo> preNodeList, ref string defaultMessage, ref InfoMessageType messageType)
    {
        var e =
            from eIn in CheckInPort()
            from eOut in CheckOutPort()
            select eOut;
        if (e.IsLeft(out _, out var r))
        {
            defaultMessage = string.Blank;
            messageType = InfoMessageType.Info;
        }
        else
        {
            defaultMessage = r;
            messageType = InfoMessageType.Error;
        }
        return false;
    }
    MyEither<bool, string> CheckInPort()
    {
        var inPortGroup =
            from preNode in PreNodeList
            group preNode by preNode.InPortID into g
            select g;
        return inPortGroup.Any(g => g.Count() > 1) ? "输入端口中有ID重复" : true;
    }
    MyEither<bool, string> CheckOutPort()
    {
        var outPortGroup =
            from nextNode in NextNodeList
            group nextNode by nextNode.OutPortID into g
            select g;
        return outPortGroup.Any(g => g.Count() > 1) ? "输出端口中有ID重复" : true;
    }
}
[PublicAPI][Serializable]
public class AdjNodeInfo
{
    [Required] public TechNode TechNode = null!;
    [Range(1, 5)] public int OutPortID = 3;
    [Range(1, 5)] public int InPortID = 3;
}

[PublicAPI]
[Serializable]
public class NodeLineInfo
{
    public TechNode Left = null!;
    public TechNode Right = null!;
}