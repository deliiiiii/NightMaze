using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace NM.Config.Editor;

public static class MgrEditor
{
#if UNITY_EDITOR
    public static TagMgr? LoadTagMgr(string assetName)
     => UnityEditor.AssetDatabase.LoadAssetAtPath<TagMgr>(Const.Res.Config.EnumPre + assetName + ".asset");
    public static List<ValueDropdownItem<int>> GetConfigEnumDropDownList(string assetName)
    {
        TagMgr? config = LoadTagMgr(assetName);
        return config == null 
            ? []
            : [..config.TagList.Select(tagEntry => new ValueDropdownItem<int>(text: tagEntry.Tag,value: tagEntry.ID))];
    }
    public static List<ValueDropdownItem<TagEntry>> GetConfigEnumDropDownList2(string assetName)
    {
        TagMgr? config = LoadTagMgr(assetName);
        return config == null 
            ? []
            : [..config.TagList.Select(tagEntry => new ValueDropdownItem<TagEntry>(text: tagEntry.Tag ,value: tagEntry))];
    }
#endif
}