using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin : PlayStateBase<PlaySpin>
{
    public override string ToString()
    {
        return "Spin";
    }

    [JsonProperty(Order = 9999)]List<IUniAction> toDoList = [];
    public IEnumerable<IUniAction> ToDoList => toDoList;
    public bool CanHarvest => !toDoList.Any();
    int FindAfterId(Func<IUniAction, bool>? beforeWho = null)
    {
        beforeWho ??= RTrue1;
        int beforeId = toDoList.IndexOf(toDoList.FirstOrDefault(beforeWho));
        return beforeId;
    }
    public void InsertAfter(IUniAction act, Func<IUniAction, bool>? afterWho = null)
    {
        toDoList.Insert(FindAfterId(afterWho) + 1, act);
    }
    public void InsertAfter(IEnumerable<IUniAction> actList, Func<IUniAction, bool>? afterWho = null)
    {
        toDoList.InsertRange(FindAfterId(afterWho) + 1, actList);
    }
    
    public IEnumerable<MyItem> Items =>
        from itemInPlay in BelongNode.Items
        select itemInPlay.InSpin(this);
    protected override void OnCreateFreshData()
    {
        // BelongNode.Items.ForEach(item => item.CreateInSpin(this));
        toDoList = [..
            from itemInPlay in BelongNode.Items
            where itemInPlay.AllConfigList.Any()
            orderby itemInPlay.PivotPos.Y descending, itemInPlay.PivotPos.X, itemInPlay.Config.Order ascending 
            select new ActCheckItem(this)
            {
                Item = itemInPlay
            }
        ];
    }

    protected override async UniTask OnLaunchCom(bool isThisFromLoad)
    {
        while (toDoList.Count != 0)
        {
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
}