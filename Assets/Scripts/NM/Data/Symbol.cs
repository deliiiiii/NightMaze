using System;
using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using NM.Config;
using Sirenix.OdinInspector;

namespace NM.Data;
[Serializable]
public class SymbolEtt : EttBase<SymbolEtt>
{
    SymbolEtt(int configID)
    {
        ConfigID = configID;
    }
    
    public int ConfigID { get; init; }
    public MyOption<Vector2Int> Pos = None;
    public readonly List<int> TempAdd = [];
    public readonly List<int> TempMulti = [];
    public readonly List<int> TowaAdd = [];
    public readonly List<int> TowaMulti = [];
    
    public SymbolConfig Config => RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ConfigID);
    

    public void DoTempAdd(int add)
    {
        TempAdd.Add(add);
        Bus.FireAndForget(new EvtSpinSymbolUltimateGiveChanged(this, GetUltimateGive()));
    }

    public void DoTempMulti(int multi)
    {
        TempMulti.Add(multi);
        Bus.FireAndForget(new EvtSpinSymbolUltimateGiveChanged(this, GetUltimateGive()));
    }
    
    public void DoTowaAdd(int add)
    {
        TowaAdd.Add(add);
        Bus.FireAndForget(new EvtSpinSymbolUltimateGiveChanged(this, GetUltimateGive()));
    }
    public void DoTowaMulti(int multi)
    {
        TowaMulti.Add(multi);
        Bus.FireAndForget(new EvtSpinSymbolUltimateGiveChanged(this, GetUltimateGive()));
    }

    public long GetUltimateGive()
    {
        long ret = Config.Payout;
        ret += TempAdd.Aggregate(0L, (current, add) => current + add);
        ret += TowaAdd.Aggregate(0L, (current, add) => current + add);
        ret *= TempMulti.Aggregate(1, (current, multi) => current * multi);
        ret *= TowaMulti.Aggregate(1, (current, multi) => current * multi);
        return ret;
    }
    
    public bool IsEmpty => ConfigID == -1;
    public bool AlreadyChecked;
    public static SymbolEtt CreateEmptySymbol() => new(-1);
    public static SymbolEtt CreateSymbol(int id) => new(id);

    public override string ToString() => $"{Config.Name}(ID:{Config.ID}) {PosInfo})";
    string PosInfo => Pos.Match(some => $"Pos{some.ToString()}", RStr);
    
    public static Comparison<SymbolEtt> ByPos => (s1, s2) =>
    {
        var p1 = s1.Pos.Match(Rid, () => new Vector2Int(int.MaxValue, int.MaxValue));
        var p2 = s2.Pos.Match(Rid, () => new Vector2Int(int.MaxValue, int.MaxValue));
        return p1.X != p2.X ? p1.X - p2.X : p1.Y - p2.Y;
    };
}

#region DoCount
public abstract class DoCountBase;
public class DoCountInfinite : DoCountBase;
public class DoCountNumber : DoCountBase
{
    [MinValue(1)]public int N = 1;
}
#endregion
public class SymbolComStock : SymbolEtt.ICom
{
    public int Count;
}

public class SymbolComEveryNSpin : SymbolEtt.ICom
{
    public int Count;
}