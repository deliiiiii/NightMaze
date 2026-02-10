using UnityEngine;

namespace GeneralPreview;

public abstract class ViewBase : MonoBehaviour
{
    protected abstract void Bind();

    void Awake()
    {
        Bind();
    }
}