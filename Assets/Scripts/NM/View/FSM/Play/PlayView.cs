using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
using NM.Data;
using NM.ViewEvt;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace NM.View;
public class PlayView : ViewBase<GamePlaying>
{
    public GridDetail GridDetail;
    
    public Trs GridTrs;
    public Btn BtnSpin;
    public Btn BtnHarvest;
    public Btn BtnExit;

    [SerializeField] GridView gridPfb;
    [SerializeField] SymbolView symbolPfb;
    [SerializeField] ResourceView resourcePfb;
    [SerializeField] BuildingView buildingPfb;

    readonly List<ItemViewBase> itemList = [];
    IEnumerable<GridView> Grids => itemList.OfType<GridView>();
    IEnumerable<SymbolView> Symbols => itemList.OfType<SymbolView>();
    IEnumerable<BuildingView> Buildings => itemList.OfType<BuildingView>();
    IEnumerable<ResourceView> Resources => itemList.OfType<ResourceView>();
    
    public Vector2Int? LockedPosDetail;
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnSpin.onClick.EvtBindTo(() => new EvtPlayViewClickSpin().Forget());
        yield return BtnHarvest.onClick.EvtBindTo(() => new EvtPlayViewClickHarvest().Forget());
        yield return BtnExit.onClick.EvtBindTo(() => new EvtPlayViewClickExit().Forget());
    }

    void Update()
    {
        if (LockedPosDetail != null)
        {
            ShowGridDetailAtPos(LockedPosDetail.Value);
        }

        BtnSpin.interactable = (
            from play in GamePlayData
            select play.IsState<PlayIdle>()) | false;
        BtnHarvest.interactable = (
            from spin in PlaySpinData
            select spin.CanHarvest) | false;
    }

    #region OnEvt
    UniEvt<GamePlaying.EvtOnEnter> OnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = evt.WhoHasCt;
            // ClearAllGrid();
            Data.Items.ForEach(SetItemAtPos);
            // Data.Grids.ForEach(SetGridAtPos);
            // Data.Symbols.ForEach(SetSymbolAtPos);
            // Data.Resources.ForEach(SetResourceAtPos);
            gameObject.SetActiveTrue();
            return UniTask.CompletedTask;
        },
        Des = "(进入Root - Playing状态时) 恢复游戏"
    };

    UniEvt<GamePlaying.EvtOnExit> OnExit => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = null!;
            ClearAllGrid();
            this.SetActiveFalse();
            GridDetail.SetActiveFalse();
            LockedPosDetail = null;
            return UniTask.CompletedTask;
        },
        Des = "(退出Root - Playing状态时) 隐藏界面"
    };

    UniEvt<GamePlaying.EvtSetGridAtPos> OnSetGridAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetItemAtPos(evt.Grid);
            return UniTask.CompletedTask;
        },
        Des = "显示地块",
    };
    UniEvt<GamePlaying.EvtSetSymbolAtPos> OnSetSymbolAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetItemAtPos(evt.Symbol);
            return UniTask.CompletedTask;
        },
        Des = "显示符号",
    };
    #endregion
    
    public void ShowGridDetailAtPos(Vector2Int gridPos)
    {
        List<DetailInfo> detailList =
        [
            ..
            from item in Data.Items
            where item.CoverPos(gridPos)
            // from config in (List<IItemConfig>)[item.Config, .. item.EatConfigs]
            select new DetailInfo
            {
                Type = item.Config.PrefixName,
                Name = item.Config.Name,
                TagInfoList = item.Config.DetailTagInfos,
                // TODO 不仅仅是风味文本.还有描述文本.
                Detail = $"""
                          {item.PivotPos}{ResolveItemDesList(item.Config.DesList)}{ResolveItemDesList(item.EatConfigList)}
                          <color=grey>{item.Config.FlavorDes}</color>
                          """,
                InSpinLineList =
                [
                    ..
                    from spin in PlaySpinData.ToIEnumerable()
                    from itemInSpin in spin.GetItemByEtt(item.BelongEtt).ToIEnumerable()
                    from modProp in itemInSpin.ModifyPropList
                    orderby modProp.PropType, modProp.AddValue, modProp.MultiValue
                    select $"{item.Config.Name} " +
                           (modProp.AddValue != 0 ? modProp.AddValue.ToStringWithSymbol() : string.Empty) +
                           (modProp.MultiValue != 0 ? $"<color=green>x{modProp.MultiValue}</color>" : string.Empty)
                ]
            }
        ];
        GridDetail.SetActiveTrue();
        GridDetail.transform.position = GridToWorld(gridPos + new Vector2Int(1,1) * Const.GridSize);
        GridDetail.transform.SetLocalPositionZ(0);
        GridDetail.Refresh(detailList);
    }

    string ResolveItemDesList(List<ItemDesConfig> desConfigList)
    {
        var ret = string.Join("\n", desConfigList.Select(ResolveItemDes));
        if (ret != string.Empty)
            return $"\n{ret}";
        return string.Empty;
    }
    string ResolveItemDes(ItemDesConfig desConfig)
    {
        var sb = new StringBuilder();
        var result = desConfig.Result;
        bool isFirst = true;
        while (result != null)
        {
            if(!isFirst)
                sb.Append("<color=red> & </color>");
            isFirst = false;
            sb.Append(result.GetType().GetCustomAttribute<TypeRegistryItemAttribute>()?.Name ?? result.GetType().Name);
            result = result.Next;
        }
        return sb.ToString();
    }
    

    public void HideGridDetail()
    {
        if (LockedPosDetail != null)
            return;
        GridDetail.SetActiveFalse();
    }

    void ClearAllGrid()
    {
        itemList.ForEach(item => Destroy(item.gameObject));
        itemList.Clear();
    }

    void SetItemAtPos(GamePlaying.IItem item)
    {
        ItemViewBase pfb = item switch
        {
            GamePlaying.Grid => gridPfb,
            GamePlaying.Symbol => symbolPfb,
            GamePlaying.Resource => resourcePfb,
            GamePlaying.Building => buildingPfb,
            _ => throw new Exception($"未适配的Item类型 {item.GetType()}")
        };
        
        ItemViewBase? ins = itemList.FirstOrDefault(s => s.Data == item);
        if (ins == null)
        {
            ins = Instantiate(pfb);
            ins.Data = item;
            ins.SetActiveTrue();
            ins.OnCreateView();
        }

        if (ins is GridView)
        {
            ins.transform.parent = GridTrs;
            ins.transform.position = GridToWorld(item.PivotPos);
        }
        else
        {
            ins.transform.parent = Grids.FirstOrDefault(g => g.Data.PivotPos == item.PivotPos)?.TrsSymbol;
            ins.transform.localPosition = Vector3.zero;
        }
        itemList.Add(ins);
    }
    public static Vector2Int ScreenToGrid(Vector2 screenPos) => WorldToGrid(MyCamera.Main.ScreenToWorldPoint(screenPos));
    static Vector2 GridToScreen(Vector2Int gridPos) => MyCamera.Main.WorldToScreenPoint(GridToWorld(gridPos));
    static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
}

internal static class IntExt
{
    extension(long self)
    {
        public string ToStringWithSymbol()
        {
            // if(ignoreZero && self == 0)
                // return string.Empty;
            string symbol = self switch
            {
                > 0 => "+",
                < 0 => "-",
                _ => string.Empty
            };
            return $"{symbol}{self}";
        }
    }
}