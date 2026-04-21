using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
    [Obsolete("某物让某物属性变化(加算)")]
    UniTask EttAddSymbolModifyPropAsync(GamePlaying.MyItem from, GamePlaying.MyItem to, EPropType propType, long value,
        ResultWrap? resultWrap, CancellationToken ct)
    {
        to[this].ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            From = from,
            AddValue = value,
        });
        resultWrap?.Success = true;
        resultWrap?.ItemWraps.Add(new ResultItemWrap(to)
        {
            CtxList = [new ResultItemWrap.CtxAddPropX { PropType = propType, Value = value }]
        });
        return UniTask.CompletedTask;
    }

    [Obsolete("某物让某物属性变化(乘算)")]
    UniTask EttMulSymbolModifyPropAsync(GamePlaying.MyItem from, GamePlaying.MyItem to, EPropType propType,
        double value, ResultWrap? resultWrap, CancellationToken ct)
    {
        to[this].ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            From = from,
            MultiValue = value
        });
        resultWrap?.Success = true;
        resultWrap?.ItemWraps.Add(new ResultItemWrap(to)
        {
            CtxList = [new ResultItemWrap.CtxMulPropX { PropType = propType, Value = value }]
        });
        return UniTask.CompletedTask;
    }
}