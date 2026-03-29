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
    
    // UniEvt<EvtDragSymbolAtPos> OnDragSymbolOnPos => new()
    // {
    //     Invoke = (evt, ct) =>
    //     {
    //         var symbol = evt.Symbol;
    //         var tarPos = evt.Pos;
    //     }
    // }
}