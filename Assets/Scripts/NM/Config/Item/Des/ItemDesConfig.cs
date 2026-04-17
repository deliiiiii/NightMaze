using System;
using System.Collections.Generic;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace NM.Config;
[Serializable]
public record ItemDesConfig
{
    public const string FromLast = "上{0}步中的: ";

    [LabelText("人话描述"), TextArea(2, 10)] public string DesToPlayer = "输入人话...";
    [SerializeReference, LabelText("触发器"), OnValueChanged(nameof(OnChanged))]
    public ItemDesTriggerBase? Trigger = new ItemDesTriggerEnterSpin();
    [SerializeReference, LabelText("尝试执行...")] 
    [ValueDropdown("@NM.Config.ItemDesConfig.GetOptionsDeep($property)", NumberOfItemsBeforeEnablingSearch = 1)] 
    public ItemDesResultBase? Result;
    void OnChanged()
    {
        if (!CheckResult())
            Result = null;
    }
    bool CheckResult()
    {
        if (Trigger is IItemDesInPlay)
            return Result is null or IItemDesInPlay;
        return Result is null or not IItemDesInSpin;
    }
    
#if UNITY_EDITOR
    public static List<ValueDropdownItem<ItemDesResultBase>> GetOptionsDeep(Sirenix.OdinInspector.Editor.InspectorProperty property)
    {
        var parent = property.Parent;
        ItemDesConfig? c = null;
        
        while (parent != null)
        {
            if (parent.ValueEntry?.WeakSmartValue is ItemDesConfig config)
            {
                c = config;
                break;
            }
            parent = parent.Parent;
        }
        bool inPlay = c is { Trigger: IItemDesInPlay };
        var subTypes = typeof(ItemDesResultBase).SubTypes();
        subTypes = inPlay 
            ? subTypes.Where(t => typeof(IItemDesInPlay).IsAssignableFrom(t))
            : subTypes.Where(t => typeof(IItemDesInSpin).IsAssignableFrom(t));
        return subTypes.Select(t =>
        {
            var attr = t.GetAttribute<TypeRegistryItemAttribute>();
            var text = t.Name;
            if (attr != null)
            {
                if(attr.CategoryPath != null && attr.CategoryPath.Any())
                    text = $"{attr.CategoryPath}/{attr.Name}";
                else
                    text = attr.Name;
            }
            return new ValueDropdownItem<ItemDesResultBase>
            {
                Text = text,
                Value = (ItemDesResultBase)Activator.CreateInstance(t)
            };
        }).ToList();
    }
#endif
}