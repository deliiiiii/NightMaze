using System;
using System.Collections.Generic;
using GeneralPreview;
using NM.Config;

namespace NM.Data;
[Serializable]
public class SymbolEtt(SymbolConfig config) : EttBase<SymbolEtt>
{
    public SymbolConfig Config = config;

    public IEnumerable<Action<EvtBase>> EvtList()
    {
        yield return As<EvtAdjacent>(SymbolInSpin.OnEvtAdjacent);
    }
    
    public static Action<EvtBase> As<T>(Action<T> action) where T : EvtBase
    {
        return evt =>
        {
            if (evt is T t)
            {
                action(t);
            }
        };
    }
}

public class SymbolInSpin : SymbolEtt.ICom<PlayingSpin>
{
    public Vector2Int Pos;
    
    public static void OnEvtAdjacent(EvtAdjacent evt)
    {
        
    }
}

public class SymbolComStock : SymbolEtt.ICom
{
    public int Count;
}

public class SymbolComEveryNSpin : SymbolEtt.ICom
{
    public int Count;
}