using GeneralPreview;

namespace NM.Data;

public partial record GamePlaying 
{
    public record EvtClickSpin : EvtBase;

    UniEvt<EvtClickSpin> OnEvtClickSpinAsync => new()
    {
        Invoke = async (evt, ct) =>
        {
            await InState<PlayingSpin>().MatchAsync(RTask, () => EnterStateAsync(new PlayingSpin(), false));
        },
        Des = "(点击了旋转按钮) 尝试进入旋转状态"
    };
    
    public record EvtClickExit : EvtBase;
    UniEvt<EvtClickExit> OnEvtClickExitAsync => new()
    {
        Invoke = async (evt, ct) =>
        {
            await BelongFSM.EnterStateAsync(new GameTitle(), false);
        },
        Des = "(点击了退出按钮) ..直接退出游戏状态"
    };
}