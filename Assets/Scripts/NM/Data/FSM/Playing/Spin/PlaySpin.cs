using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
    [JsonConstructor]PlaySpin(){}
    public PlaySpin(GamePlaying belongNode)
    {
        BelongNode = belongNode;
        var items = 
            from itemInPlay in BelongNode.Items
            orderby itemInPlay.PivotPos.Y descending, itemInPlay.PivotPos.X, itemInPlay.Config.Order ascending
            select itemInPlay;
        InsertAfter((List<IUniAction>)[
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
            new ActWaitForClickNextTurn(this),
        ]);
        
    }

    [JsonIgnore] readonly CancellationTokenSource cts = new();
    [JsonIgnore] CancellationTokenSource LinkedCts =>
        field ??= CancellationTokenSource.CreateLinkedTokenSource(cts.Token, BelongNode.CurCt);
    public CancellationToken CurCt => LinkedCts.Token;
    
    [JsonProperty(Order = -1)] public GamePlaying BelongNode;
    public override string ToString() => "Spin";
    List<DistributePropInfo> noSourceDistributePropList = [];
    #region toDoList
    [JsonProperty(Order = 9999)]readonly List<IUniAction> toDoList = [];
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

    public async UniTask WaitForTodoAsync()
    {
        while (toDoList.Any())
        {
            var first = toDoList[0];
            await first;
            toDoList.Remove(first);
        }
    }
    #endregion
    
    #region Getter
    public IEnumerable<IUniAction> ToDoList => toDoList;
    public bool IsWaitClickNextTurn => toDoList.FirstOrDefault() is ActWaitForClickNextTurn;
    public long GetDeltaPropValue(EPropType propType)
    {
        List<DistributePropInfo> list = [..
            from itemInPlay in BelongNode.Items
            from dProp in itemInPlay[this].DistributePropList
            select dProp,
            ..noSourceDistributePropList];
        return list.Where(d => d.PropType == propType).Sum(d => d.Value);
    }
    #endregion
}