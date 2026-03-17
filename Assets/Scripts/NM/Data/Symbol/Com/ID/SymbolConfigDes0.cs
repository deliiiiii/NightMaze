using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

[FacIns(typeof(SymbolConfig0))]
public class SymbolConfigDes0 : SymbolData.ConfigDesBase<SymbolConfig0, SymbolConfigDes0>
{
    UniEvt<PlayingSpin.EvtSpinSymbolAdjacentSymbol> OnEvtSpinSymbolAdjacentSymbolAsync => new()
    {
        Invoke = (evt, _) =>
        {
            var spinCtx = evt.WhoHasCt;
            var playCtx = spinCtx.BelongData;
            if (evt.Symbol == BelongData && evt.AdjacentSymbol.ConfigID == Config.TarID)
            {
                spinCtx.InsertBeforeCheckUnchecked(new GamePlaying.ActSymbolAddSymbol(playCtx)
                {
                    SubjectSymbol = evt.Symbol,
                    AddedSymbol = SymbolData.Create(Config.CreateID)
                });
            }
            return UniTask.CompletedTask;
        },
        Des = "(香蕉发现和香蕉皮相邻时) 添加一个葡萄酒"
    };
}
