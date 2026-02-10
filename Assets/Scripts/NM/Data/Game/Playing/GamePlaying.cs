using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace NM.Data;
[Serializable]
public partial class GamePlaying
{
    [ShowInInspector] List<SymbolEtt> symbolDeckList = [];
    public IEnumerable<SymbolEtt> Deck => symbolDeckList;
    public void ClearDeck()
    {
        
    }

    public void AddSymbol(SymbolEtt toAdd)
    {
        toAdd.OnEvtList().ForEach(w => w.Register());
        symbolDeckList.Add(toAdd);
    }
    public void RemoveSymbol(SymbolEtt toRemove)
    {
    }
    
    public long Coin;
    public int RemoveToken;
    public int RefreshToken;
    public int NextRentCount;
    public int SpinCount;
    
    public override void OnEnter()
    {
        Launch<PlayingInit>();
        EvtBus.FireAsync(new EvtOnEnterPlaying()).Forget();
    }
    public override void OnExit()
    {
        Release();
    }
}
[Serializable]
public class PlayingIdle : GamePlaying.StateFSM<PlayingIdle>
{
    public override void OnEnter()
    {
    }
    public override void OnExit()
    {
    }
}
[Serializable]
public class PlayingInit : GamePlaying.StateFSM<PlayingInit>
{
    [Button]
    public void EnterSpin () => BelongFSM.EnterState<PlayingSpin>();
    
    IEnumerable<SymbolEtt> Deck => BelongFSM.Deck;
    public override void OnEnter()
    {
        MyDebug.Log($"{nameof(PlayingInit)} OnEnter");
        FillDeckWithInitSymbols();
        FillDeckWithEmpty();
    }
    public override void OnExit()
    {
        BelongFSM.ClearDeck();
    }

    void FillDeckWithInitSymbols()
    {
        BelongFSM.AddSymbol(SymbolEtt.CreateSymbol(0));
        BelongFSM.AddSymbol(SymbolEtt.CreateSymbol(1));
        BelongFSM.AddSymbol(SymbolEtt.CreateSymbol(2));
    }
    void FillDeckWithEmpty()
    {
        while (Deck.Count() < Const.DeckMax)
        {
            BelongFSM.AddSymbol(SymbolEtt.CreateEmptySymbol());
        }
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
        var symbolInSpin = symbolEtt.Ctx(this).As<SymbolInSpin>();
        var cx = symbolInSpin.Pos.X;
        var cy = symbolInSpin.Pos.Y;
        List<int> xRange = [cx - 1, cx, cx + 1];
        List<int> yRange = [cy - 1, cy, cy + 1];
        return
            from x in xRange
            from y in yRange
            where x is >= Const.SpinFirstID and <= Const.SpinW
            where y is >= Const.SpinFirstID and <= Const.SpinH
            select SymbolShownList.First(xs =>
            {
                var xsInSpin = xs.Ctx(this).As<SymbolInSpin>();
                return xsInSpin.Pos.X == x && xsInSpin.Pos.Y == y;
            });
    }
    public List<SymbolEtt> SymbolShownList = [];
    public bool TestToggle;
    List<IAction> adjacentActList = [];
    List<IAction> removeActList = [];
    CancellationTokenSource onSpinCts = new();
    public override void OnEnter()
    {
        OnSpinAsync(onSpinCts.Token).Forget();
    }
    public override void OnExit()
    {
        onSpinCts.Cancel();
        SymbolShownList.Clear();
    }

    async UniTask OnSpinAsync(CancellationToken token)
    {
        var leftList = BelongFSM.Deck.ToList();
        while (leftList.Count > 0)
        {
            var addSymbol = leftList.RandomItem();
            var shownCount = SymbolShownList.Count;
            var addX = shownCount / Const.SpinH + 1;
            var addY = shownCount % Const.SpinH + 1;
            var addPos = new Vector2Int(addX, addY);
            addSymbol.AddCom(new SymbolInSpin() { Pos = addPos });
            leftList.Remove(addSymbol);
            SymbolShownList.Add(addSymbol);
            await EvtBus.FireAsync(new EvtSpinSymbolAt(addSymbol, addPos));
        }
       
        var pairList =
            (
                from symbolEtt in SymbolShownList
                from adjacentSymbol in GetAdjacent(symbolEtt)
                select (symbolEtt, adjacentSymbol)).ToList();
        foreach (var (symbolEtt, adjacentSymbol) in pairList)
        {
            await EvtBus.FireAsync(new EvtAdjacent(symbolEtt, adjacentSymbol));
            // await UniTask.WaitUntil(() => TestToggle, cancellationToken: token);
            // TestToggle = false;
            // await UniTask.WaitUntil(() => adjacentActList.Count == 0, cancellationToken: token);
        }
    }
    public interface IAction
    {
        UniTask Do();
    }
}



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