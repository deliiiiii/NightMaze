using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

public class SymbolCom0 : SymbolData.ConfigCom<SymbolConfig0>
{
    public SymbolCom0()
    {
        OnEvtSpinSymbolAdjacentSymbolAsync.AddTo(CancellationToken.None);
    }
    UniEvt<PlayingSpin.EvtSpinSymbolAdjacentSymbol> OnEvtSpinSymbolAdjacentSymbolAsync => new()
    {
        Invoke = (evt, _) =>
        {
            var spinCtx = evt.Ctx;
            var playCtx = spinCtx.BelongFSM;
            if (evt.Symbol == BelongData && evt.AdjacentSymbol.ConfigID == Config.TarConfig.ID)
            {
                spinCtx.DelayAddList.Add(playCtx.SymbolAddSymbolAct.Apply(evt.Symbol, SymbolData.CreateSymbol(9)));
            }

            return UniTask.CompletedTask;
        },
        Des = "(香蕉发现和香蕉皮相邻时) 添加一个葡萄酒"
    };
}
