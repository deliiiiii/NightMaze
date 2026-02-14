using System.Collections.Generic;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SymbolView : MonoBehaviour
{
    [SerializeField, Required] DOTweenSequence onSpinTween;

    [ShowInInspector, ReadOnly]
    public SymbolEtt SymbolEtt
    {
        get;
        set
        {
            field = value;
            TxtName.text = field.IsEmpty ? string.Empty : field.Config.Name;
            TxtAdd.text = string.Empty;
            TxtMulti.text = string.Empty;
            buffList.ForEach(Destroy);
            buffList.Clear();
        }
    } = null!;

    public TextMeshProUGUI TxtName;
    public TextMeshProUGUI TxtAdd;
    public TextMeshProUGUI TxtMulti;
    [SerializeField] Transform tranImgBuff;
    [ShowInInspector, ReadOnly] List<ImgBuff> buffList = [];


    void Awake()
    {
        OnEvt().RegAll();
    }

    IEnumerable<IUniEvt> OnEvt()
    {
        yield return new UniEvt<EvtSpinImmediateDoSymbol>()
        {
            DoAsync = async (evt, ct) =>
            {
                if (evt.Arg1 != SymbolEtt || SymbolEtt.IsEmpty)
                    return;
                await onSpinTween.PlayAsync(ct);
            },
            Des = $"{SymbolEtt?.Config.Name ?? ""} 放变红动画"
        };
    }
}