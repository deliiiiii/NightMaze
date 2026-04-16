using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using General;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.Utilities;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Data;

public partial class GamePlaying
{
    public partial class MyItem
    {
        [JsonConstructor]
        MyItem()
        {
            DeltaPosList = [];
            EatConfigList = [];
            BuildingOrEventProgress = [];
        }
        public MyItem(int id, Vector2Int pivotPos)
        {
            ID = id;
            PivotPos = pivotPos;
            DeltaPosList = Config!.Pos switch
            {
                ItemPosRectangle rect => (
                    from x in Range(0, rect.Length)
                    from y in Range(0, rect.Height)
                    select new Vector2Int(x, y)).ToList(),
                ItemPosCustom custom => custom.DeltaPosList,
                _ => [Vector2Int.Zero],
            };
            EatConfigList = [];
            BuildingOrEventProgress = Config.IsBuildingOrEvent 
                ? Config.BuildPropValueList.ToDictionary(p => p.Key, _ => 0L) 
                : [];
        }
        [DebuggerStepThrough] public bool CoverPos(Vector2Int pos) => CoveredPosList.Contains(pos);
        public IEnumerable<Vector2Int> CoveredPosList => DeltaPosList.Select(d => d + PivotPos);
        public ItemConfig Config => field ??= ConfigLoader.Acquire<ItemConfig>(ID); 
        public EItemType ItemType => Config.ItemType;
        
        public int ID { [DebuggerStepThrough] get; [DebuggerStepThrough] private init; }
        public bool Dragging { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        public bool Spawning { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        public bool ReallyInWorld => !Dragging && !Spawning;
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public Vector2Int PivotPos { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        // public Vector2 GridLeftUp => PivotPos + new Vector2(-0.5f, 0.5f);
        // public Vector2 GridRightUp => PivotPos + new Vector2(0.5f, 0.5f);
        // public Vector2 GridLeftDown => PivotPos + new Vector2(-0.5f, -0.5f);
        // public Vector2 GridRightDown => PivotPos + new Vector2(0.5f, -0.5f);
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public List<Vector2Int> DeltaPosList { [DebuggerStepThrough] get; private init; }
        public List<ItemDesConfig> EatConfigList { [DebuggerStepThrough] get; private init; }
        public List<ItemDesConfig> AllConfigList => [..Config.DesList, ..EatConfigList];

        public Dictionary<EPropType, long> BuildingOrEventProgress
        {
            get
            {
                Config.BuildPropValueList.ForEach(pair => field.TryAdd(pair.Key, 0));
                return field;
            }
            private init;
        }
        public bool IsBuildingOrEventKanSei =>
            Config.IsBuildingOrEvent && Config.BuildPropValueList.All(pair =>
                BuildingOrEventProgress.TryGetValue(pair.Key, out var progress) && progress >= pair.Value);
        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append($"Name = {Config.Name}, ");
            builder.Append($"Config = {Config}, ");
            builder.Append($"PivotPos = {PivotPos}, ");
            builder.Append($"DeltaPosList = [{string.Join("|", DeltaPosList)}],");
            if (inSpin != null)
            {
                builder.Append($"ModifyPropList = [{string.Join("| ", inSpin.ModifyPropList)}]");
            }
            return builder.ToString();
        }
        
        PlaySpin.MyItem? inSpin;
        public PlaySpin.MyItem this[PlaySpin spin] => inSpin ??= new PlaySpin.MyItem();
        public void DestroyInSpin() => inSpin = null;
    }
}
