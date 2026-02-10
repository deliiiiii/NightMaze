using System;
using System.Collections.Generic;
using System.Linq;
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
        yield return EvtBus.Bind<EvtSymbolAdjacentSymbol>(SymbolInSpin.OnEvtAdjacent);

        // foreach (var ectReceiver in Config.EvtList ?? [])
        // {
        //     yield return EvtBus.Bind<TEvtReceived>(evtObj =>
        //     {
        //         if (!ectReceiver.CheckArg(evtObj))
        //             return UniTask.CompletedTask;
        //         EvtBus.FireAsync(new TEvtSent() { args = Selectors(evtObj) });
        //     });
        // }
    }
    
    public static SymbolEtt CreateEmptySymbol()
        => new(RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == -1));
    public static SymbolEtt CreateSymbol(int id) 
        => new(RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == id));
}

public class SymbolInSpin : SymbolEtt.ICom<PlayingSpin>
{
    public Vector2Int Pos;
    
    public static UniTask OnEvtAdjacent(EvtSymbolAdjacentSymbol evt)
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