using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;

[Serializable]
public partial class PlayingSpin : CompositeBase<GamePlaying, PlayingSpin>
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
        BelongData.SymbolDeck.ForEach(s =>
        {
            s.AlreadyChecked = false;
            s.TempAdd.Clear();
            s.TempMulti.Clear();
            s.Pos.MatchA(some => s.Pos = None);
        });
        var showSymbolActs = BelongData.SymbolRandomly
            .Take(Const.SpinW * Const.SpinH)
            .Select((toShow, index) => new GamePlaying.ActShowSymbolAt(BelongData)
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

    public override async UniTask OnAddAsync(bool isThisFromLoad)
    {
        await base.OnAddAsync(isThisFromLoad);
        if (!isThisFromLoad)
        {
            doList = GetDoList().ToList();
        }

        while (doList.Any())
        {
            var cur = doList[0];
            await cur;
            doList.Remove(cur);
            await UniTask.Yield();
        }
    }
    [EvtName("结算符号")]
    public record EvtSymbolPay(SymbolData WhoHasCt, long Pay) : EvtBase<SymbolData>(WhoHasCt);
    
}