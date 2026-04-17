using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using NM.Config;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;
namespace NM.View.Len;

public class LenGrid : MonoBehaviour
{
    [SerializeField, LabelText("欲生成/移除位置(可左键点击)"), PropertyOrder(0)]Vector2Int curMos;
    [SerializeReference, LabelText("欲生成物体"), PropertyOrder(5)]ItemConfig? itemConfig;
    bool IsPlaying => GamePlayData.HasValue;
    [Button("生成物体"), EnableIf(nameof(IsPlaying)), PropertyOrder(10)]
    public void Add()
    {
        if (itemConfig == null)
            return;
        GamePlayData.MatchA(some =>
        {
            new GamePlaying.ActSpawnItemAtPos(some)
            {
                Id = itemConfig.ID,
                Pos = curMos,
                ResultWrap = null
            }.Forget();
        });
    }

    static List<ValueDropdownItem<EItemType>> GetItemTypes() => ItemConfig.GetItemTypes();
    [SerializeField, LabelText("筛选欲移除的类型"), ValueDropdown(nameof(GetItemTypes), IsUniqueList = true)
     , PropertyOrder(20)
    ] List<EItemType> itemType = [EItemType.Symbol, EItemType.Building, EItemType.Resource, EItemType.Event];
    int ToRemoveCount => ToRemoveItems.Count();
    IEnumerable<GamePlaying.MyItem> ToRemoveItems =>
        from play in GamePlayData.ToIEnumerable()
        from item in play.Items
        where itemType.Contains(item.ItemType) && item.CoverPos(curMos)
        select item;
    [Button("@\"移除物体(共\" + ToRemoveCount + \"个)\""), EnableIf(nameof(IsPlaying)), PropertyOrder(30)]
    public void Remove()
    {
        GamePlayData.MatchA(some =>
        {
            ToRemoveItems.ToList()
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
    void Update()
    {
        if(Input.GetMouseButtonDown(0))        
        {
            curMos = PlayView.ScreenToGrid(Input.mousePosition);
        }
    }
}