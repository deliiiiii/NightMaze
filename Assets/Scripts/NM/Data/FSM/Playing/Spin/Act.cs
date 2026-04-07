using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;

namespace NM.Data;
[ActContainer]
public partial class PlaySpin
{
    [Obsolete("执行物体词条")]
    async UniTask CheckItemAsync(GamePlaying.IItem item, CancellationToken ct)
    {
        MyDebug.Log($"执行物体 pos:{item.PivotPos}");
        await UniTask.Delay(1000, cancellationToken: ct);
    }

    public record EvtBeforeCheckSymbol(PlaySpin WhoHasCt, IItem Item) : EvtBase<PlaySpin>(WhoHasCt);
    
    [Obsolete("某物让某物属性1变化")]
    UniTask EttAddSymbolModifyProp1Async(IItem from, IItem to, int value, CancellationToken ct)
    {
        to.ModifyProp1.Add(new ModifyPropInfo
        {
            Ett = from,
            Value = value
        });
        return UniTask.CompletedTask;
    }
}