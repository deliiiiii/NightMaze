using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace NM.Config;
[Serializable]
public abstract record EvtDesResultBase;
[TypeRegistryItem("解锁下一层")][DebuggerStepThrough]
public record ItemDesResultUnlockNextLayer : EvtDesResultBase;
[TypeRegistryItem("清空敌意值")][DebuggerStepThrough]
public record ItemDesResultClearHostility : EvtDesResultBase;