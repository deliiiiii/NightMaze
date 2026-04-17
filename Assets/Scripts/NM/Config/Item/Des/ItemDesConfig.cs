using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[Serializable]
public record ItemDesConfig
{
    public const string FromLast = "上{0}步中的: ";
    [LabelText("人话描述"), TextArea(2, 10)] public string DesToPlayer = "输入人话...";
    [SerializeReference, LabelText("触发器")] public ItemDesTriggerBase? Trigger = new ItemDesTriggerEnterSpin();
    [SerializeReference, LabelText("尝试执行...")] public ItemDesResultBase? Result;
}