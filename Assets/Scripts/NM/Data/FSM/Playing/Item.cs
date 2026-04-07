using System.Collections.Generic;
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

        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public Vector2Int PivotPos;
        [JsonConverter(typeof(CompactFormatNoRefConverter))]
        public List<Vector2Int> DeltaPosList { get; init; }

        public bool CoverPos(Vector2Int pos)
            => CoveredPosList.Contains(pos);

        public IEnumerable<Vector2Int> CoveredPosList
            => DeltaPosList.Select(d => d + PivotPos);

        public abstract TConfig Config { get; }

        EttBase IItem.BelongEtt => BelongEtt;
        int IItem.ID => ID;
        Vector2Int IItem.PivotPos => PivotPos;
        IEnumerable<Vector2Int> IItem.DeltaPosList => DeltaPosList;
        bool IItem.CoverPos(Vector2Int pos) => CoverPos(pos);
        IEnumerable<Vector2Int> IItem.CoveredPosList => CoveredPosList;
        IItemConfig IItem.Config => Config;
    }

    public interface IItem
    {
        EttBase BelongEtt { get; }
        int ID { get; }
        Vector2Int PivotPos { get; }
        IEnumerable<Vector2Int> DeltaPosList { get; }
        bool CoverPos(Vector2Int pos);
        IEnumerable<Vector2Int> CoveredPosList { get; }
        IItemConfig Config { get; }
    }
}
