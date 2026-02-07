using System.Collections.Generic;
using System.IO;
using System.Linq;
using General;
using GeneralProj;
using NM.Config;
using UnityEditor;
using UnityEngine;

namespace NM.Editor;

public static class ScriptableObjectOModifier
{
    const string TargetFolderPath = "Assets/Config/Symbol";

    static void FilterAndDo(IEnumerable<ScriptableObject> sos, out List<ScriptableObject> modified)
    {
        var tar = sos
            .OfType<SymbolConfig>()
            // .Where(x => x.Color == ECardColor.Green)
            .ToList();
        tar.ForEach(x =>
        {
            // x.DesList = x.EffList;
            // x.Upgrades.ForEach(upgrade =>
            // {
            //     upgrade.Des.EmbedTypes
            //         .OfType<EmbedAddBuff>()
            //         .ForEach(embedType =>
            //         {
            //             embedType.BuffData.StackCount.Value = embedType.BuffData.StackInfo?.Count ?? 0;
            //         });
            // });
        });
        
        modified = tar.OfType<ScriptableObject>().ToList();
    }
    
    
    [MenuItem("Tools/" + NameC.Name + "/Modify ScriptableObjects in Folder: ")]
    public static void ModifyScriptableObjectsInFolder()
    {
        if (!Directory.Exists(TargetFolderPath))
        {
            MyDebug.LogError($"文件夹不存在: {TargetFolderPath}");
            return;
        }

        // 获取文件夹内所有.asset文件
        string[] allAssetFiles = Directory.GetFiles(TargetFolderPath, "*.asset", SearchOption.AllDirectories);
        var targetObjects = new List<ScriptableObject>();

        // 查找所有指定类型的ScriptableObject
        foreach (string assetFile in allAssetFiles)
        {
            string assetPath = assetFile.Replace("\\", "/");
            if (assetPath.StartsWith(Application.dataPath))
            {
                assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
            }

            targetObjects.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath));
        }

        if (targetObjects.Count == 0)
        {
            MyDebug.Log($"在文件夹 {TargetFolderPath} 中未找到ScriptableObject");
            return;
        }

        FilterAndDo(targetObjects, out var modified);

        modified.ForEach(EditorUtility.SetDirty);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        MyDebug.Log($"成功修改 {modified.Count} 个对象");
    }
}