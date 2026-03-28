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
public partial record EttSymbol : EttBase<EttSymbol>
{
    // public static Func<SymbolData> CreateEmpty => () => Create(-1);
    // public static Func<int, SymbolData> Create => id => new SymbolData(id);
    // SymbolData()
    // {
        // ConfigID = configID;
        // if (Config == null)
        // {
            // MyDebug.LogError($"符号ID {configID} 不存在，已创建空符号");
            // ConfigID = -1;
            // if (Config == null)
            // {
                // MyDebug.LogError($"空符号(ID = -1)也不存在");
            // }
            // return;
        // }
    // }
    [JsonConstructor] [DebuggerStepThrough] EttSymbol(){}
   
    // public readonly List<int> TempAdd = [];
    // public readonly List<int> TempMulti = [];
    // public readonly List<int> TowaAdd = [];
    // public readonly List<int> TowaMulti = [];
    
    // [ShowInInspector, PropertyOrder(0)] public int ConfigID { get; init; }
    // Node? configDes;
    // [HideInInspector] public bool IsEmpty => ConfigID == -1;
    // [ShowInInspector, PropertyOrder(1)]public string Name => Config.Name;
    // SymbolConfig Config => RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ConfigID);

    // string PosInfo => (
    //     from pos in Pos 
    //     select $"(Pos:{pos.X} {pos.Y})") | string.Empty;
    
    // public override string ToString() => $"{Config.Name}(ID:{ConfigID}){PosInfo}";
}
public partial class SymbolInPlay : EttSymbol.ICom, GamePlaying.INodeCom
{
    [EvtChanged] public partial Vector2Int Pos { get; private set; }
}
