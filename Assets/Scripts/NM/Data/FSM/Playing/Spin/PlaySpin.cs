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
        BelongNode.Items.ForEach(AddEttComToItem);
        ToDoList = [..
            from item in BelongNode.Items
            orderby item.PivotPos.Y descending, item.PivotPos.X, item.Config.Order ascending 
            select new ActCheckItem(this)
            {
                Item = item
            }
        ];
    }

    void AddEttComToItem(GamePlaying.IItem item)
    {
        switch (item)
        {
            case GamePlaying.Grid grid:
                AddEttCom<EttGrid, Grid>(new Grid(grid.BelongEtt));
                break;
            case GamePlaying.Symbol symbol:
                AddEttCom<EttSymbol, Symbol>(new Symbol(symbol.BelongEtt));
                break;
            case GamePlaying.Building building:
                AddEttCom<EttBuilding, Building>(new Building(building.BelongEtt));
                break;
            case GamePlaying.Resource resource:
                AddEttCom<EttResource, Resource>(new Resource(resource.BelongEtt));
                break;
            default:
                throw new System.Exception($"没有这个物体类型 {item.GetType()}.");
        }
    }

    public MyOption<IItem> GetItemByPlay(GamePlaying.IItem item)
    {
        return item switch
        {
            GamePlaying.Grid grid => GetEttComOptional<EttGrid, Grid>(grid.BelongEtt).Map<IItem>(x => x),
            GamePlaying.Symbol symbol => GetEttComOptional<EttSymbol, Symbol>(symbol.BelongEtt).Map<IItem>(x => x),
            GamePlaying.Building building => GetEttComOptional<EttBuilding, Building>(building.BelongEtt).Map<IItem>(x => x),
            GamePlaying.Resource resource => GetEttComOptional<EttResource, Resource>(resource.BelongEtt).Map<IItem>(x => x),
            _ => throw new System.Exception($"没有这个物体类型 {item.GetType()}.")
        };
    }
    protected override async UniTask OnLaunchCom(bool isThisFromLoad)
    {
        while (ToDoList.Count != 0)
        {
            var first = ToDoList[0];
            await first;
            ToDoList.Remove(first);
        }
    }
}