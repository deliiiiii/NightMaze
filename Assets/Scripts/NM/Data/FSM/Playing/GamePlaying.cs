using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

[Serializable]
public partial record GamePlaying : GameRoot.StateFSM<GamePlaying>
{
    public GamePlaying(string playerName)
    {
        PlayerName = playerName;
    }
    public override string ToString() => nameof(GamePlaying);
    public string PlayerName { get; private set;}= "Deli";
    public double PlayTime { get; private set;}
    List<SymbolData> symbolDeckList = [];
    public long Coin { get; private set;}
    // 标注[EvtChanged]则源生↓↓↓
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
    public int RemoveToken{ get; private set;}
    public int RefreshToken{ get; private set;}
    public int NextRentCount{ get; private set;}
    public int SpinCount{ get; private set;}
    public int DeckMax{ get; private set;} = 20;
    
    public ImmutableList<SymbolData> SymbolDeck => symbolDeckList.ToImmutableList();
    public ImmutableList<SymbolData> SymbolShownSorted =>
        symbolDeckList
            .Where(s => s.Pos.IsSome)
            .OrderBy(s => s.Pos.Match(pos => pos, () => Vector2Int.MaxValue))
            .ToImmutableList();
    public IEnumerable<SymbolData> GetAdjacent(SymbolData symbolData)
        => symbolData.Pos.Match(
            thisPos => SymbolShownSorted.
                Where(other => other.Pos.Match(
                    otherPos => Math.Abs(otherPos.X - thisPos.X) <= 1 && 
                                Math.Abs(otherPos.Y - thisPos.Y) <= 1 &&
                                !(otherPos.X == thisPos.X && otherPos.Y == thisPos.Y),
                    RFalse)),
            () => []);
    
    protected override async UniTask OnEnterAsync(bool isThisFromLoad)
    {
        if (!isThisFromLoad)
        {
            symbolDeckList = 
            [
                SymbolData.Create(0),
                SymbolData.Create(1),
                SymbolData.Create(1),
                SymbolData.Create(1),
                SymbolData.Create(1),
                SymbolData.Create(2)
            ];
            symbolDeckList.AddRange(SymbolData.CreateEmpty.Repeat(DeckMax - symbolDeckList.Count));
        }
        symbolDeckList.ForEach(s => s.BindAllCom());
        await Bus.FireAsync(new EvtOnEnter(this), CurCt);
        await LaunchAsync(CurState ?? new PlayingIdle(), CurState != null);
    }
    public record EvtOnEnter(GamePlaying Ctx) : EvtBase;
    protected override void OnExit()
    {
        Release();
        Bus.FireAndForget(new EvtOnExit());
        symbolDeckList.ForEach(s => s.Dispose());
        // new ActClearDeck{ Ctx = this }.Forget();
    }
    public record EvtOnExit : EvtBase;
    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);
        PlayTime += dt;
    }
}