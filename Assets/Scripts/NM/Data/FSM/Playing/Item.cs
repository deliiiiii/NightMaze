using System.Collections.Generic;
using System.Diagnostics;
using General;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public abstract partial class MyItem<TEtt, TSub, TConfig> : ComBase<TEtt, TSub>, IItem
        where TEtt : EttBase<TEtt>, new()
        where TSub : MyItem<TEtt, TSub, TConfig>
        where TConfig : ItemConfigBase<TConfig>, new()
    {
        protected MyItem(TEtt belongEtt, int id, Vector2Int pivotPos) : base(belongEtt)
        {
            ID = id;
            PivotPos = pivotPos;
            DeltaPosList = Config.Pos switch
            {
                ItemPosRectangle rect => (
                    from x in Range(0, rect.Length)
                    from y in Range(0, rect.Height)
                    select new Vector2Int(x, y)).ToList(),
                ItemPosCustom custom => custom.DeltaPosList,
                _ => [Vector2Int.Zero],
            };
        }

        public int ID;
        public bool Dragging;
        public bool Spawning;

        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public Vector2Int PivotPos;
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public List<Vector2Int> DeltaPosList { get; init; }
        public List<ItemDesConfig> EatConfigList = [];

        public bool CoverPos(Vector2Int pos) => CoveredPosList.Contains(pos);
        public IEnumerable<Vector2Int> CoveredPosList => DeltaPosList.Select(d => d + PivotPos);

        public abstract TConfig Config { get; }
        public abstract EItemType ItemType { get; }
        

        EttBase IItem.BelongEtt { [DebuggerStepThrough]get => BelongEtt; }
        int IItem.ID { [DebuggerStepThrough]get => ID; }
        bool IItem.Dragging { [DebuggerStepThrough]get => Dragging; [DebuggerStepThrough]set => Dragging = value; }
        bool IItem.Spawning { [DebuggerStepThrough]get => Spawning; [DebuggerStepThrough]set => Spawning = value; }
        Vector2Int IItem.PivotPos
        {
            [DebuggerStepThrough] get => PivotPos;
            [DebuggerStepThrough] set => PivotPos = value;
        }
        IEnumerable<Vector2Int> IItem.DeltaPosList { [DebuggerStepThrough]get => DeltaPosList; }
        [DebuggerStepThrough] bool IItem.CoverPos(Vector2Int pos) => CoverPos(pos);
        IEnumerable<Vector2Int> IItem.CoveredPosList { [DebuggerStepThrough]get => CoveredPosList; }
        IItemConfig IItem.Config { [DebuggerStepThrough]get => Config; }
        EItemType IItem.ItemType { [DebuggerStepThrough]get => ItemType; }
        List<ItemDesConfig> IItem.EatConfigList { [DebuggerStepThrough]get => EatConfigList; }
        [DebuggerStepThrough] public override string ToString() 
            => $"{GetType().Name}(ID: {ID}, PivotPos: {PivotPos}, DeltaPosList: [{string.Join(", ", DeltaPosList)}])";
    }

    public interface IItem
    {
        EttBase BelongEtt { get; }
        int ID { get; }
        bool Dragging { get; set; }
        bool Spawning { get; set; }
        bool ReallyInWorld => !Dragging && !Spawning;
        Vector2Int PivotPos { get; set; }
        IEnumerable<Vector2Int> DeltaPosList { get; }
        bool CoverPos(Vector2Int pos);
        IEnumerable<Vector2Int> CoveredPosList { get; }
        IItemConfig Config { get; }
        EItemType ItemType { get; }
        List<ItemDesConfig> EatConfigList { get; }
    }
}
