using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace NM.Config;
[Serializable]
public class EvtConfig
{
    [SerializeReference, ValueDropdown(nameof(GetEvtList), NumberOfItemsBeforeEnablingSearch = 1)]
    public required Type UniEvtType;

    [field:MaybeNull]
    List<ValueDropdownItem<Type>> GetEvtList
    {
        get
        {
            if (field != null)
                return field;
            field = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "NM.Data")
                        ?.GetTypes()
                        .Where(t => typeof(IEvtBase).IsAssignableFrom(t) && !t.IsAbstract)
                        .Select(t => new ValueDropdownItem<Type>()
                        {
                            Text = t.GetAttribute<EvtNameAttribute>()?.Name ?? t.Name,
                            Value = t
                        }).ToList()
                    ?? [];
            return field;
        }
    }
}

