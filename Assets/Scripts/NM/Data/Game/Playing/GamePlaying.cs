using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using R3;

namespace NM.Data;
[Serializable]
public partial class GamePlaying
{
    List<SymbolEtt> symbolDeckList = [];
    public long Coin;
    public int RemoveToken;
    public int RefreshToken;
    public int NextRentCount;
    public int SpinCount;

    public void ClearDeck()
    {
        
    }

    public void AddSymbol(SymbolEtt toAdd)
    {
        
    }

    public void RemoveSymbol(SymbolEtt toRemove)
    {
        
    }
    
    public override void OnEnter()
    {
        Launch<PlayingInit>();
    }
    public override void OnExit()
    {
        Release();
    }
}
[Serializable]
public class PlayingInit : GamePlaying.StateFSM<PlayingInit>
{
    public override void OnEnter()
    {
        MyDebug.Log($"{nameof(PlayingInit)} OnEnter");
    }
    public override void OnExit()
    {
        BelongFSM.ClearDeck();
    }
}
[Serializable]
public class PlayingBeforeSpin : GamePlaying.StateFSM<PlayingBeforeSpin>
{
    public override void OnEnter()
    {
    }
    public override void OnExit()
    {
    }
}
[Serializable]
public class PlayingSpin : GamePlaying.StateFSM<PlayingSpin>
{
    IEnumerable<SymbolEtt> GetAdjacent(SymbolEtt symbolEtt)
    {
        var cx = symbolEtt.Pos.x;
        var cy = symbolEtt.Pos.y;
        List<int> xRange = [cx - 1, cx, cx + 1];
        List<int> yRange = [cy - 1, cy, cy + 1];
        return
            from x in xRange
            from y in yRange
            where x is >= 1 and <= Const.SpinW
            where y is >= 1 and <= Const.SpinH
            select SymbolShownList.First(xs => xs.Pos.x == x && xs.Pos.y == y);
    }
    public List<SymbolEtt> SymbolShownList = [];
    public bool TestToggle;
    List<IAction> adjacentActList = [];
    List<IAction> removeActList = [];
    public override void OnEnter()
    {
        OnSpin().Forget();
    }
    public override void OnExit()
    {
        SymbolShownList.Clear();
    }

    async UniTask OnSpin()
    {
        foreach (var (symbolEtt, adjacentSymbol) in 
                 from symbolEtt in SymbolShownList 
                 from adjacentSymbol in GetAdjacent(symbolEtt)
                 select (symbolEtt, adjacentSymbol))
        {
            EvtBus.Fire(new AdjacentEvent(symbolEtt, adjacentSymbol));
            await UniTask.WaitUntil(() => TestToggle);
            await UniTask.WaitUntil(() => adjacentActList.Count == 0);
        }
    }
    public interface IAction
    {
        UniTask Do();
    }
}
public record AdjacentEvent(SymbolEtt Symbol, SymbolEtt AdjacentSymbol) : EvtBase;


[Serializable]
public class PlayingAfterSpin : GamePlaying.StateFSM<PlayingAfterSpin>
{
    public override void OnEnter()
    {
    }
    public override void OnExit()
    {
    }
}