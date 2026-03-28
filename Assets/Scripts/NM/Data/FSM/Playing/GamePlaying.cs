using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
using Sirenix.Utilities;

namespace NM.Data;

[Serializable]
public partial class GamePlaying : RootStateBase<GamePlaying>
{
    [JsonConstructor] GamePlaying() { }
    public GamePlaying(string playerName)
    {
        PlayerName = playerName;
    }
    public override string ToString() => nameof(GamePlaying);
    public string PlayerName { get; private set;}= "Deli";
    public double PlayTime { get; private set;}
    List<Symbol> symbolList = [];
    List<Grid> gridList = [];
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
    // public record EvtCoinChanged(GamePlaying gamePlaying,
    //              long OldValue,
    //              long NewValue): EvtForgetBase;
    
    public IEnumerable<Symbol> SymbolList => symbolList;
    public IEnumerable<Grid> GridList => gridList;

    public IEnumerable<Grid> EmptyGridList
    {
        get
        {
            var posSet = (
                from symbol in symbolList
                select symbol.Pos).ToHashSet();
            return from grid in gridList
                where !posSet.Contains(grid.Pos)
                select grid;
        }
    }

    protected override void OnCreateFreshData()
    {
        (from x in Range(1, 8) 
            from y in Range(1, 8)
            select new Vector2Int(x, y))
            .ForEach(pos =>
            {
                var grid = new Grid(pos);
                AddEttCom(new EttGrid(), grid);
                gridList.Add(grid);
            });
        EmptyGridList
            .Take(5)
            .ForEach(grid =>
            {
                var symbol = new Symbol(grid.Pos);
                AddEttCom(new EttSymbol(), symbol);
                symbolList.Add(symbol);
            });
    }

    protected override async UniTask OnLaunchCom(bool isThisFromLoad)
    {
        // await symbolDeckList.EachOnLaunchCom(isThisFromLoad);
        // await state!.OnCreateAsync(isThisFromLoad);
    }
    protected override void OnReleaseCom()
    {
        // state?.OnRemove();
        // symbolDeckList.EachOnReleaseCom();
    }

    protected override void OnSelfTick(float dt)
    {
        PlayTime += dt;
    }

    Node? state;
    public UniTask ChangeState<T>(T com, bool isNewFromLoad) where T : PlayStateBase<T>
        => _ChangeAsync(ref state, com, isNewFromLoad);
    public MyOption<T> GetStateOptional<T>() where T : PlayStateBase<T>
        => state is T s ? s : None;
}

public abstract class PlayStateBase<T> : Node<GamePlaying, T> where T : PlayStateBase<T>;