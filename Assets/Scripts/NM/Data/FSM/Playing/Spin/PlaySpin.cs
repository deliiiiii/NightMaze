using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin : PlayStateBase<PlaySpin>
{
    public override string ToString() => "Spin";
    #region toDoList
    [JsonProperty(Order = 9999)]readonly List<IUniAction> toDoList = [];
    readonly List<DistributePropInfo> noSourceDistributePropList = [];
    int FindAfterId(Func<IUniAction, bool>? beforeWho = null)
    {
        beforeWho ??= RTrue1;
        int beforeId = toDoList.IndexOf(toDoList.FirstOrDefault(beforeWho));
        return beforeId;
    }
    void InsertAfter(IUniAction act, Func<IUniAction, bool>? afterWho = null) => 
        toDoList.Insert(FindAfterId(afterWho) + 1, act);
    void InsertAfter(IEnumerable<IUniAction> actList, Func<IUniAction, bool>? afterWho = null) => 
        toDoList.InsertRange(FindAfterId(afterWho) + 1, actList);
    #endregion
    
    #region Getter
    public IEnumerable<IUniAction> ToDoList => toDoList;
    public bool CanHarvest => !toDoList.Any();
    IEnumerable<MyItem> Items =>
        from itemInPlay in BelongNode.Items
        select itemInPlay[this];
    // TODO 临时拿GamePlaying.AddHostilityPerTurn
    public long GetDeltaPropValue(EPropType propType)
    {
        List<DistributePropInfo> list = [
            ..Items.SelectMany(item => item.DistributePropList),
            ..noSourceDistributePropList];
        return list.Where(d => d.PropType == propType).Sum(d => d.Value);
    }
    #endregion
    
    #region Node
    protected override void OnCreateFreshData()
    {
        var items = 
            from itemInPlay in BelongNode.Items
            orderby itemInPlay.PivotPos.Y descending, itemInPlay.PivotPos.X, itemInPlay.Config.Order ascending
            select itemInPlay;
        toDoList.Clear();
        toDoList.AddRange([
            new ActDoNoSourceProp(this)
            {
                PropType = EPropType.PropA2,
                Value = GamePlaying.AddHostilityPerTurn
            }, ..
            // ReSharper disable once PossibleMultipleEnumeration
            from itemInPlay in items
            select new ActCheckItem(this)
            {
                Item = itemInPlay
            }, ..
            // ReSharper disable once PossibleMultipleEnumeration
            from itemInPlay in items
            select new ActDistributePropForItem(this)
            {
                Item = itemInPlay
            },
        ]);
    }
    protected override UniTask OnLaunchCom(bool isThisFromLoad)
    {
        StartTodo().Forget();
        return UniTask.CompletedTask;
    }

    async UniTask StartTodo()
    {
        while (true)
        {
            if (!toDoList.Any())
            {
                await UniTask.Yield(CurCt);
                continue;
            }
            var first = toDoList[0];
            await first;
            toDoList.Remove(first);
        }
    }

    protected override void OnReleaseCom()
    {
        BelongNode.Items.ForEach(item => item.DestroyInSpin());
        base.OnReleaseCom();
    }
    #endregion
}