using System;
using UnityEngine;

public static class TransformExt
{
    public static Transform DestroyActiveChildren(this Transform t)
    {
        for(int i = 0; i < t.childCount; i++)
        {
            if (!t.GetChild(i).gameObject.activeSelf)
                continue;
            GameObject.Destroy(t.GetChild(i).gameObject);
        }
        return t;
    }

    public static Transform DestroyChild(this Transform t, Predicate<Transform> filter)
    {
        for(int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            if (!filter(child))
                continue;
            GameObject.Destroy(child.gameObject);
        }
        return t;
    }
}