using System;

namespace GeneralPreview;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class EvtNameAttribute(string name) : Attribute
{
    public string Name = name;
}