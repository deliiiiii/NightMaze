using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using UnityEngine;

namespace NM.View;

public class GridMapScroll : MonoBehaviour
{
    // public float 
    void Awake()
    {
        IUniEvt.BindAll(this, destroyCancellationToken);
    }

    UniEvt<MyInput.EvtKeyDown> EvtMouseDown => new()
    {
        Des = "按下鼠标键",
        Invoke = (evt, ct) =>
        {
            if (evt.Key == KeyCode.Mouse0)
            {
                MyDebug.Log("0");
            }
            else if(evt.Key == KeyCode.Mouse1)
            {
                MyDebug.Log("1");
            }
            return UniTask.CompletedTask;
        },
    };
}