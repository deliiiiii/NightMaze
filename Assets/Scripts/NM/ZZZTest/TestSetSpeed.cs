using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.ZZZTest;

internal class TestSetSpeed : MonoBehaviour
{
    [Button]
    public void SetSpeed(float tar = 1)
    {
        DOTween.timeScale = tar;
    }
}