using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using General;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;

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
        }
        [DebuggerStepThrough] public bool CoverPos(Vector2Int pos) => CoveredPosList.Contains(pos);
        public IEnumerable<Vector2Int> CoveredPosList => DeltaPosList.Select(d => d + PivotPos);
        public ItemConfig Config => field ??= RefPoolMulti<ItemConfig>.AcquireOne(c => c.ID == ID) 
                                        ?? RefPoolMulti<ItemConfig>.AcquireFirst()
                                        ?? throw new Exception($"ItemConfig 一个配置也没有.");
        public EItemType ItemType => Config.ItemType;
        
        public int ID { [DebuggerStepThrough] get; [DebuggerStepThrough] private init; }
        public bool Dragging { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        public bool Spawning { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        public bool ReallyInWorld => !Dragging && !Spawning;
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public Vector2Int PivotPos { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public List<Vector2Int> DeltaPosList { [DebuggerStepThrough] get; private init; }
        public List<ItemDesConfig> EatConfigList { [DebuggerStepThrough] get; private init; }
        public List<ItemDesConfig> AllConfigList => [..Config.DesList, ..EatConfigList];


        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append($"Name = {Config.Name}, ");
            builder.Append($"Config = {Config}, ");
            builder.Append($"PivotPos = {PivotPos}, ");
            builder.Append($"DeltaPosList = [{string.Join(", ", DeltaPosList)}]");
            if (inSpin != null)
            {
                builder.Append($"ModifyPropList = [{string.Join(", ", inSpin.ModifyPropList)}]");
            }
            return builder.ToString();
        }
        
        PlaySpin.MyItem? inSpin;
        public PlaySpin.MyItem InSpin(PlaySpin spin) => inSpin ??= new();
        public void DestroyInSpin() => inSpin = null;
    }
}
