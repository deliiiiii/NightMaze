using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;

[Serializable]
public partial class GamePlaying : FSM<GamePlaying>
{
    public override string ToString() => nameof(GamePlaying);

    public List<SymbolData> SymbolDeckList = [];
    public long Coin;
    public int RemoveToken;
    public int RefreshToken;
    public int NextRentCount;
    public int SpinCount;
    public int DeckMax = 20;

    public List<SymbolData> SymbolShownList = [];
    
    public GamePlaying()
    {
        OnEvtClickSpinAsync.AddTo(CurCt);
        InitAct.Invoke(CurCt).Forget();
    }
    
    public IEnumerable<SymbolData> GetAdjacent(SymbolData SymbolData) 
        => SymbolData.Pos.Match(
            pos =>
                from x in Enumerable.Range(pos.X - 1, 3)
                from y in Enumerable.Range(pos.Y - 1, 3)
                where x is >= Const.SpinFirstID and <= Const.SpinW
                where y is >= Const.SpinFirstID and <= Const.SpinH
                where !(x == pos.X && y == pos.Y)
                select SymbolShownList.FirstOrDefault(xs => xs.Pos.Match(some => some.X == x && some.Y == y, RFalse)),
            () => []);
}

[Serializable]
public class PlayingIdle : GamePlaying.StateFSM<PlayingIdle>;
[Serializable]
public class PlayingSpin : GamePlaying.StateFSM<PlayingSpin>
{
    public List<UniAction> DelayAddList = [];
    public List<UniAction> DelayDestroyList = [];

    public override void RegisterAll()
    {
        OnEvtSpinSymbolAdjacentSymbolAsync.AddTo(CurCt);
    }

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
            await BelongFSM.ShowSymbolAtAsync.Invoke(toShow, new Vector2Int(addX, addY), CurCt);
        }

        do
        {
            BelongFSM.SymbolShownList.Sort(SymbolData.ByPos);
            DelayAddList.Clear();
            foreach (var symbol in BelongFSM.SymbolShownList.Where(s => !s.AlreadyChecked))
            {
                symbol.AlreadyChecked = true;
                await Bus.FireAsync(new EvtImmediateDoSymbol(symbol), CurCt);
                foreach (var adjacentSymbol in BelongFSM.GetAdjacent(symbol))
                {
                    var debug = !adjacentSymbol.IsEmpty && !symbol.IsEmpty;
                    await Bus.FireAsync(new EvtSpinSymbolAdjacentSymbol(adjacentSymbol, symbol), CurCt, () => debug);
                }
            }
            foreach (var doDelay in DelayAddList)
            {
                await doDelay.Invoke(CurCt);
            }
        } while (DelayAddList.Count != 0);

        
        foreach (var symbol in BelongFSM.SymbolShownList)
        {
            await Bus.FireAsync(new EvtPay(symbol), CurCt);
        }
        
        MyDebug.Log("Spin End");
        await BelongFSM.EnterStateAsync<PlayingIdle>();
    }
    
    
    public record EvtOnEnter(PlayingSpin Ctx) : EvtBase;

    [TypeRegistryItem("立即")]
    public record EvtImmediateDoSymbol(SymbolData Symbol) : EvtBase;
    [TypeRegistryItem("发现某符号与(当前)某符号相邻时\t(SymbolData, SymbolData)")]
    public record EvtSpinSymbolAdjacentSymbol(SymbolData AdjacentSymbol, SymbolData Symbol) : EvtBase;
    [TypeRegistryItem("某符号结算时\t(SymbolData)")]
    public record EvtPay(SymbolData Symbol) : EvtBase;
    
    UniEvt<EvtSpinSymbolAdjacentSymbol> OnEvtSpinSymbolAdjacentSymbolAsync => new()
    {
        Invoke = (evt, _) =>
        {
            foreach (var s in BelongFSM.SymbolDeckList)
            {
                if (evt.Symbol.ConfigID == 1 && evt.Symbol == s && evt.AdjacentSymbol.ConfigID == 2)
                {
                    DelayAddList.Add(BelongFSM.SymbolAddSymbolAct.Apply(evt.Symbol, SymbolData.CreateSymbol(9)));
                }
            }

            return UniTask.CompletedTask;
        },
        Des = "(香蕉发现和香蕉皮相邻时) 添加一个葡萄酒"
    };
}