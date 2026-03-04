using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;

namespace NM.Data;

[Serializable]
public partial class PlayingSpin : GamePlaying.StateFSM<PlayingSpin>
{
    public List<UniAction> DelayAddList = [];
    public List<UniAction> DelayDestroyList = [];

    public override async UniTask OnEnterAsync()
    {
        BelongFSM.SymbolDeckList.ForEach(s =>
        {
            s.AlreadyChecked = false;
            s.TempAdd.Clear();
            s.TempMulti.Clear();
        });
        
        await Bus.FireAsync(new EvtOnEnter(this), CurCt);
        BelongFSM.SymbolShownList.Clear();
        foreach (var toShow in BelongFSM.SymbolDeckList.ShuffleTo())
        {
            var shownCount = BelongFSM.SymbolShownList.Count;
            if(shownCount == Const.SpinW * Const.SpinH)
                break;
            var addX = shownCount / Const.SpinH + 1;
            var addY = shownCount % Const.SpinH + 1;
            await new GamePlaying.ActShowSymbolAt
            {
                Symbol = toShow,
                Pos = new Vector2Int(addX, addY),
                Ctx = BelongFSM
            };
        }

        do
        {
            var shownList = BelongFSM.SymbolShownList;
            shownList.Sort(SymbolData.ByPos);
            DelayAddList.Clear();
            foreach (var symbol in shownList.Where(s => !s.AlreadyChecked))
            {
                symbol.AlreadyChecked = true;
                await Bus.FireAsync(new EvtImmediateDoSymbol(symbol), CurCt);
                foreach (var adjacentSymbol in BelongFSM.GetAdjacent(symbol))
                {
                    var debug = !adjacentSymbol.IsEmpty && !symbol.IsEmpty;
                    await Bus.FireAsync(new EvtSpinSymbolAdjacentSymbol(this, adjacentSymbol, symbol), CurCt, () => debug);
                }
            }
            foreach (var doDelay in DelayAddList)
            {
                await doDelay;
            }
        } while (DelayAddList.Count != 0);

        
        foreach (var symbol in BelongFSM.SymbolShownList)
        {
            var pay = symbol.GetUltimateGive();
            await Bus.FireAsync(new EvtPay(symbol, pay), CurCt);
            BelongFSM.Coin += pay;
        }
        MyDebug.Log("Spin End");
        await BelongFSM.EnterStateAsync(new PlayingIdle());
    }
    
    
    public record EvtOnEnter(PlayingSpin Ctx) : EvtBase;

    /// <summary>
    /// 立即
    /// </summary>
    /// <param name="Symbol">被执行的符号</param>
    public record EvtImmediateDoSymbol(SymbolData Symbol) : EvtBase;
    /// <summary>
    /// 某符号结算时
    /// </summary>
    /// <param name="Symbol"></param>
    public record EvtPay(SymbolData Symbol, long Pay) : EvtBase;
    
    /// <summary>
    /// 发现某符号与(当前)某符号相邻时
    /// </summary>
    /// <param name="Ctx">上下文</param>
    /// <param name="AdjacentSymbol">被发现的符号</param>
    /// <param name="Symbol">当前符号</param>
    public record EvtSpinSymbolAdjacentSymbol(PlayingSpin Ctx, SymbolData AdjacentSymbol, SymbolData Symbol) : EvtBase;
}