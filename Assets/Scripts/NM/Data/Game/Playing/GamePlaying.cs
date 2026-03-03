using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;

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
        IUniEvt.BindAll(this, CurCt);
        InitAct.Invoke(CurCt).Forget();
    }
    
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
}