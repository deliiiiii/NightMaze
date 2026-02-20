using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.ViewEvt;
using Sirenix.OdinInspector;

namespace NM.Data;

[Serializable]
public class GamePlaying : FSM<GamePlaying>
{
    public override string ToString() => nameof(GamePlaying);

    List<SymbolEtt> symbolDeckList = [];
    public IEnumerable<SymbolEtt> Deck => symbolDeckList;
    public long Coin;
    public int RemoveToken;
    public int RefreshToken;
    public int NextRentCount;
    public int SpinCount;
    public int DeckMax = 20;

    public List<SymbolEtt> SymbolShownList = [];
    CancellationTokenSource cts = new();
    
    public GamePlaying()
    {
        Init().Forget();
    }

    async UniTask Init()
    {
        Bus.Register(OnEvtClickSpinAsync);
        Bus.Register(OnEvtSpinSymbolAdjacentSymbolAsync);
        
        await Bus.FireAsync(new EvtOnEnterPlaying(this), cts.Token);
        await ClearDeckAsync(cts.Token);
        
        
        await AddSymbolAsync(SymbolEtt.CreateSymbol(0), cts.Token);
        await AddSymbolAsync(SymbolEtt.CreateSymbol(1), cts.Token);
        await AddSymbolAsync(SymbolEtt.CreateSymbol(1), cts.Token);
        await AddSymbolAsync(SymbolEtt.CreateSymbol(1), cts.Token);
        await AddSymbolAsync(SymbolEtt.CreateSymbol(1), cts.Token);
        await AddSymbolAsync(SymbolEtt.CreateSymbol(1), cts.Token);
        await AddSymbolAsync(SymbolEtt.CreateSymbol(2), cts.Token);
        while (symbolDeckList.Count < DeckMax)
        {
            await AddSymbolAsync(SymbolEtt.CreateEmptySymbol(), cts.Token);
        }
        await LaunchAsync<PlayingIdle>();
    }

    public void UnRegisterAll()
    {
        Bus.UnRegister(OnEvtClickSpinAsync);
        Bus.UnRegister(OnEvtSpinSymbolAdjacentSymbolAsync);
    }


    [ActionDes("符号添加符号")]
    public async UniTask SymbolAddSymbolAsync(SymbolEtt arg1, SymbolEtt arg2, CancellationToken ct)
    {
        await AddSymbolAsync(arg2, ct);
        await Bus.FireAsync(new EvtSpinSymbolAddSymbol(arg1, arg2), ct);
    }

    [ActionDes("清空符号列表")]
    public async UniTask ClearDeckAsync(CancellationToken ct)
    {
        foreach (var symbol in symbolDeckList.ToList())
        {
            await RemoveSymbol(symbol, ct);
        }
    }

    [ActionDes("添加符号")]
    async UniTask AddSymbolAsync(SymbolEtt toAdd, CancellationToken ct)
    {
        symbolDeckList.Add(toAdd);
        if (!toAdd.IsEmpty)
        {
            await symbolDeckList.MyFirst(s => s.IsEmpty)
                .MatchAsync(async some => await RemoveSymbol(some, ct), RTask);
        }
        await ShowSymbolRandomlyAsync(toAdd, ct);
    }

    [ActionDes("移除符号")]
    async UniTask RemoveSymbol(SymbolEtt toRemove, CancellationToken ct)
    {
        symbolDeckList.Remove(toRemove);
        if (symbolDeckList.Count < DeckMax)
        {
            await AddSymbolAsync(SymbolEtt.CreateEmptySymbol(), ct);
        }
    }
    
    public async UniTask ShowSymbolRandomlyAsync(SymbolEtt symbol, CancellationToken ct)
    {
        await 
            (
                from s in SymbolShownList
                where s.IsEmpty
                from pos in s.Pos.Match(some => [some], Enumerable.Empty<Vector2Int>)
                select pos
            )
            .ToList()
            .RandomItem()
            .MatchAsync(async some => await ShowSymbolAtAsync(symbol, some, ct), RTask);

    }
    
    [ActionDes("将符号显示在某位置")]
    public async UniTask ShowSymbolAtAsync(SymbolEtt symbol, Vector2Int pos, CancellationToken ct)
    {
        SymbolShownList.Add(symbol);
        symbol.Pos = pos;
        await Bus.FireAsync(new EvtSpinSymbolAt(this, symbol, pos), ct);
    }
    

    [UniEvtDes("(香蕉发现和香蕉皮相邻时) 添加一个葡萄酒")]
    UniEvt<EvtSpinSymbolAdjacentSymbol> OnEvtSpinSymbolAdjacentSymbolAsync => (evt, _) =>
    {
        foreach (var s in symbolDeckList)
        {
            if (evt.Symbol.ConfigID == 1 && evt.Symbol == s && evt.AdjacentSymbol.ConfigID == 2)
            {
                InState<PlayingSpin>().MatchA(some =>
                {
                    some.DelayDoList.Add(ct => SymbolAddSymbolAsync(s, SymbolEtt.CreateSymbol(9), ct));
                });
            }
        }
        return UniTask.CompletedTask;
    };

    [UniEvtDes("(点击了旋转按钮) 尝试进入旋转状态")]
    UniEvt<EvtClickSpin> OnEvtClickSpinAsync => (evt, ct) =>
    {
        return InState<PlayingSpin>()
            .Match<UniTask>(_ => UniTask.CompletedTask, async () => await EnterStateAsync<PlayingSpin>());
    };


    public IEnumerable<SymbolEtt> GetAdjacent(SymbolEtt symbolEtt)
        => symbolEtt.Pos.Match(pos =>
        {
            var cx = pos.X;
            var cy = pos.Y;
            return
                from x in Enumerable.Range(cx - 1, 3)
                from y in Enumerable.Range(cy - 1, 3)
                where x is >= Const.SpinFirstID and <= Const.SpinW
                where y is >= Const.SpinFirstID and <= Const.SpinH
                where !(x == cx && y == cy)
                select SymbolShownList.FirstOrDefault(xs => xs.Pos.Match(some => some.X == x && some.Y == y, RFalse));
        }, () => []);
}

[Serializable]
public class PlayingIdle : GamePlaying.StateFSM<PlayingIdle>;
[Serializable]
public class PlayingSpin : GamePlaying.StateFSM<PlayingSpin>
{
    public readonly List<UniAction> DelayDoList = [];
    readonly CancellationTokenSource cts = new();

    IEnumerable<SymbolEtt> Deck => BelongFSM.Deck;
    List<SymbolEtt> SymbolShownList => BelongFSM.SymbolShownList;

    public override async UniTask OnEnterAsync()
    {
        await Bus.FireAsync(new EvtOnEnterSpin(this), cts.Token);
        await OnSpinAsync(cts.Token);
    }
    
    async UniTask OnSpinAsync(CancellationToken ct)
    {
        SymbolShownList.Clear();
        var toShowList = Deck.ToList().ShuffleTo();
        foreach (var toShow in toShowList)
        {
            var shownCount = SymbolShownList.Count;
            if(shownCount == Const.SpinW * Const.SpinH)
                break;
            var addX = shownCount / Const.SpinH + 1;
            var addY = shownCount % Const.SpinH + 1;
            await BelongFSM.ShowSymbolAtAsync(toShow, new Vector2Int(addX, addY), ct);
        }

        do
        {
            foreach (var symbol in SymbolShownList)
            {
                await Bus.FireAsync(new EvtSpinImmediateDoSymbol(symbol), ct);
                foreach (var adjacentSymbol in BelongFSM.GetAdjacent(symbol))
                {
                    var debug = !adjacentSymbol.IsEmpty && !symbol.IsEmpty;
                    await Bus.FireAsync(new EvtSpinSymbolAdjacentSymbol(adjacentSymbol, symbol), ct, () => debug);
                }
            }
            foreach (var doDelay in DelayDoList)
            {
                await doDelay(ct);
            }
            // DelayDoList.RemoveAll(IsSpinTimingP(InSpinTiming.AfterAdjacent));
            DelayDoList.Clear();
        } while (DelayDoList.Count != 0);
        MyDebug.Log("Spin End");
        await BelongFSM.EnterStateAsync<PlayingIdle>();
    }
}