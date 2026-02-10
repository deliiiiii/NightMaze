using System.Collections.Generic;
using NM.Data;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SymbolModel : MonoBehaviour
{
    [field: SerializeReference]
    public SymbolEtt SymbolEtt
    {
        get => field;
        set
        {
            field = value;
            TxtName.text = field.Config.ID == -1 ? string.Empty : field.Config.Name;
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

    public void SetEmpty()
    {
        SymbolEtt = SymbolEtt.CreateEmptySymbol();
    }
}