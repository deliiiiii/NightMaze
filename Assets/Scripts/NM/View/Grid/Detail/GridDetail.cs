using System;
using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using NM.Config;
using Sirenix.Utilities;
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

    List<GridDetailHead> headList;
    GridDetailHead? curHead;

    void Awake()
    {
        BtnClose.onClick.EvtBindTo(() =>
        {
            PlayViewIns.LockedPosDetail = null;
            gameObject.SetActiveFalse();
        }).Bind(destroyCancellationToken);

        headList = TrsGridHead.GetChildren().Select(c => c.GetComponent<GridDetailHead>()).ToList();
    }


    public void SwitchToFirst()
    {
        headList[0].OnClick?.Invoke();
    }
    public void Refresh(List<DetailInfo> detailList)
    {
        int headCount = 0;
        int tarCount = detailList.Count;
        for (int i = 0; i < tarCount; i++)
        {
            headList[i].SetActiveTrue();
        }
        for (int i = tarCount; i < TrsGridHead.childCount; i++)
        {
            headList[i].SetActiveFalse();
        }
        detailList.ForEach(detail =>
        {
            var head = headList[headCount++];
            head.TxtType.text = detail.Type;
            head.TxtName.text = detail.Name;
            head.OnClick = () =>
            {
                curHead = head;
                TrsGridType.ClearChildren();
                detail.TagInfoList.ForEach(tagInfo =>
                {
                    var item = Instantiate(pfbGridType, TrsGridType);
                    item.SetActiveTrue();
                    item.TxtType.text = tagInfo.TagName;
                    item.ImgBack.color = tagInfo.BackColor;
                    item.ImgIcon.sprite = tagInfo.Icon;
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
        curHead ??= headList[0];
        curHead.OnClick?.Invoke();
    }
}
