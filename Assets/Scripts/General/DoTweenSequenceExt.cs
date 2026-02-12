using UnityEngine;
using UnityEngine.UI;

public static class DoTweenSequenceExt
{
    public static void SetPositionX(this Transform t, float x)
    {
        var pos = t.position;
        pos.x = x;
        t.position = pos;
    }

    public static void SetPositionY(this Transform t, float y)
    {
        var pos = t.position;
        pos.y = y;
        t.position = pos;
    }

    public static void SetPositionZ(this Transform t, float z)
    {
        var pos = t.position;
        pos.z = z;
        t.position = pos;
    }

    public static void SetLocalPositionX(this Transform t, float x)
    {
        var pos = t.localPosition;
        pos.x = x;
        t.localPosition = pos;
    }

    public static void SetLocalPositionY(this Transform t, float y)
    {
        var pos = t.localPosition;
        pos.y = y;
        t.localPosition = pos;
    }

    public static void SetLocalPositionZ(this Transform t, float z)
    {
        var pos = t.localPosition;
        pos.z = z;
        t.localPosition = pos;
    }

    public static void SetLocalScaleX(this Transform t, float x)
    {
        var scale = t.localScale;
        scale.x = x;
        t.localScale = scale;
    }

    public static void SetLocalScaleY(this Transform t, float y)
    {
        var scale = t.localScale;
        scale.y = y;
        t.localScale = scale;
    }

    public static void SetLocalScaleZ(this Transform t, float z)
    {
        var scale = t.localScale;
        scale.z = z;
        t.localScale = scale;
    }

    public static void SetAnchoredPositionX(this RectTransform t, float x)
    {
        var pos = t.anchoredPosition;
        pos.x = x;
        t.anchoredPosition = pos;
    }

    public static void SetAnchoredPositionY(this RectTransform t, float y)
    {
        var pos = t.anchoredPosition;
        pos.y = y;
        t.anchoredPosition = pos;
    }

    public static void SetAnchoredPosition3Dz(this RectTransform t, float z)
    {
        var pos = t.anchoredPosition3D;
        pos.z = z;
        t.anchoredPosition3D = pos;
    }

    public static void SetColorAlpha(this Graphic g, float alpha)
    {
        var color = g.color;
        color.a = alpha;
        g.color = color;
    }

    public static Vector2 GetFlexibleSize(this LayoutElement le)
    {
        return new Vector2(le.flexibleWidth, le.flexibleHeight);
    }

    public static void SetFlexibleSize(this LayoutElement le, Vector2 size)
    {
        le.flexibleWidth = size.x;
        le.flexibleHeight = size.y;
    }

    public static Vector2 GetMinSize(this LayoutElement le)
    {
        return new Vector2(le.minWidth, le.minHeight);
    }

    public static void SetMinSize(this LayoutElement le, Vector2 size)
    {
        le.minWidth = size.x;
        le.minHeight = size.y;
    }

    public static Vector2 GetPreferredSize(this LayoutElement le)
    {
        return new Vector2(le.preferredWidth, le.preferredHeight);
    }

    public static void SetPreferredSize(this LayoutElement le, Vector2 size)
    {
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;
    }
}