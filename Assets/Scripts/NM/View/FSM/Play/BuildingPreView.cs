using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
using NM.Data;
using UnityEngine.EventSystems;

namespace NM.View;

public class BuildingPreView : ViewBase,
    IMultiBeginDragHandler,
    IMultiDragHandler,
    IMultiEndDragHandler
{
    public ItemConfig ToBuildConfig => ConfigLoader.Acquire<ItemConfig>(20002);
    public ItemView DraggingView;

    void Start()
    {
        var data = new GamePlaying.MyItem(ToBuildConfig.ID, Vector2Int.Zero);
        DraggingView = Instantiate(PlayViewIns.PfbItemView, PlayViewIns.TrsToBuild);
        DraggingView.SetActiveFalse();
        DraggingView.OnCreateView(data);
    }
    public void OnMultiBeginDrag(PointerEventData eventData)
    {
        if (PlayViewIns.Data.ToDoList.FirstOrDefault() is GamePlaying.ActWaitForSpin)
        {
            PlayViewIns.InstantInfoView.ShowAsync("正在结算/未收获, 暂不能建造.").Forget();
            return;
        }
        DraggingView.SetActiveTrue();
    }
    public void OnMultiDrag(PointerEventData eventData)
    {
        if(!DraggingView.gameObject.activeSelf)
            return;
        DraggingView.transform.position = MyCamera.Main.ScreenToWorldPoint(eventData.position);
        DraggingView.transform.SetPositionZ(0);
    }

    public void OnMultiEndDrag(PointerEventData eventData)
    {
        if(!DraggingView.gameObject.activeSelf)
            return;
        DraggingView.SetActiveFalse();
        var pos = PlayView.ScreenToGrid(eventData.position);
        DraggingView.Data.PivotPos = pos;
        new GamePlaying.ActSpawnItemAtPos(PlayViewIns.Data)
        {
            Id = ToBuildConfig.ID,
            Pos = pos,
            ResultWrap = null
        }.Forget();
    }
}