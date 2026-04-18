using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using General;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.Utilities;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Data;

public partial class GamePlaying
{
    [DebuggerStepThrough]
    public partial class MyItem
    {
        [JsonConstructor] MyItem()
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
        public bool CoverPos(Vector2Int pos) => CoveredPosList.Contains(pos);
        public IEnumerable<Vector2Int> CoveredPosList => DeltaPosList.Select(d => d + PivotPos);
        public ItemConfig Config => field ??= ConfigLoader.Acquire<ItemConfig>(ID); 
        public EItemType ItemType => Config.ItemType;

        public int ID { get; private set; }
        public bool Dragging;
        public bool Spawning;
        public bool ReallyInWorld => !Dragging && !Spawning;

        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public Vector2Int PivotPos;
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public List<Vector2Int> DeltaPosList;
        public List<ItemDesConfig> EatConfigList;
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
                builder.Append($"DistributePropList = [{string.Join("| ", inSpin.DistributePropList)}]");
            }
            return builder.ToString();
        }
        
        PlaySpin.MyItem? inSpin;
        public PlaySpin.MyItem this[PlaySpin spin] => inSpin ??= new PlaySpin.MyItem();
        public void DestroyInSpin() => inSpin = null;
    }
}
