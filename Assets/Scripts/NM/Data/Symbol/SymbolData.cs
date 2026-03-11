using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using General;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Data;

[Serializable]
public class SymbolData : DataBase<SymbolData>
{
    SymbolData(int configID)
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
            return;
        }
        AddCom(SymbolC2Com.Create(Config));
    }
    [JsonConstructor]
    [DebuggerStepThrough] SymbolData(){}
    public static Func<SymbolData> CreateEmpty => () => Create(-1);
    public static Func<int, SymbolData> Create => id => new SymbolData(id);
    public MyOption<Vector2Int> Pos = None;
    public bool AlreadyChecked;
    public readonly List<int> TempAdd = [];
    public readonly List<int> TempMulti = [];
    public readonly List<int> TowaAdd = [];
    public readonly List<int> TowaMulti = [];
    public DoCountBase DoCount = new DoCountInfinite();
    public MyOption<int> Stock = None;
    public MyOption<int> EveryNSpin = None;
    
    [ShowInInspector, PropertyOrder(0)] public int ConfigID { get; init; }
    [HideInInspector] public bool IsEmpty => ConfigID == -1;
    [ShowInInspector, PropertyOrder(1)]public string Name => Config.Name;
    SymbolConfig Config => field ??= RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ConfigID);
    string PosInfo => Pos.Match(some => $"(Pos:{some.X},{some.Y})", RStr);
    
    public override string ToString() => $"{Config.Name}(ID:{Config.ID}){PosInfo}";
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
    
    public abstract class ConfigDesBase<TConfig> : ComBase where TConfig : SymbolConfig
    {
        protected TConfig Config => (TConfig)BelongData.Config;
    }
}