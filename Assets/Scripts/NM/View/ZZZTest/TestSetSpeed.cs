using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View.ZZZTest;

internal class TestSetSpeed : MonoBehaviour
{
    [SerializeField][OnValueChanged(nameof(OnChanged))] float tar = 1;
    void OnChanged()
    {
        DOTween.timeScale = tar;
    }

    public void Test<T>() where T : ITestInterface<T>
    {
        var x = Factory<T>.Create();
    }
}

public static class TExt
{
    extension<T>(Factory<T>) where T : ITestInterface<T>
    {
        public static T Create() => default!;
    }
}
// ReSharper disable once UnusedTypeParameter
public static class Factory<T>;
public interface ITestInterface<out T>
{
    // CS9281: 目标运行时不支持接口中的 static abstract 成员
    // static abstract T Create();
}