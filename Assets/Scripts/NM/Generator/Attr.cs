using System;

namespace NM;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EvtChangedAttribute : Attribute;
[AttributeUsage(AttributeTargets.Class)]
public sealed class ActContainerAttribute : Attribute;