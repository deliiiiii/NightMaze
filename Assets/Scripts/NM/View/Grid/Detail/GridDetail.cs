using System;
using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using NM.Config;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class GridDetail : MonoBehaviour
{
    [SerializeField] GridDetailHead pfbGridDetailHead;
    [SerializeField] GridType pfbGridType;
    [SerializeField] GridInSpinLine pfbGridInSpinLine;

    public Btn BtnClose;
    public Txt TxtDetail;

    public Trs TrsGridHead;
    public Trs TrsGridType;
    public Trs TrsGridInSpinLine;
    
    ItemTypeResourceMgr mgr => field ??= RefPoolSingle<ItemTypeResourceMgr>.Acquire();

    void Awake()
    {
        BtnClose.onClick.EvtBindTo(() =>
        {
            PlayViewIns.SwitchLockGridDetail(false);
            gameObject.SetActiveFalse();
        }).Bind(destroyCancellationToken);
    }

    public void Refresh(List<DetailInfo> detailList)
    {
        TrsGridHead.ClearChildren();
        GridDetailHead? firstHead = null;
        detailList.ForEach(detail =>
        {
            var head = Instantiate(pfbGridDetailHead, TrsGridHead);
            head.SetActiveTrue();
            head.TxtType.text = detail.Type;
            head.TxtName.text = detail.Name;
            firstHead ??= head;
            head.OnClick = () =>
            {
                TrsGridType.ClearChildren();
                detail.ItemTypeList.ForEach(itemType =>
                {
                    var item = Instantiate(pfbGridType, TrsGridType);
                    item.SetActiveTrue();
                    item.TxtType.text = itemType.GetLabelText();
                    item.ImgBack.color = mgr.Dic[itemType].backColor;
                    item.ImgIcon.sprite = mgr.Dic[itemType].icon;
                });
                TxtDetail.text = detail.Detail;
                TrsGridInSpinLine.ClearChildren();
                detail.InSpinLineList.ForEach(inSpinLine =>
                {
                    var line = Instantiate(pfbGridInSpinLine, TrsGridInSpinLine);
                    line.SetActiveTrue();
                    line.TxtLine.text = inSpinLine;
                });
            };
        });
        firstHead?.OnClick?.Invoke();
    }
}

public class DetailInfo
{
    public required string Type;
    public required string Name;
    public required List<EItemType> ItemTypeList;
    public required string Detail;
    public required List<string> InSpinLineList;
}