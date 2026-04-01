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
    [EvtChanged]public partial long Prop1 { get;private set; }
    [EvtChanged]public partial long Prop2 { get;private set; }
    [EvtChanged]public partial long Prop3 { get;private set; }
    [EvtChanged]public partial long Prop4 { get;private set; }
    [EvtChanged]public partial long Prop5 { get;private set; }
    [EvtChanged]public partial long Prop6 { get;private set; }
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

    public IEnumerable<Symbol> Symbols => GetComs<Symbol>();
    public IEnumerable<Grid> Grids => GetComs<Grid>();

    public IEnumerable<Grid> EmptyGrids
    {
        get
        {
            var posSet = (
                from symbol in Symbols
                select symbol.Pos).ToHashSet();
            return from grid in Grids
                where !posSet.Contains(grid.Pos)
                select grid;
        }
    }

    protected override void OnCreateFreshData()
    {
        (from x in Range(1, 8) 
            from y in Range(1, 8)
            select new Vector2Int(x, y))
            .ForEach(pos => AddEttCom(EttGrid.Create(), new Grid(pos)));
        EmptyGrids
            .ToList()
            .Take(5)
            .ForEach(grid => AddEttCom(EttSymbol.Create(), new Symbol(grid.Pos)));
        // state = new play
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
}

public abstract class PlayStateBase<T> : Node<GamePlaying, T> where T : PlayStateBase<T>;