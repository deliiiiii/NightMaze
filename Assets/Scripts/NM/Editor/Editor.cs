using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeneralProj;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace RSTS.Editor
{
    public static class AddressableBatchProcessor
    {
        const string TargetFolderPath = "Assets/Config/"; // + NameC.Name;
        const string LabelName = NameC.ConfigTag;

        [MenuItem("Tools/" + NameC.Name + "/Mark Folder as Addressable with \"" + LabelName + "\" Label")]
        public static void MarkFolderAsAddressable()
        {
            if (!Directory.Exists(TargetFolderPath))
            {
                Debug.LogError($"文件夹不存在: {TargetFolderPath}");
                return;
            }
            string[] allFiles = Directory.GetFiles(TargetFolderPath, "*.*", SearchOption.AllDirectories);
            List<string> validAssetPaths = new List<string>();
            foreach (string file in allFiles)
            {
                if (!file.EndsWith(".meta"))
                {
                    string assetPath = file.Replace("\\", "/");
                    if (assetPath.StartsWith(Application.dataPath))
                    {
                        assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                    }
                    validAssetPaths.Add(assetPath);
                }
            }
            if (validAssetPaths.Count == 0)
            {
                Debug.Log("选择的文件夹中没有找到有效资源文件");
                return;
            }
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Settings 未找到，请确保Addressable包已安装并初始化");
                return;
            }
            if (!settings.GetLabels().Contains(LabelName)) 
                settings.AddLabel(LabelName);
            int processedCount = 0;
            foreach (string assetPath in validAssetPaths)
            {
                if (MarkSingleAssetAsAddressable(assetPath, settings)) 
                    processedCount++;
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();
            Debug.Log($"成功处理 {processedCount} 个文件，标记为Addressable并设置Label为: {LabelName}");
        }

        static bool MarkSingleAssetAsAddressable(string assetPath, AddressableAssetSettings settings)
        {
            try
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"无法获取资源的GUID: {assetPath}");
                    return false;
                }
                AddressableAssetEntry entry = settings.FindAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                if (entry != null)
                {
                    string address = Path.GetFileNameWithoutExtension(assetPath);
                    entry.address = address;
                    entry.labels.Add(LabelName);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理资源时出错 {assetPath}: {e.Message}");
            }

            return false;
        }
    }

    public static class ScriptableObjectOModifier
    {
        const string TargetFolderPath = "Assets/Config/RSTS/Cards";

        static void FilterAndDo(IEnumerable<ScriptableObject> sos, out List<ScriptableObject> modified)
        {
            var tar = sos
                // .OfType<CardConfigMulti>()
                // .Where(x => x.Color == ECardColor.Green)
                .ToList();
            tar.ForEach(x =>
            {
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
                Debug.LogError($"文件夹不存在: {TargetFolderPath}");
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
                Debug.Log($"在文件夹 {TargetFolderPath} 中未找到ScriptableObject");
                return;
            }

            FilterAndDo(targetObjects, out var modified);

            modified.ForEach(EditorUtility.SetDirty);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"成功修改 {modified.Count} 个对象");
        }
    }
}