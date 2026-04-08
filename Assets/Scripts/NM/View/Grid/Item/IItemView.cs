using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public abstract class ItemViewBase : ViewBase
{
    [HideInInspector]public GamePlaying.IItem Data { get; set; }
    public abstract void OnCreateView();
}

public abstract class ItemViewBase<TSub, TData> : ItemViewBase 
    where TSub : ItemViewBase<TSub, TData>
    where TData : class, GamePlaying.IItem
{
    [ShowInInspector, ReadOnly]
    public TData DataT
    {
        get => (TData)Data;
        set => Data = value;
    }
}