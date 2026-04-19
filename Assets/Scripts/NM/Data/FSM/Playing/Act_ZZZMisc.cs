using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NM.Config;

namespace NM.Data;
[ActContainer]
public partial class GamePlaying
{
    [Obsolete("某物添加某物的词条")]
    UniTask ItemEatItemConfigAsync(MyItem whoEat, MyItem toEat, ResultWrap? resultWrap, CancellationToken ct)
    {
        whoEat.EatConfigList.AddRange(toEat.Config.DesList);
        resultWrap?.Success = true;
        return UniTask.CompletedTask;
    }
    [Obsolete("领取事件奖励")]
    UniTask ObtainEvtAsync(MyItem item, CancellationToken ct)
    {
        if (!item.Config.IsEvent || !item.IsBuildingOrEventKanSei)
            return UniTask.CompletedTask;
        item.Config.EvtDesResultList.ForEach(des =>
        {
            switch (des)
            {
                case ItemDesResultClearHostility:
                    new ActClearProp(this) { PropType = EPropType.PropA2 }.Forget();
                    break;
                case ItemDesResultUnlockNextLayer:
                    new ActUnlockNextLayer(this).Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(des));
            }
        });
        new ActRemoveItem(this)
        {
            ToRemove = item,
            ResultWrap = null
        }.Forget();
        return UniTask.CompletedTask;
    }
}