using System;
using GeneralPreview;
using NM.Config;

namespace NM.Data;
public partial class GamePlaying
{
    public partial class Grid : MyItem<Grid, GridConfig>
    {
        public Grid(int id, Vector2Int pivotPos) : base(id, pivotPos) { }
        public override GridConfig Config => 
            field ??= RefPoolMulti<GridConfig>.AcquireOne(c => c.ID == ID) 
                      ?? RefPoolMulti<GridConfig>.AcquireFirst()
                      ?? throw new Exception($"GridConfig 一个配置也没有.");
        public sealed override EItemType ItemType => EItemType.Grid;
        protected override PlaySpin.IItem CreateInSpin(PlaySpin spin) => new PlaySpin.Grid(spin, this);
    }
}