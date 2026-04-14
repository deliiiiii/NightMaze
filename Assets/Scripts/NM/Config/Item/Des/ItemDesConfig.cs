using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[Serializable]
public record ItemDesConfig
{
    // [SerializeReference, ValueDropdown(nameof(GetEvtList), NumberOfItemsBeforeEnablingSearch = 1)]
    // public required Type UniEvtType;
    //
    // [field:MaybeNull]
    // List<ValueDropdownItem<Type>> GetEvtList
    // {
    //     get
    //     {
    //         if (field != null)
    //             return field;
    //         field = AppDomain.CurrentDomain
    //                     .GetAssemblies()
    //                     .FirstOrDefault(a => a.GetName().Name == "NM.Data")
    //                     ?.GetTypes()
    //                     .Where(t => typeof(IEvtBase).IsAssignableFrom(t) && !t.IsAbstract)
    //                     .Select(t => new ValueDropdownItem<Type>()
    //                     {
    //                         Text = t.GetAttribute<EvtNameAttribute>()?.Name ?? t.Name,
    //                         Value = t
    //                     }).ToList()
    //                 ?? [];
    //         return field;
    //     }
    // }

    public const string FromLast = "上{0}步中的: ";

    [LabelText("人话描述"), MultiLineProperty(2)] public string DesToPlayer = "输入人话...";
    [Required, SerializeReference, LabelText("触发器"), OnValueChanged(nameof(OnChanged))]
    public ItemDesTriggerBase Trigger = new ItemDesTriggerEnterSpin();
    [Header("尝试执行.."), HideLabel]
    [SerializeReference] public ItemDesResultBase? Result;
    
    void OnChanged()
    {
        Trigger ??= new ItemDesTriggerEnterSpin();
    }
}