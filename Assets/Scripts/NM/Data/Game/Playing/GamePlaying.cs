using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.ViewEvt;
using Sirenix.Utilities;

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
        await ClearDeckAct.Invoke(cts.Token);
        
        
        await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(0), cts.Token);
        await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(1), cts.Token);
        await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), cts.Token);
        await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), cts.Token);
        await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), cts.Token);
        await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), cts.Token);
        while (symbolDeckList.Count < DeckMax)
        {
            await AddSymbolAct.Invoke(SymbolEtt.CreateEmptySymbol(), cts.Token);
        }
        await LaunchAsync<PlayingIdle>();
    }

    public void UnRegisterAll()
    {
        Bus.UnRegister(OnEvtClickSpinAsync);
        Bus.UnRegister(OnEvtSpinSymbolAdjacentSymbolAsync);
    }


    public UniAction<SymbolEtt, SymbolEtt> SymbolAddSymbolAct => new()
    {
        Invoke = async (arg1, arg2, ct) =>
        {
            await AddSymbolAct.Invoke(arg2, ct);
            await Bus.FireAsync(new EvtSpinSymbolAddSymbol(arg1, arg2), ct);
        },
        Des = "符号添加符号",
    };
    public UniAction ClearDeckAct => new()
    {
        Invoke = async (ct) =>
        {
            foreach (var symbol in symbolDeckList.ToList())
            {
                await RemoveSymbolAct.Invoke(symbol, ct);
            }
        },
        Des = "清空符号列表"
    };
    public UniAction<SymbolEtt> AddSymbolAct => new()
    {
        Invoke = async (toAdd, ct) =>
        {
            symbolDeckList.Add(toAdd);
            if (!toAdd.IsEmpty)
            {
                await symbolDeckList.MyFirst(s => s.IsEmpty)
                    .MatchAsync(async some => await RemoveSymbolAct.Invoke(some, ct), RTask);
            }

            await ShowSymbolRandomlyAct.Invoke(toAdd, ct);
        },
        Des = "添加符号"
    };
    public UniAction<SymbolEtt> RemoveSymbolAct => new()
    {
        Invoke = async (toRemove, ct) =>
        {
            symbolDeckList.Remove(toRemove);
            if (symbolDeckList.Count < DeckMax)
            {
                await AddSymbolAct.Invoke(SymbolEtt.CreateEmptySymbol(), ct);
            }
        },
        Des = "移除符号"
    };
    public UniAction<SymbolEtt> ShowSymbolRandomlyAct => new()
    {
        Invoke = async (symbol, ct) =>
        {
            await SymbolShownList
                .Where(s => s.IsEmpty)
                .SelectMany(s => s.Pos.Match(some => [some], Enumerable.Empty<Vector2Int>))
                .ToList()
                .RandomItem()
                .MatchAsync(async some => await ShowSymbolAtAsync.Invoke(symbol, some, ct), RTask);
        },
        Des = "将符号显示在随机一个空位上"
    };
    public UniAction<SymbolEtt, Vector2Int> ShowSymbolAtAsync => new()
    {
        Invoke = async (symbol, pos, ct) =>
        {
            SymbolShownList.Add(symbol);
            symbol.Pos = pos;
            await Bus.FireAsync(new EvtSpinSymbolAt(this, symbol, pos), ct);
        },
        Des = "将符号显示在某位置"
    };

    [UniEvtDes("(香蕉发现和香蕉皮相邻时) 添加一个葡萄酒")]
    UniEvt<EvtSpinSymbolAdjacentSymbol> OnEvtSpinSymbolAdjacentSymbolAsync => (evt, _) =>
    {
        foreach (var s in symbolDeckList)
        {
            if (evt.Symbol.ConfigID == 1 && evt.Symbol == s && evt.AdjacentSymbol.ConfigID == 2)
            {
                InState<PlayingSpin>().MatchA(some =>
                    some.DelayAddList.Add(SymbolAddSymbolAct.Apply(evt.Symbol, SymbolEtt.CreateSymbol(9))));
            }
        }
        return UniTask.CompletedTask;
    };
    // [UniEvtDes("(点击了旋转按钮) 尝试进入旋转状态")]
    UniEvt<EvtClickSpin> OnEvtClickSpinAsync => (evt, ct) =>
    {
        return InState<PlayingSpin>().Match(_ => UniTask.CompletedTask, EnterStateAsync<PlayingSpin>);
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
    public List<UniAction> DelayAddList = [];
    public List<UniAction> DelayDestroyList = [];
    readonly CancellationTokenSource cts = new();


    public override async UniTask OnEnterAsync()
    {
        await Bus.FireAsync(new EvtOnEnterSpin(this), cts.Token);
        await OnSpinAsync(cts.Token);
    }
    
    async UniTask OnSpinAsync(CancellationToken ct)
    {
        BelongFSM.SymbolShownList.Clear();
        foreach (var toShow in BelongFSM.Deck.ToList().ShuffleTo())
        {
            var shownCount = BelongFSM.SymbolShownList.Count;
            if(shownCount == Const.SpinW * Const.SpinH)
                break;
            var addX = shownCount / Const.SpinH + 1;
            var addY = shownCount % Const.SpinH + 1;
            await BelongFSM.ShowSymbolAtAsync.Invoke(toShow, new Vector2Int(addX, addY), ct);
        }

        do
        {
            BelongFSM.SymbolShownList.Sort(SymbolEtt.ByPos);
            DelayAddList.Clear();
            foreach (var symbol in BelongFSM.SymbolShownList.Where(s => !s.AlreadyChecked))
            {
                symbol.AlreadyChecked = true;
                await Bus.FireAsync(new EvtSpinImmediateDoSymbol(symbol), ct);
                foreach (var adjacentSymbol in BelongFSM.GetAdjacent(symbol))
                {
                    var debug = !adjacentSymbol.IsEmpty && !symbol.IsEmpty;
                    await Bus.FireAsync(new EvtSpinSymbolAdjacentSymbol(adjacentSymbol, symbol), ct, () => debug);
                }
            }
            foreach (var doDelay in DelayAddList)
            {
                await doDelay.Invoke(ct);
            }
        } while (DelayAddList.Count != 0);

        
        foreach (var symbol in BelongFSM.SymbolShownList)
        {
            await Bus.FireAsync(new EvtSpinPay(symbol), ct);
        }

        
        
        BelongFSM.Deck.ForEach(s =>
        {
            s.AlreadyChecked = false;
            s.TempAdd.Clear();
            s.TempMulti.Clear();
        });
        MyDebug.Log("Spin End");
        await BelongFSM.EnterStateAsync<PlayingIdle>();
    }
}