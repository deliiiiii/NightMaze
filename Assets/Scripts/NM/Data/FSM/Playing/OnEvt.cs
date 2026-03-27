using GeneralPreview;

namespace NM.Data;

public partial class GamePlaying 
{
    public record EvtClickExit : EvtForgetBase;
    UniEvt<EvtClickExit> OnEvtClickExitAsync => new()
    {
        Invoke = (evt, ct) => GameRoot.ChangeStateAsync(new GameTitle(), false),
        Des = "(点击了退出按钮) ..直接退出游戏状态"
    };
}