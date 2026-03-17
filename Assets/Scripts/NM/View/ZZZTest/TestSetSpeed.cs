using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.ZZZTest;

internal class TestSetSpeed : MonoBehaviour
{
    [SerializeField][OnValueChanged(nameof(OnChanged))] float tar = 1;
    void OnChanged()
    {
        DOTween.timeScale = tar;
    }
}