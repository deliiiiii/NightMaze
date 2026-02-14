using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

internal class TestSetSpeed : MonoBehaviour
{
    [Button]
    public void SetSpeed(float tar = 1)
    {
        DOTween.timeScale = tar;
    }
}