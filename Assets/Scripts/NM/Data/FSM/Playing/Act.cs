using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;
[ActContainer]
public partial class GamePlaying
{
    [Obsolete("某地块放置在某位置")]
    UniTask SetGridAtPosAsync(Grid grid, Vector2Int pos, CancellationToken ct)
    {
        grid.PivotPos = pos;
        return UniTask.CompletedTask;
    }   
    [Obsolete("某符号放置在某位置")]
    UniTask SetSymbolAtPosAsync(Symbol symbol, Vector2Int pos, CancellationToken ct)
    {
        symbol.PivotPos = pos;
        return UniTask.CompletedTask;
    }
}