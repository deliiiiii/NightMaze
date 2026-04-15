using System;
using System.Linq;
using General;
using GeneralPreview;
using NM.Config;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class ItemView : ViewBase
{
    [ShowInInspector]public GamePlaying.MyItem Data { get; set; }

    
    [SerializeField]Trs trsBuildingSlot;
    [SerializeField]GO pfbBuildingSlot;

    [SerializeField]BoxCollider2D boxCollider2D;
    [SerializeField]SpriteRenderer gridSr;
    [SerializeField]SpriteRenderer onGridSr;
    [SerializeField]DOTweenSequence onSpinTween;
    public void OnCreateView()
    {
        onGridSr.gameObject.SetActive(!Data.Config.IsGrid);
        onGridSr.sprite = ItemResLoader.Acquire(Data.Config.ID);
        onGridSr.sortingLayerID = SortingLayer.NameToID(Data.Config.ItemType switch
        {
            EItemType.None => "Default",
            EItemType.Symbol => Const.SortingLayer.GridSymbol,
            EItemType.Building => Const.SortingLayer.GridBuilding,
            EItemType.Resource => Const.SortingLayer.GridResource,
            EItemType.Event => Const.SortingLayer.GridEvent,
            EItemType.Grid => Const.SortingLayer.GridBack,
            _ => throw new ArgumentOutOfRangeException()
        });
        gridSr.gameObject.SetActive(Data.Config.IsGrid);
        
        boxCollider2D.offset = Data.Config.Pos switch
        {
            ItemPosRectangle rect => new Vector2((rect.Length - 1) / 2f, (rect.Height - 1) / 2f) * Const.World.GridSize,
            _ => Vector2.zero
        };
        boxCollider2D.size = Data.Config.Pos switch
        {
            ItemPosRectangle rect => new Vector2(rect.Length - 0.2f, rect.Height - 0.2f) * Const.World.GridSize,
            _ => Vector2.one * Const.World.GridSize
        };

        if (Data.Config.IsBuildingOrEvent)
        {
            trsBuildingSlot.gameObject.SetActive(true);
            trsBuildingSlot.ClearActiveChildren();
            if (Data.Config.Pos is ItemPosRectangle rect2)
            {
                foreach (var x in Enumerable.Range(0, rect2.Length))
                {
                    foreach (var y in Enumerable.Range(0, rect2.Height))
                    {
                        var ins = Instantiate(pfbBuildingSlot, trsBuildingSlot);
                        ins.transform.localPosition = new Vector2(x, y) * Const.World.GridSize;
                        ins.SetActiveTrue();
                    }
                }
            }
        }
        else
        {
            trsBuildingSlot.gameObject.SetActive(false);
        }
    }
    
    UniEvt<PlaySpin.EvtBeforeCheckSymbolTween> OnBeforeCheckSymbol => new()
    {
        Invoke = async (evt, ct) =>
        {
            if (evt.Item != Data)
                return;
            if (GameRoot.Setting.SpinTweenSpeed is SettingData.SpinTweenSpeedImmediate)
                return;
            await onSpinTween.PlayAsync(ct);
        },
        Des = "符号结算前播放动画.",
    };
}