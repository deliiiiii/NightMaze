using System;
using GeneralPreview;
using NM.Config;
using UnityEngine;

namespace NM.Data;
[Serializable]
public class SymbolEtt : EttBase<SymbolEtt>
{
    public required SymbolConfig Config;
    public Vector2Int Pos;
}

public class SymbolComStock : SymbolEtt.ICom
{
    public int Count;
}

public class SymbolComEveryNSpin : SymbolEtt.ICom
{
    public int Count;
}