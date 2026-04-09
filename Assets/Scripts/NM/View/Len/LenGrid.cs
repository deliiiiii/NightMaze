using System;
using System.Linq;
using NM.Config;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;
namespace NM.View.Len;

public class LenGrid : MonoBehaviour
{
    [Button]
    public void Add(IItemConfig? itemConfig)
    {
        if (itemConfig == null)
            return;
        GamePlayData.MatchA(some =>
        {
            new GamePlaying.ActSpawnItemAtPos(some)
            {
                Type = itemConfig switch
                {
                    GridConfig => EItemType.Grid,
                    BuildingConfig => EItemType.Building,
                    ResourceConfig => EItemType.Resource,
                    SymbolConfig => EItemType.Symbol,
                    _ => throw new Exception($"不支持的物体类型: {itemConfig.GetType()}")
                },
                Id = itemConfig.ID,
                Pos = CurMos,
                ResultWrap = null
            }.Forget();
        });
    }

    [Button]
    public void Remove(EItemType itemType = EItemType.Symbol | EItemType.Building | EItemType.Resource)
    {
        GamePlayData.MatchA(some =>
        {
            some.Items
                .Where(i => itemType != 0 && itemType.HasFlag(i.ItemType) && i.PivotPos == CurMos)
                .ToList()
                .ForEach(toRemove =>
                {
                    new GamePlaying.ActRemoveItem(some)
                    {
                        ToRemove = toRemove,
                        ResultWrap = null
                    }.Forget();
                });
        });
    }
    // List<GamePlaying.IItem> selectedItemList = [];
    public Vector2Int CurMos;
    void Update()
    {
        if(Input.GetMouseButtonDown(0))        
        {
            CurMos = PlayView.ScreenToGrid(Input.mousePosition);
        }
    }
}