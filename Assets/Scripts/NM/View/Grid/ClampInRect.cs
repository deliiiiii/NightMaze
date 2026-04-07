using System;
using UnityEngine;

namespace NM.View;

public class ClampInRect : MonoBehaviour
{
    [SerializeField] BoxCollider2D boxCollider2D;
    Rect? rect;


    public void Clamp()
    {
        var curPos = transform.position;
        var bounds = boxCollider2D.bounds;
        transform.position = new Vector3(
            Math.Clamp(curPos.x, bounds.min.x, bounds.max.x),
            Math.Clamp(curPos.y, bounds.min.y, bounds.max.y),
            curPos.z);
    }
}