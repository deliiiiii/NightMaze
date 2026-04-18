using GeneralPreview;

namespace NM.Data;

public partial class GamePlaying 
{
    public record EvtClickSpin : EvtForgetBase;
    public record EvtClickNextTurn : EvtForgetBase;
    public record EvtClickExit : EvtForgetBase;
    UniEvt<EvtClickExit> OnEvtClickExitAsync => new()
    {
        Invoke = (evt, ct) => GameRoot.ChangeStateAsync(new GameTitle(), false),
        Des = "(点击了退出按钮) ..直接退出游戏状态"
    };
    UniEvt<EvtClickSpin> OnEvtClickSpinAsync => new()
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
}