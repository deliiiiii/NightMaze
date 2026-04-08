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

    [Obsolete("在某位置生成某物体")]
    UniTask SpawnItemAtPosAsync(IItem toSpawn, Vector2Int pos, CancellationToken ct)
    {
        // TODO
        return UniTask.CompletedTask;
    }
    
    [Obsolete("移除某物体")]
    UniTask RemoveItemAsync(IItem toRemove, CancellationToken ct)
    {
        // TODO
        return UniTask.CompletedTask;
    }

    [Obsolete("某物添加某物的词条")]
    UniTask ItemEatItemConfigAsync(IItem whoEat, IItem toEat, CancellationToken ct)
    {
        whoEat.EatConfigList.AddRange(toEat.Config.DesList);
        return UniTask.CompletedTask;
    }
    
}