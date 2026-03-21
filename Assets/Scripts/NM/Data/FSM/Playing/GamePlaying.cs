using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
namespace NM.Data;

[Serializable]
public partial class GamePlaying : CompositeBase<GameRoot, GamePlaying>
{
    protected override List<HashSet<Type>> MutexListSet => 
    [
        [typeof(PlayingIdle), typeof(PlayingSpin)]
    ];
    [JsonConstructor] GamePlaying() { }
    public GamePlaying(string playerName)
    {
        PlayerName = playerName;
    }
    public override string ToString() => nameof(GamePlaying);
    public string PlayerName { get; private set;}= "Deli";
    public double PlayTime { get; private set;}
    List<SymbolData> symbolDeckList = [];
    [EvtChanged]
    public partial long Coin { get; private set;}
    // 标注[EvtChanged]则源生↓↓↓
    // public long Coin
    // {
    //     get;
    //     private set
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
    
    public IEnumerable<SymbolData> SymbolDeck => symbolDeckList;
    public IEnumerable<SymbolData> SymbolRandomly => symbolDeckList.ShuffleTo();
    public IEnumerable<SymbolData> SymbolShownSorted => 
        from symbol in symbolDeckList
        from pos in symbol.Pos.ToIEnumerable()
        orderby pos
        select symbol;
    public IEnumerable<SymbolData> GetAdjacent(SymbolData symbolData) =>
        from thisPos in symbolData.Pos.ToIEnumerable()
        from other in symbolDeckList
        from otherPos in other.Pos.ToIEnumerable()
        where Math.Abs(otherPos.X - thisPos.X) <= 1 &&
              Math.Abs(otherPos.Y - thisPos.Y) <= 1 &&
              !(otherPos.X == thisPos.X && otherPos.Y == thisPos.Y)
        orderby otherPos
        select other;

    public override async UniTask OnAddAsync(bool isThisFromLoad)
    {
        await base.OnAddAsync(isThisFromLoad);
        if(!isThisFromLoad)
        {
            List<SymbolData> initDeck = 
            [
                SymbolData.Create(0),
                SymbolData.Create(1),
                SymbolData.Create(1),
                SymbolData.Create(1),
                SymbolData.Create(1),
                SymbolData.Create(2)
            ];
            symbolDeckList = [..initDeck, ..SymbolData.CreateEmpty.Repeat(DeckMax - initDeck.Count)];
        }
        else
        {
            await symbolDeckList.ForEachAsync(async s => await s.OnAddAsync(true));
        }
        await new EvtOnEnter(this);
        await (isThisFromLoad 
            ? AddComAsync(new PlayingIdle(), false) 
            : AllComOnAddAsync());
    }
    public record EvtOnEnter(GamePlaying WhoHasCt) : EvtBase<GamePlaying>(WhoHasCt);
    public override void OnRemove()
    {
        new EvtOnExit().Forget();
        symbolDeckList.ForEach(s => s.OnRemove());
        base.OnRemove();
    }
    public record EvtOnExit : EvtForgetBase;
    public override void OnUpdate(float dt)
    {
        PlayTime += dt;
    }
}