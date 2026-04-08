using System;
using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using NM.Config;
using UnityEngine;
// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

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

    const int GridHeadMax = 20;
    const int TagMax = 20;
    const int SpinLineMax = 50;

    List<GridDetailHead> headList;
    List<GridType> tagList;
    List<GridInSpinLine> inSpinLineList;
    GridDetailHead? curHead;

    void Awake()
    {
        BtnClose.onClick.EvtBindTo(() =>
        {
            PlayViewIns.LockedPosDetail = null;
            gameObject.SetActiveFalse();
        }).Bind(destroyCancellationToken);

        headList = TrsGridHead.GetChildren().Select(c => c.GetComponent<GridDetailHead>()).ToList();
        tagList = TrsGridType.GetChildren().Select(c => c.GetComponent<GridType>()).ToList();
        inSpinLineList = TrsGridInSpinLine.GetChildren().Select(c => c.GetComponent<GridInSpinLine>()).ToList();
    }


    public void SwitchToFirst()
    {
        headList[0].OnClick?.Invoke();
    }
    public void Refresh(List<DetailInfo> detailList)
    {
        int headCount = 0;
        int gridHeadCount = detailList.Count;
        for (int i = 0; i < Math.Min(GridHeadMax, gridHeadCount); i++)
        {
            headList[i].SetActiveTrue();
        }
        for (int i = gridHeadCount; i < GridHeadMax; i++)
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
                int tagCount = detail.TagInfoList.Count;
                for (int i = 0; i < Math.Min(TagMax, tagCount); i++)
                {
                    tagList[i].SetActiveTrue();
                    tagList[i].TxtType.text = detail.TagInfoList[i].TagName;
                    tagList[i].ImgBack.color = detail.TagInfoList[i].BackColor;
                    tagList[i].ImgIcon.sprite = detail.TagInfoList[i].Icon;
                }
                for (int i = tagCount; i < TagMax; i++)
                {
                    tagList[i].SetActiveFalse();
                }
                TxtDetail.text = detail.Detail;
                
                int inSpinLineCount = detail.InSpinLineList.Count;
                for (int i = 0; i < Math.Min(SpinLineMax, inSpinLineCount); i++)
                {
                    inSpinLineList[i].SetActiveTrue();
                    inSpinLineList[i].TxtLine.text = detail.InSpinLineList[i];
                }
                for (int i = inSpinLineCount; i < SpinLineMax; i++)
                {
                    inSpinLineList[i].SetActiveFalse();
                }
            };
        });
        curHead ??= headList[0];
        curHead.OnClick?.Invoke();
    }
}
