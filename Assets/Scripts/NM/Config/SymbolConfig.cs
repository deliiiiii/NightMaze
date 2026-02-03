using System;
using System.Collections.Generic;
using GeneralPreview;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "NM_" + nameof(SymbolConfig))]
public class SymbolConfig : ConfigMulti<SymbolConfig>
{
    protected override string PrefixName => "Symbol";
    public List<SymbolDes> EffList = [];
}

#region Symbol Effects
[Serializable]
public class SymbolDes
{
    [SerializeReference] public EventBase Event = EventImmediate.One;
    [SerializeReference] public SymbolEffectBase Eff = SymbolEffectNone.One;
}

[Serializable]
public abstract class SymbolEffectBase
{
    public bool OnlyOnce;
}

public class SymbolEffectNone : SymbolEffectBase
{
    public static readonly SymbolEffectNone One = new ();
}

public class SymbolEffectAddGivePermanent : SymbolEffectBase
{
    public int Add;
}

#endregion

#region Event
[Serializable]
public abstract class EventBase;

public class EventImmediate : EventBase
{
    public static readonly EventImmediate One = new ();
}

public class EventTurnCount : EventBase
{
    public int Threshold;
}
#endregion