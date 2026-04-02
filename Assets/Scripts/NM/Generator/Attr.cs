using System;

namespace NM;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EvtChangedAttribute : Attribute;
[AttributeUsage(AttributeTargets.Class)]
public sealed class ActContainerAttribute : Attribute;
[AttributeUsage(AttributeTargets.Method)]
public sealed class ActConfigAttribute(bool muteEvt = false) : Attribute
{
    public readonly bool MuteEvt = muteEvt;
}