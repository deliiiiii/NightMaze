using System;
using System.Diagnostics;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Config;
[Serializable]
public abstract record EvtDesResultBase;
[TypeRegistryItem("【弃置】解锁下一层", "弃置")][DebuggerStepThrough]
public record ItemDesResultUnlockNextLayer : EvtDesResultBase;

[TypeRegistryItem("解锁相邻区域, 偏移{0}")]
[DebuggerStepThrough]
public record ItemDesResultUnlockAdjacentArea : EvtDesResultBase
{
    [LabelText("{0}: 区域坐标偏移"), ValidateInput(nameof(CheckDelta))]public Vector2Int Delta = Vector2Int.Zero;
    bool CheckDelta => Delta != Vector2Int.Zero;
}
[TypeRegistryItem("清空敌意值")][DebuggerStepThrough]
public record ItemDesResultClearHostility : EvtDesResultBase;