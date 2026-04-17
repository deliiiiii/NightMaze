using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
[CreateAssetMenu(fileName = nameof(TagMgr), menuName = "NM/Mgr/" + nameof(TagMgr))]
public class TagMgr : ConfigSingle<TagMgr>
{
    public ImmutableList<TagEntry> TagList => tagList.ToImmutableList();
    
    [SerializeField, LabelText("Tags")]
    [ListDrawerSettings(CustomAddFunction = nameof(CreateNewTag), HideRemoveButton = true, DraggableItems = false)]
    List<TagEntry> tagList = [];

    TagEntry CreateNewTag()
    {
        int newId = 0;
        while (tagList.Any(t => t.ID == newId))
        {
            newId++;
        }
        return new TagEntry { ID = newId, Tag = string.Empty };
    }

 
}

[Serializable]
public record TagEntry
{
    [ReadOnly, HorizontalGroup("Row", Width = 50), HideLabel]
    public int ID;
    [HorizontalGroup("Row"), HideLabel]
    public string Tag = string.Empty;
}