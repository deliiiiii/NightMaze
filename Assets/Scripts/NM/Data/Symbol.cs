using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;

namespace NM.Data;
[Serializable]
public class SymbolEtt(SymbolConfig config) : EttBase<SymbolEtt>
{
    public SymbolConfig Config = config;

    public IEnumerable<IFuncWrap> OnEvtList()
    {
        yield return EvtBus.Bind<EvtAdjacent>(SymbolInSpin.OnEvtAdjacent);
    }
    
    public static SymbolEtt CreateEmptySymbol()
        => new(RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == -1));
    public static SymbolEtt CreateSymbol(int id) 
        => new(RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == id));
}

public class SymbolInSpin : SymbolEtt.ICom<PlayingSpin>
{
    public Vector2Int Pos;
    
    public static UniTask OnEvtAdjacent(EvtAdjacent evt)
    {
        return UniTask.CompletedTask;
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