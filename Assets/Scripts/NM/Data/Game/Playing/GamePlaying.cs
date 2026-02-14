using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;
[Serializable]
public partial class GamePlaying
{
    public DoSymbolAddSymbol DoSymbolAddSymbol => new() { Ctx = this };
    [ShowInInspector] List<SymbolEtt> symbolDeckList = [];
    public IEnumerable<SymbolEtt> Deck => symbolDeckList;
    public void ClearDeck()
    {
        
    }

    public void AddSymbol(SymbolEtt toAdd)
    {
        toAdd.OnEvt(this).BindAll();
        symbolDeckList.Add(toAdd);
        InState<PlayingSpin>().MatchA(some => some.AddDelayDo(async ct =>
        {
            await some.ShowSymbolRandomlyAsync(toAdd, ct);
        }, EInSpinTiming.AfterAdjacent));
        if(!toAdd.IsEmpty)
            symbolDeckList.MyFirst(s => s.IsEmpty).MatchA(some => symbolDeckList.Remove(some));
    }
    public void RemoveSymbol(SymbolEtt toRemove)
    {
    }
    
    public long Coin;
    public int RemoveToken;
    public int RefreshToken;
    public int NextRentCount;
    public int SpinCount;
    
    CancellationTokenSource cts = new();
    
    
    public override IEnumerable<IFuncWrap> OnEvt()
    {
        yield return Bus.Bind<EvtClickSpin>((evt, ct) =>
        {
            EnterStateIfNotIn<PlayingSpin>();
            return UniTask.CompletedTask;
        });
    }
    
    public override void OnEnter()
    {
        Launch<PlayingInit>();
        Bus.FireAsync(new EvtOnEnterPlaying(), cts.Token).Forget();
    }
    public override void OnExit()
    {
        cts.Cancel();
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
        BelongFSM.AddSymbol(SymbolEtt.CreateSymbol(1));
        BelongFSM.AddSymbol(SymbolEtt.CreateSymbol(1));
        BelongFSM.AddSymbol(SymbolEtt.CreateSymbol(1));
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
    public List<SymbolEtt> SymbolShownList = [];
    List<DoDelay> delayDoList = [];
    CancellationTokenSource cts = new();
    public override void OnEnter()
    {
        Bus.FireAsync(new EvtOnEnterSpin(), cts.Token).ContinueWith(async () =>
        {
            await OnSpinAsync(cts.Token);
        }).Forget();
    }
    public override void OnExit()
    {
        cts.Cancel();
        SymbolShownList.ForEach(s => s.RemoveCom<SymbolInSpin>());
        SymbolShownList.Clear();
    }
    
    public void AddDelayDo(UniAction doFunc, EInSpinTiming timing)
    {
        delayDoList.Add(new DoDelay{ DoAsync = doFunc, DelayTiming = (int)timing });
    }
    
    
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
            where !(x == cx && y == cy)
            select SymbolShownList.First(xs =>
            {
                var xsInSpin = xs.Ctx(this).As<SymbolInSpin>();
                return xsInSpin.Pos.X == x && xsInSpin.Pos.Y == y;
            });
    }

    public async UniTask ShowSymbolRandomlyAsync(SymbolEtt symbol, CancellationToken token)
    {
        var emptyPosList = (
            from s in SymbolShownList
            where s.IsEmpty
            select s.Ctx(this).As<SymbolInSpin>().Pos
            ).ToList();
        if (!emptyPosList.Any())
            return;
        var ranPos = emptyPosList.RandomItem();
        await ShowSymbolAtAsync(symbol, ranPos, token);
    }
    async UniTask ShowSymbolAtAsync(SymbolEtt symbol, Vector2Int pos, CancellationToken token)
    {
        SymbolShownList.Add(symbol);
        symbol.AddCom(new SymbolInSpin() { Pos = pos });
        await Bus.FireAsync(new EvtSpinSymbolAt()
        {
            Arg1 = symbol,
            Arg2 = pos
        }, token);
    }
    
    async UniTask OnSpinAsync(CancellationToken ct)
    {
        var leftList = BelongFSM.Deck.ToList();
        while (leftList.Count > 0)
        {
            var addSymbol = leftList.RandomItem();
            leftList.Remove(addSymbol);
            var shownCount = SymbolShownList.Count;
            if(shownCount == Const.SpinW * Const.SpinH)
                break;
            var addX = shownCount / Const.SpinH + 1;
            var addY = shownCount % Const.SpinH + 1;
            await ShowSymbolAtAsync(addSymbol, new Vector2Int(addX, addY), ct);
        }

        do
        {
            foreach (var symbol in SymbolShownList)
            {
                await Bus.FireAsync(new EvtSpinImmediateDoSymbol { Arg1 = symbol }, ct);
                var adjacentList = GetAdjacent(symbol);
                foreach (var adjacentSymbol in adjacentList)
                {
                    var debug = !adjacentSymbol.IsEmpty && !symbol.IsEmpty;
                    await Bus.FireAsync(new EvtSymbolAdjacentSymbol { Arg1 = adjacentSymbol, Arg2 = symbol }, ct, () => debug);
                }
            }

            foreach (var doDelay in delayDoList.Where(IsSpinTiming(EInSpinTiming.AfterAdjacent)))
            {
                await doDelay.DoAsync(ct);
            }

            delayDoList.RemoveAll(IsSpinTimingP(EInSpinTiming.AfterAdjacent));
        } while (delayDoList.Count != 0);
        MyDebug.Log("Spin End");
        BelongFSM.EnterState<PlayingIdle>();
    }
    
    Func<DoDelay, bool> IsSpinTiming(EInSpinTiming timing) => d => d.DelayTiming == (int)timing;
    Predicate<DoDelay> IsSpinTimingP(EInSpinTiming timing) => d => d.DelayTiming == (int)timing;
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