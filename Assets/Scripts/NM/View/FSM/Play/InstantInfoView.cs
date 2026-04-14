using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using UnityEngine;

namespace NM.View;

public class InstantInfoView : ViewBase
{
    [SerializeField] DOTweenSequence tween;
    [SerializeField] Txt txtInfo;
    [SerializeField] int hideDelayMs = 1500;
    CancellationTokenSource cts = new();
    public async UniTask ShowAsync(string info)
    {
        cts.Cancel();
        cts = new();
        txtInfo.text = info;
        gameObject.SetActiveTrue();
        await tween.PlayAsync(cts.Token);
        await UniTask.Delay(hideDelayMs, cancellationToken: cts.Token);
        Hide();
    }
    public void Hide()
    {
        gameObject.SetActiveFalse();
    }
}