using System;
using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using NM.Config;
using Sirenix.OdinInspector;

namespace NM.Data;
[Serializable]
public class SymbolData : DataBase<SymbolData>
{
    public static SymbolData CreateEmptySymbol() => CreateSymbol(-1);
    public static SymbolData CreateSymbol(int id) => new(id);
    
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
        Bus.FireAndForget(new EvtUltimateGiveChanged(this, GetUltimateGive()));
    }

    public void DoTempMulti(int multi)
    {
        TempMulti.Add(multi);
        Bus.FireAndForget(new EvtUltimateGiveChanged(this, GetUltimateGive()));
    }
    
    public void DoTowaAdd(int add)
    {
        TowaAdd.Add(add);
        Bus.FireAndForget(new EvtUltimateGiveChanged(this, GetUltimateGive()));
    }
    public void DoTowaMulti(int multi)
    {
        TowaMulti.Add(multi);
        Bus.FireAndForget(new EvtUltimateGiveChanged(this, GetUltimateGive()));
    }
    [TypeRegistryItem("某符号的最终金钱改变时\t(SymbolEtt)")]
    public record EvtUltimateGiveChanged(SymbolData Symbol, long UltimateGive) : EvtBase;

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

    public SymbolData(int configID)
    {
        ConfigID = configID;
        if (Config == null)
        {
            MyDebug.LogError($"符号ID {configID} 不存在，已创建空符号");
            ConfigID = -1;
            if (Config == null)
            {
                MyDebug.LogError($"空符号(ID = -1)也不存在");
            }
        }
    }

    public override string ToString() => $"{Config.Name}(ID:{Config.ID}) {PosInfo})";
    string PosInfo => Pos.Match(some => $"Pos{some.ToString()}", RStr);
    
    public static Comparison<SymbolData> ByPos => (s1, s2) =>
    {
        var p1 = s1.Pos.Match(Rid, () => new Vector2Int(int.MaxValue, int.MaxValue));
        var p2 = s2.Pos.Match(Rid, () => new Vector2Int(int.MaxValue, int.MaxValue));
        return p1.X != p2.X ? p1.X - p2.X : p1.Y - p2.Y;
    };
}

// public abstract class SymbolData<TConfig>(int configID) : SymbolData(configID) where TConfig : SymbolConfig
// {
//     protected new TConfig Config => (TConfig)base.Config;
// }

#region DoCount
public abstract class DoCountBase;
public class DoCountInfinite : DoCountBase;
public class DoCountNumber : DoCountBase
{
    [MinValue(1)]public int N = 1;
}
#endregion
public class SymbolComStock : SymbolData.ICom
{
    public int Count;
}

public class SymbolComEveryNSpin : SymbolData.ICom
{
    public int Count;
}