using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM;
using NM.Data;
using Sirenix.OdinInspector;
// ReSharper disable NotAccessedPositionalProperty.Global

[TypeRegistryItem("立即")]
public record EvtSpinImmediateDoSymbol : EvtUnit;
[TypeRegistryItem("某符号每旋转N次\t(SymbolEtt, Int)")]
public record EvtSymbolEverySpinN : EvtBase2<SymbolEtt, int>;
[TypeRegistryItem("某符号添加某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolAddSymbol : EvtBase2<SymbolEtt, SymbolEtt>;
[TypeRegistryItem("某符号消除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolDestroySymbol : EvtBase2<SymbolEtt, SymbolEtt>;
[TypeRegistryItem("某符号移除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolRemoveSymbol : EvtBase2<SymbolEtt, SymbolEtt>;
[TypeRegistryItem("某符号临时加算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutAddTemp : EvtBase2<SymbolEtt, int>;
[TypeRegistryItem("某符号临时乘算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutMulTemp : EvtBase2<SymbolEtt, int>;
[TypeRegistryItem("某符号永久加算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutAddPermanent : EvtBase2<SymbolEtt, int>;
[TypeRegistryItem("某符号积攒X时\t(SymbolEtt, int)")]
public record EvtSymbolStock : EvtBase2<SymbolEtt, int>;
[TypeRegistryItem("玩家移除某符号时\t(SymbolEtt)")]
public record EvtPlayerRemoveSymbol : EvtBase1<SymbolEtt>;
[TypeRegistryItem("某符号与某符号相邻时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolAdjacentSymbol : EvtBase2<SymbolEtt, SymbolEtt>;
[TypeRegistryItem("某符号旋转到某位置时\t(SymbolEtt, Vector2Int)")]
public record EvtSpinSymbolAt : EvtBase2<SymbolEtt, Vector2Int>;

public class ActionSymbolAddSymbol : ActionWithEvt2<GamePlaying, EvtSymbolAddSymbol, SymbolEtt, SymbolEtt>
{
    public override async UniTask DoAsync()
    {
        MyDebug.Log($"执行动作：为符号{Evt.Arg1.Config.ID}添加符号{Evt.Arg2.Config.ID}");
        await EvtBus.FireAsync(Evt);
    }
}