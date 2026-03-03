using GeneralPreview;

namespace NM.Data;

public partial class GamePlaying 
{
    public record EvtClickSpin : EvtBase;

    UniEvt<EvtClickSpin> OnEvtClickSpinAsync => new()
    {
        Invoke = async (evt, ct) =>
        {
            await InState<PlayingSpin>().MatchAsync(RTask, () => EnterStateAsync(new PlayingSpin()));
        },
        Des = "(点击了旋转按钮) 尝试进入旋转状态"
    };
}