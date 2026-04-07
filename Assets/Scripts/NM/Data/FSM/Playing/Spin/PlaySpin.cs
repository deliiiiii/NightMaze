using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;

public partial class PlaySpin : PlayStateBase<PlaySpin>
{
    public List<IUniAction> ToDoList = [];
    IEnumerable<IItem> Items => GetComs<IItem>();
    protected override void OnCreateFreshData()
    {
        BelongNode.Items.ForEach(item => OnBelongAddEtt(item.BelongEtt));
        ToDoList = [..
            from item in BelongNode.Items
            orderby item.PivotPos.Y descending, item.PivotPos.X, item.Config.Order ascending 
            from itemInThis in GetItemByEtt(item.BelongEtt).ToIEnumerable()
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
        while (ToDoList.Count != 0)
        {
            var first = ToDoList[0];
            await first;
            ToDoList.Remove(first);
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