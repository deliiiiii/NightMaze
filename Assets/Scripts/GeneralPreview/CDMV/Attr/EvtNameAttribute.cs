using System;
using System.Diagnostics;

namespace GeneralPreview;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)][DebuggerStepThrough]
public sealed class EvtNameAttribute(string name) : Attribute
{
    public string Name = name;
}