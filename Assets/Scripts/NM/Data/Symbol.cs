using System;
using System.Collections.Generic;
using GeneralPreview;
using NM.Config;
using Sirenix.OdinInspector;

namespace NM.Data;
[Serializable]
public class SymbolEtt(SymbolConfig config) : EttBase<SymbolEtt>
{
    public SymbolConfig Config = config;

    public IEnumerable<IFuncWrap> OnEvt(GamePlaying ctx)
    {
        if (Config.ID == 1)
        {
            yield return Bus.Bind<EvtSymbolAdjacentSymbol>(async (evt, ct) =>
            {
                if (evt.Arg2 == this && evt.Arg1.Config.ID == 2)
                    await ctx.DoSymbolAddSymbol.Do(this, CreateSymbol(9), ct);
            });
        }
    }
    public bool IsEmpty => Config.ID == -1;
    public static SymbolEtt CreateEmptySymbol()
        => new(RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == -1));
    public static SymbolEtt CreateSymbol(int id) 
        => new(RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == id));

    public override string ToString()
    {
        return $"Symbol{Config.Name}({Config.ID}) {PosInfo})";
    }
    string PosInfo => GetCom<SymbolInSpin>().Match(some => some.Pos.ToString(), () => string.Empty);
}

#region DoCount
public abstract class DoCountBase;
public class DoCountInfinite : DoCountBase;
public class DoCountNumber : DoCountBase
{
    [MinValue(1)]public int N = 1;
}
#endregion

public class SymbolInSpin : SymbolEtt.ICom<PlayingSpin>
{
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