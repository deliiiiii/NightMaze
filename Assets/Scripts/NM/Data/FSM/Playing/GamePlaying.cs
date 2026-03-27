using System;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
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
    // List<SymbolData> symbolDeckList = [];
    // [EvtChanged]
    // public partial long Coin { get; private set;}
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
    public int DeckMax{ get; private set;} = 20;
    
    protected override void OnCreateFreshData()
    {
        // List<SymbolData> initDeck = 
        // [
            // SymbolData.Create(0),
            // SymbolData.Create(1),
            // SymbolData.Create(1),
            // SymbolData.Create(1),
            // SymbolData.Create(1),
            // SymbolData.Create(2)
        // ];
        // symbolDeckList = [..initDeck, ..SymbolData.CreateEmpty.Repeat(DeckMax - initDeck.Count)];
        // state = new PlayingIdle();
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
        => _ChangeAsync(this, ref state, com, isNewFromLoad);
    public MyOption<T> GetStateOptional<T>() where T : PlayStateBase<T>
        => state is T s ? s : None;
}

public abstract class PlayStateBase<T> : Node<GamePlaying, T> where T : PlayStateBase<T>;