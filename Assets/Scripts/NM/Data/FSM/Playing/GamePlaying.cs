using System;
using System.Collections.Generic;
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

    public long Coin
    {
        get;
        set
        {
            field = value;
            Bus.FireAndForget(new EvtCoinChanged(value));
        }
    }
    public record EvtCoinChanged(long Value): EvtBase;
    
    public int RemoveToken;
    public int RefreshToken;
    public int NextRentCount;
    public int SpinCount;
    public int DeckMax = 20;

    public List<SymbolData> SymbolShownList =>
        SymbolDeckList
        .Where(s => s.Pos.Match(_ => true, RFalse))
        .ToList();

    public override async UniTask OnEnterAsync()
    {
        if (!SymbolDeckList.Any())
        {
            await new ActClearDeck() { Ctx = this };
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
        

        await Bus.FireAsync(new EvtOnEnter(this), CurCt);
        await LaunchAsync(new PlayingIdle());
    }
    public record EvtOnEnter(GamePlaying Ctx) : EvtBase;

    public override void OnExit()
    {
        new ActClearDeck{ Ctx = this }.Forget();
        Bus.FireAndForget(new EvtOnExit());
    }

    public override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);
        PlayTime += dt;
    }

    public record EvtOnExit : EvtBase;

    public IEnumerable<SymbolData> GetAdjacent(SymbolData symbolData) 
        => symbolData.Pos.Match(
            pos =>
                from x in Enumerable.Range(pos.X - 1, 3)
                from y in Enumerable.Range(pos.Y - 1, 3)
                where x is >= Const.SpinFirstID and <= Const.SpinW
                where y is >= Const.SpinFirstID and <= Const.SpinH
                where !(x == pos.X && y == pos.Y)
                select SymbolShownList.FirstOrDefault(xs => xs.Pos.Match(some => some.X == x && some.Y == y, RFalse)),
            () => []);

    public MyOption<SymbolData> GetEmpty() => SymbolDeckList.MyFirst(s => s.IsEmpty);
}