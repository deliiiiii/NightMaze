using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;

[Serializable]
public partial record PlayingSpin : GamePlaying.StateFSM<PlayingSpin>
{
    public override string ToString() => nameof(PlayingSpin);

    List<ICanAwait> doList = [];
    public IEnumerable<ICanAwait> DoList => doList;
    void InsertBeforeType(IEnumerable<ICanAwait> toInsert, Type type)
    {
        var id = doList.FindIndex(d => d.GetType() == type);
        if (id == -1)
            id = 0;
        doList.InsertRange(id, toInsert);
    }
    public void InsertHead(IEnumerable<ICanAwait> toInsert) => doList.InsertRange(0, toInsert);
    public void InsertHead(ICanAwait toInsert) => InsertHead([toInsert]);
    public void InsertBeforeCheckUnchecked(IEnumerable<ICanAwait> toInsert) => InsertBeforeType(toInsert, typeof(ActWillCheckUncheckedSymbol));
    public void InsertBeforeCheckUnchecked(ICanAwait toInsert) => InsertBeforeCheckUnchecked([toInsert]);

    IEnumerable<ICanAwait> GetDoList()
    {
        BelongFSM.SymbolDeck.ForEach(s =>
        {
            s.AlreadyChecked = false;
            s.TempAdd.Clear();
            s.TempMulti.Clear();
            s.Pos.MatchA(some => s.Pos = None);
        });
        var showSymbolActs = BelongFSM.SymbolRandomly
            .Take(Const.SpinW * Const.SpinH)
            .Select((toShow, index) => new GamePlaying.ActShowSymbolAt(BelongFSM)
            {
                Symbol = toShow,
                Pos = new Vector2Int(index / Const.SpinH + 1, index % Const.SpinH + 1)
            });
        foreach (var item in showSymbolActs)
            yield return item;
        yield return new ActWillCheckUncheckedSymbol(this);
        yield return new ActWillPayShownSymbol(this);
        yield return new ActEnterIdle(this);
    }
    protected override async UniTask OnEnterAsync(bool isThisFromLoad)
    {
        // do
        // {
        //     DelayAddList.Clear();
        //     await BelongFSM.SymbolShownSorted
        //         .Where(s => !s.AlreadyChecked)
        //         .ForEachAsync(async symbol =>
        //         {
        //             await new ActImmediateDoSymbol
        //             {
        //                 Symbol = symbol,
        //                 @this = this
        //             };
        //         });
        //     await DelayAddList.SeqAwait();
        // } while (DelayAddList.Count != 0);

        // await BelongFSM.SymbolShownSorted.ForEachAsync(async symbol =>
        // {
        //     var pay = symbol.GetUltimateGive();
        //     if(pay == 0)
        //         return;
        //     await new EvtSymbolPay(symbol, pay);
        //     await new GamePlaying.ActSetCoin
        //     {
        //         Value = BelongFSM.Coin + pay,
        //         @this = BelongFSM,
        //     };
        // });
        // await BelongFSM.EnterStateAsync(new PlayingIdle(), false);
        if (!isThisFromLoad)
        {
            doList = GetDoList().ToList();
        }

        while (doList.Any())
        {
            var cur = doList[0];
            await cur;
            doList.Remove(cur);
        }
    }
    [EvtName("结算符号")]
    public record EvtSymbolPay(SymbolData WhoHasCt, long Pay) : EvtBase<SymbolData>(WhoHasCt);
    
}