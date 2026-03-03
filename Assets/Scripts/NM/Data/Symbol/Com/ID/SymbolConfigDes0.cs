using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

[FacIns(typeof(SymbolConfig0))]
public class SymbolConfigDes0 : SymbolData.ConfigDesBase<SymbolConfig0>
{
    UniEvt<PlayingSpin.EvtSpinSymbolAdjacentSymbol> OnEvtSpinSymbolAdjacentSymbolAsync => new()
    {
        Invoke = (evt, _) =>
        {
            var spinCtx = evt.Ctx;
            var playCtx = spinCtx.BelongFSM;
            if (evt.Symbol == BelongData && evt.AdjacentSymbol.ConfigID == Config.TarID)
            {
                spinCtx.DelayAddList.Add(new GamePlaying.ActSymbolAddSymbol
                {
                    Ctx = evt.Ctx.BelongFSM,
                    Arg1 = evt.Symbol,
                    Arg2 = SymbolData.Create(Config.CreateID),
                });
            }

            return UniTask.CompletedTask;
        },
        Des = "(香蕉发现和香蕉皮相邻时) 添加一个葡萄酒"
    };
}
