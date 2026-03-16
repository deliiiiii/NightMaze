using GeneralPreview;

namespace NM.Data;

public partial record GamePlaying 
{
    public record EvtClickSpin : EvtForgetBase;

    UniEvt<EvtClickSpin> OnEvtClickSpinAsync => new()
    {
        Invoke = (evt, ct) => InState<PlayingSpin>().Reverse() | EnterStateAsync(new PlayingSpin(), false),
        Des = "(点击了旋转按钮) 尝试进入旋转状态"
    };
    
    public record EvtClickExit : EvtForgetBase;
    UniEvt<EvtClickExit> OnEvtClickExitAsync => new()
    {
        Invoke = (evt, ct) => BelongFSM.EnterStateAsync(new GameTitle(), false),
        Des = "(点击了退出按钮) ..直接退出游戏状态"
    };
}