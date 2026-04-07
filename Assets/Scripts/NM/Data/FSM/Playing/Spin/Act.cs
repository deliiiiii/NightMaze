using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;
[ActContainer]
public partial class PlaySpin
{
    [Obsolete("执行符号效果")]
    async UniTask CheckSymbolAsync(Symbol symbol, CancellationToken ct)
    {
        await new EvtBeforeCheckSymbol(this, symbol);
        symbol.SelfAddBaseValue(this);
    }

    public record EvtBeforeCheckSymbol(PlaySpin WhoHasCt, Symbol Symbol) : EvtBase<PlaySpin>(WhoHasCt);
    
    [Obsolete("某物让某符号属性1 变化（添加时）")]
    UniTask EttAddSymbolModifyProp1Async(EttBase ett, Symbol symbol, int value, CancellationToken ct)
    {
        symbol.ModifyProp1.Add(new ModifyPropInfo
        {
            Ett = ett,
            Value = value
        });
        return UniTask.CompletedTask;
    }
    [Obsolete("某物让某符号属性1的 变化（移除时）")]
    UniTask EttRemoveSymbolModifyProp1Async(EttBase ett, Symbol symbol, CancellationToken ct)
    {
        var item = symbol.ModifyProp1.Find(m => m.Ett == ett);
        if (item != null)
        {
            symbol.ModifyProp1.Remove(item);
        }
        return UniTask.CompletedTask;
    }
}