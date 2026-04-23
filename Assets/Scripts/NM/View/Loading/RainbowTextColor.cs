using UnityEngine;
using TMPro;
[RequireComponent(typeof(TMP_Text))]
public class RainbowTextColor : MonoBehaviour
{
    [Header("设置")][Tooltip("颜色在色环上旋转的速度")]
    public float ColorSpeed = 0.5f;
    [Tooltip("饱和度 (0-1)，1为最鲜艳，即截图中心正方形的最右侧")][Range(0f, 1f)]
    public float Saturation = 1f;
    [Tooltip("明度 (0-1)，1为最亮，即截图中心正方形的最上方")][Range(0f, 1f)]
    public float Value = 1f;

    Txt mText;
    float currentHue;
    void Awake()
    {
        mText = GetComponent<Txt>(); 
    }
    void Update()
    {
        currentHue += ColorSpeed * Time.deltaTime;
        currentHue = Mathf.Repeat(currentHue, 1f);
        mText.color = Color.HSVToRGB(currentHue, Saturation, Value);
    }
}