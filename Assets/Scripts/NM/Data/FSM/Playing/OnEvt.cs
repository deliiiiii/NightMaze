using GeneralPreview;
using NM.ViewEvt;

namespace NM.Data;

public partial class GamePlaying 
{
    UniEvt<EvtPlayViewClickExit> OnEvtClickExitAsync => new()
    {
        Invoke = (evt, ct) => GameRoot.ChangeStateAsync(new GameTitle(), false),
        Des = "(点击了退出按钮) ..直接退出游戏状态"
    };
    
    UniEvt<EvtPlayViewClickSpin> OnEvtClickSpinAsync => new()
    {
        Invoke = async (evt, ct) =>
        {
            await GetStateOptional<PlayIdle>().MatchAsync(some: async _ =>
            {
                await ChangeStateAsync(new PlaySpin(), false);
            }, none: RTask);
        },
        Des = "(点击了旋转按钮) ..尝试进入旋转状态"
    };
    
    
    UniEvt<EvtPlayViewClickHarvest> OnEvtClickHarvestAsync => new()
    {
        Invoke = async (evt, ct) =>
        {
            await GetStateOptional<PlaySpin>().MatchAsync(async some =>
            {
                if (!some.ToDoList.Any())
                {
                    await ChangeStateAsync(new PlayIdle(), false);
                }
            }, none: RTask);
        },
        Des = "(点击了收获按钮) ..尝试进入收获状态"
    };
    // UniEvt<EvtDragSymbolAtPos> OnDragSymbolOnPos => new()
    // {
    //     Invoke = (evt, ct) =>
    //     {
    //         var symbol = evt.Symbol;
    //         var tarPos = evt.Pos;
    //     }
    // }
}