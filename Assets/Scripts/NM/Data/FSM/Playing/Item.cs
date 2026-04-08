using System.Collections.Generic;
using System.Diagnostics;
using General;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public abstract partial record MyItem<TSub, TConfig> : IItem
        where TSub : MyItem<TSub, TConfig>
        where TConfig : ItemConfigBase<TConfig>, new()
    {
        protected MyItem(int id, Vector2Int pivotPos)
        {
            ID = id;
            ((IItem)this).PivotPos = pivotPos;
            DeltaPosList = Config.Pos switch
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
        public IEnumerable<Vector2Int> CoveredPosList => DeltaPosList.Select(d => d + ((IItem)this).PivotPos);

        public abstract TConfig Config { get; }
        public abstract EItemType ItemType { get; }
        

        public int ID { [DebuggerStepThrough] get; [DebuggerStepThrough] private init; }
        IItemConfig IItem.Config { [DebuggerStepThrough]get => Config; }
        public bool Dragging { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        public bool Spawning { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public Vector2Int PivotPos { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public IEnumerable<Vector2Int> DeltaPosList { [DebuggerStepThrough] get; private init; }
        public List<ItemDesConfig> EatConfigList { [DebuggerStepThrough] get; private init; }


        PlaySpin.IItem? inSpin;
        public PlaySpin.IItem InSpin(PlaySpin spin) => inSpin ??= CreateInSpin(spin);
        void IItem.DestroyInSpin() => inSpin = null;
        protected abstract PlaySpin.IItem CreateInSpin(PlaySpin spin);
    }

    public interface IItem
    {
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

        PlaySpin.IItem InSpin(PlaySpin spin);
        void DestroyInSpin();
    }
}
