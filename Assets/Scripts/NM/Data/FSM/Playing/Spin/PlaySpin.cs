using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin : PlayStateBase<PlaySpin>
{
    List<IUniAction> toDoList = [];
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
    
    
    IEnumerable<IItem> Items => GetComs<IItem>();
    protected override void OnCreateFreshData()
    {
        BelongNode.Items.ForEach(item => OnBelongAddEtt(item.BelongEtt));
        toDoList = [..
            from itemInPlay in BelongNode.Items
            orderby itemInPlay.PivotPos.Y descending, itemInPlay.PivotPos.X, itemInPlay.Config.Order ascending 
            from itemInThis in GetItemByEtt(itemInPlay.BelongEtt).ToIEnumerable()
            where itemInPlay is GamePlaying.Symbol
            select new ActCheckItem(this)
            {
                Item = itemInThis
            }
        ];
    }

    public MyOption<IItem> GetItemByEtt(EttBase ett)
    {
        return ett switch
        {
            EttGrid ettGrid => GetEttComOptional<EttGrid, Grid>(ettGrid).Map<IItem>(x => x),
            EttSymbol ettSymbol => GetEttComOptional<EttSymbol, Symbol>(ettSymbol).Map<IItem>(x => x),
            EttBuilding ettBuilding => GetEttComOptional<EttBuilding, Building>(ettBuilding).Map<IItem>(x => x),
            EttResource ettResource => GetEttComOptional<EttResource, Resource>(ettResource).Map<IItem>(x => x),
            _ => throw new System.Exception($"没有匹配穷尽EttBase{nameof(EttBase)}类型: {ett.GetType()}.")
        };
    }
    protected override async UniTask OnLaunchCom(bool isThisFromLoad)
    {
        BelongNode.OnAddEtt += OnBelongAddEtt;
        BelongNode.OnRemoveEtt += OnBelongRemoveEtt;
        while (toDoList.Count != 0)
        {
            var first = toDoList[0];
            await first;
            toDoList.Remove(first);
        }
    }

    protected override void OnReleaseCom()
    {
        BelongNode.OnAddEtt -= OnBelongAddEtt;
        BelongNode.OnRemoveEtt -= OnBelongRemoveEtt;
        base.OnReleaseCom();
    }

    void OnBelongAddEtt(EttBase ett)
    {
        switch (ett)
        {
            case EttGrid ettGrid:
                AddEttCom<EttGrid, Grid>(new Grid(ettGrid));
                break;
            case EttSymbol ettSymbol:
                AddEttCom<EttSymbol, Symbol>(new Symbol(ettSymbol));
                break;
            case EttBuilding ettBuilding:
                AddEttCom<EttBuilding, Building>(new Building(ettBuilding));
                break;
            case EttResource ettResource:
                AddEttCom<EttResource, Resource>(new Resource(ettResource));
                break;
            default:
                throw new System.Exception($"没有匹配穷尽{nameof(EttBase)}类型: {ett.GetType()}.");
        }
    }
    void OnBelongRemoveEtt(EttBase ett)
    {
        switch (ett)
        {
            case EttGrid ettGrid:
                RemoveEttCom(ettGrid);
                break;
            case EttSymbol ettSymbol:
                RemoveEttCom(ettSymbol);
                break;
            case EttBuilding ettBuilding:
                RemoveEttCom(ettBuilding);
                break;
            case EttResource ettResource:
                RemoveEttCom(ettResource);
                break;
        }
    }
}