using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin : PlayStateBase<PlaySpin>
{
    public List<IUniAction> ToDoList = [];
    IEnumerable<Symbol> Symbols => GetComs<Symbol>();
    protected override void OnCreateFreshData()
    {
        BelongNode.Symbols.ForEach(s => AddEttCom<EttSymbol, Symbol>(new Symbol(this, s.BelongEtt)));
        ToDoList = [..
            from s in Symbols
            from symbolInPlay in BelongNode[s.BelongEtt].ToIEnumerable()
            orderby symbolInPlay.PivotPos.Y descending, symbolInPlay.PivotPos.X
            select new ActCheckSymbol(this)
            {
                Symbol = s
            }
        ];
    }

    protected override async UniTask OnLaunchCom(bool isThisFromLoad)
    {
        while (ToDoList.Count != 0)
        {
            var first = ToDoList[0];
            MyDebug.Log(1);
            await UniTask.Delay(1000);
            MyDebug.Log(2);
            await first;
            ToDoList.Remove(first);
        }

        // await BelongNode.ChangeStateAsync(new PlayIdle(), false);
    }
}