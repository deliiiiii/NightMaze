using General;
using UnityEditor;
using UnityEngine;

namespace NM.Config.Editor;

public static class ItemConfigCreator
{
    const string Path = "Assets/Create/NM/";
    [MenuItem(Path + "0_新地块", false, 0)]
    public static void Create0() => Create(EItemType.Grid);
    [MenuItem(Path + "1_新棋子", false, 1)]
    public static void Create1() => Create(EItemType.Symbol);
    [MenuItem(Path + "2_新建筑", false, 2)]
    public static void Create2() => Create(EItemType.Building);
    [MenuItem(Path + "3_新资源", false, 3)]
    public static void Create3() => Create(EItemType.Resource);
    [MenuItem(Path + "4_新事件", false, 4)]
    public static void Create4() => Create(EItemType.Event);

    static void Create(EItemType itemType)
    {
        ItemConfig asset = ScriptableObject.CreateInstance<ItemConfig>();
        asset.ItemType = itemType;
        ProjectWindowUtil.CreateAsset(asset, $"新{itemType.GetLabelText().Split('_')[1]}.asset");
    }
    
}