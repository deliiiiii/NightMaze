using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

[Serializable]
public partial class GamePlaying : GameRoot.StateFSM<GamePlaying>
{
    public override string ToString() => nameof(GamePlaying);
    public string PlayerName = "Deli";
    public double PlayTime;
    public List<SymbolData> SymbolDeckList = [];

    [EvtChanged]
    public partial long Coin {get;set;}
    // public long Coin
    // {
    //     get;
    //     set
    //     {
    //         field = value;
    //         Bus.FireAndForget(new EvtCoinChanged(value));
    //     }
    // }
    // public record EvtCoinChanged(long Value): EvtBase;
    
    public int RemoveToken;
    public int RefreshToken;
    public int NextRentCount;
    public int SpinCount;
    public int DeckMax = 20;

    public ImmutableList<SymbolData> SymbolShownListSorted =>
        SymbolDeckList
            .Where(s => s.Pos.IsSome)
            .OrderBy(s => s.Pos.Match(pos => pos, () => Vector2Int.MaxValue))
            .ToImmutableList();

    protected override async UniTask OnEnterAsync()
    {
        if (CurState == null)
        {
            await new ActAddSymbol { Ctx = this, ToAdd = SymbolData.Create(0) };
            await new ActAddSymbol { Ctx = this, ToAdd = SymbolData.Create(1) };
            await new ActAddSymbol { Ctx = this, ToAdd = SymbolData.Create(1) };
            await new ActAddSymbol { Ctx = this, ToAdd = SymbolData.Create(1) };
            await new ActAddSymbol { Ctx = this, ToAdd = SymbolData.Create(1) };
            await new ActAddSymbol { Ctx = this, ToAdd = SymbolData.Create(2) };
            while (SymbolDeckList.Count < DeckMax)
            {
                await new ActAddSymbol { ToAdd = SymbolData.CreateEmpty(), Ctx = this };
            }
        }
        else
        {
            SymbolDeckList.ForEach(s => s.BindAll());
        }
        await Bus.FireAsync(new EvtOnEnter(this), CurCt);
        await LaunchAsync(CurState ?? new PlayingIdle());
    }
    public record EvtOnEnter(GamePlaying Ctx) : EvtBase;
    protected override void OnExit()
    {
        Release();
        Bus.FireAndForget(new EvtOnExit());
        new ActClearDeck{ Ctx = this }.Forget();
    }
    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);
        PlayTime += dt;
    }

    public record EvtOnExit : EvtBase;

    public IEnumerable<SymbolData> GetAdjacent(SymbolData symbolData)
        => symbolData.Pos.Match(
            thisPos => SymbolShownListSorted.
                Where(other => other.Pos.Match(
                    otherPos => Math.Abs(otherPos.X - thisPos.X) <= 1 && 
                                Math.Abs(otherPos.Y - thisPos.Y) <= 1 &&
                                !(otherPos.X == thisPos.X && otherPos.Y == thisPos.Y),
                RFalse)),
            () => []);

    [DebuggerStepThrough]
    public MyOption<SymbolData> GetEmptyWhere(Func<SymbolData, bool> condition) => SymbolDeckList.MyFirst(s => s.IsEmpty && condition(s));
}