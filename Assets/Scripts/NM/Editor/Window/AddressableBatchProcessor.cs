using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using General;
using General.Editor;
using GeneralPreview;
using NM.Config;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace NM.Editor;
internal class AddressableBatchProcessor : EditorWindow
{
    AddressableBatchConfig? config;
    Vector2 scrollPosition;
    string[] fieldNames = [];
    string[] fieldValues = [];
        
    [MenuItem("Tools/" + Const.Name.Proj + "/" + nameof(AddressableBatchProcessor))]
    public static void ShowWindow()
    {
        GetWindow<AddressableBatchProcessor>("Addressable Tool");
    }
    void OnEnable()
    {
        IUniEvt.BindAll(this, CancellationToken.None);
        if (config == null)
        {
            MyAsset.TryLoadFirstAsset(out config);
        }

        var fieldInfoList = typeof(Const.Res.AddrTag)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.Name.EndsWith("Tag"))
            .ToList();
        fieldNames = fieldInfoList.Select(f => $"{f.Name} (= \"{f.GetValue(null)}\")").ToArray();
        fieldValues = fieldInfoList.Select(f => (string)f.GetValue(null)).ToArray();
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        config = (AddressableBatchConfig)EditorGUILayout.ObjectField("Configuration Asset", config,
            typeof(AddressableBatchConfig), false);

        if (config is null)
        {
            if (GUILayout.Button("Create New Config", GUILayout.Width(130)))
            {
                CreateConfigAsset();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (config is null)
        {
            EditorGUILayout.HelpBox("Please select or create a Configuration Asset to continue.", MessageType.Info);
            return;
        }

        GUILayout.Space(10);

        SerializedObject so = new SerializedObject(config);
        so.Update();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", GUILayout.Width(20)); // Toggle 占位
        GUILayout.Label("Folder Path", GUILayout.MinWidth(150));
        GUILayout.Label("TagName", GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
            
        for (int i = 0; i < config.RuleList.Count; i++)
        {
            DrawRuleItem(i);
        }

        if (GUILayout.Button("+ Add New Rule", GUILayout.Height(25)))
        {
            config.RuleList.Add(new BatchRule());
            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.EndScrollView();
        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        GUILayout.Label($"Total Rules: {config.RuleList.Count}", EditorStyles.miniLabel);

    }

    void DrawRuleItem(int index)
    {
        var rule = config!.RuleList[index];

        EditorGUILayout.BeginHorizontal("box");

        bool newEnable = EditorGUILayout.Toggle(rule.Enable, GUILayout.Width(20));
        if (newEnable != rule.Enable)
        {
            rule.Enable = newEnable;
            EditorUtility.SetDirty(config);
        }

        // 文件夹路径选择
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        // 如果未启用，禁用GUI显示（变灰）
        EditorGUI.BeginDisabledGroup(!rule.Enable); 
            
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField(rule.FolderPath);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path[Application.dataPath.Length..];
                }
                rule.FolderPath = path;
                EditorUtility.SetDirty(config);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // Tag 下拉选择
        if (fieldNames is { Length: > 0 })
        {
            int currentIndex = Array.IndexOf(fieldValues, rule.TagName);
            if (currentIndex == -1) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup(currentIndex, fieldNames, GUILayout.Width(150));
            string newValue = fieldValues[newIndex];
            if (newValue != rule.TagName)
            {
                rule.TagName = newValue;
                EditorUtility.SetDirty(config);
            }
        }
        else
        {
            string newTag = EditorGUILayout.TextField(rule.TagName, GUILayout.Width(150));
            if (newTag != rule.TagName)
            {
                rule.TagName = newTag;
                EditorUtility.SetDirty(config);
            }
        }
            
        EditorGUI.EndDisabledGroup();

        // 删除按钮
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            config.RuleList.RemoveAt(index);
            EditorUtility.SetDirty(config);
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();
    }

    void CreateConfigAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Config", "AddressableBatchConfig", "asset", "Save Configuration");
        if (string.IsNullOrEmpty(path)) return;
        var newConfig = CreateInstance<AddressableBatchConfig>();
        AssetDatabase.CreateAsset(newConfig, path);
        AssetDatabase.SaveAssets();
        config = newConfig;
    }
    
    [InitializeOnEnterPlayMode]
    static void SyncOnEnterPlayMode()
    {
        SyncConfig();
    }
    
    static void SyncConfig()
    {
        if(MyAsset.TryLoadFirstAsset<AddressableBatchConfig>(out var cfg))
            ProcessConfig(cfg);
    }
    
    static void ProcessConfig(AddressableBatchConfig configToProcess)
    {
        if (configToProcess == null || configToProcess.RuleList.Count == 0) 
            return;
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            MyDebug.LogError("Addressable Settings 未找到，请确保 Addressable 包已安装并初始化");
            return;
        }

        var enabledRules = configToProcess.RuleList
            .Where(rule => rule.Enable)
            .ToList();
        var managedLabels = configToProcess.RuleList
            .Where(rule => !string.IsNullOrEmpty(rule.TagName))
            .Select(rule => rule.TagName)
            .ToHashSet();
        var desiredEntries = new Dictionary<string, DesiredEntry>();
        var addressOwners = new Dictionary<string, string>();

        // 先完整扫描并校验，出错时不修改任何 Addressables 配置。
        foreach (var rule in enabledRules)
        {
            if (string.IsNullOrEmpty(rule.FolderPath) || string.IsNullOrEmpty(rule.TagName))
            {
                MyDebug.LogError("Addressable 规则无效：路径或 Tag 为空");
                return;
            }
            string folderPath = rule.FolderPath.Replace("\\", "/").TrimEnd('/');
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                MyDebug.LogError($"文件夹不存在或不是有效的 Unity 资产目录: {folderPath}");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(folderPath);
            if (string.IsNullOrEmpty(guid))
            {
                MyDebug.LogError($"无法取得文件夹 GUID: {folderPath}");
                return;
            }

            // 文件夹 Address 使用唯一的 Tag；子资源由 Addressables 按相对路径生成隐式 Address。
            string address = rule.TagName;
            if (desiredEntries.TryGetValue(guid, out var previous) && previous.Label != rule.TagName)
            {
                MyDebug.LogError($"文件夹同时命中多个 Addressable 规则: {folderPath}");
                return;
            }
            if (addressOwners.TryGetValue(address, out var ownerGuid) && ownerGuid != guid)
            {
                MyDebug.LogError($"Addressable 文件夹地址重复: {address}\n" +
                                 $"{AssetDatabase.GUIDToAssetPath(ownerGuid)}\n{folderPath}");
                return;
            }

            desiredEntries[guid] = new DesiredEntry(address, rule.TagName);
            addressOwners[address] = guid;
        }

        int added = 0;
        int changed = 0;
        int removed = 0;

        foreach (string label in managedLabels)
        {
            if (!settings.GetLabels().Contains(label))
            {
                settings.AddLabel(label);
                changed++;
            }
        }

        foreach (var pair in desiredEntries)
        {
            string guid = pair.Key;
            DesiredEntry desired = pair.Value;
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                entry.address = desired.Address;
                entry.labels.Clear();
                entry.labels.Add(desired.Label);
                added++;
                continue;
            }

            bool entryChanged = false;
            if (entry.parentGroup != settings.DefaultGroup)
            {
                entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                entryChanged = true;
            }
            if (entry.address != desired.Address)
            {
                entry.address = desired.Address;
                entryChanged = true;
            }
            if (entry.labels.Count != 1 || !entry.labels.Contains(desired.Label))
            {
                entry.labels.Clear();
                entry.labels.Add(desired.Label);
                entryChanged = true;
            }
            if (entryChanged) changed++;
        }

        var staleEntryGuids = settings.groups
            .Where(group => group != null)
            .SelectMany(group => group.entries)
            .Where(entry => entry != null &&
                            entry.labels.Any(managedLabels.Contains) &&
                            !desiredEntries.ContainsKey(entry.guid))
            .Select(entry => entry.guid)
            .Distinct()
            .ToList();
        foreach (string guid in staleEntryGuids)
        {
            if (settings.RemoveAssetEntry(guid)) removed++;
        }

        int totalChanges = added + changed + removed;
        if (totalChanges == 0)
        {
            MyDebug.Log($"Addressable 检查完成：{desiredEntries.Count} 个文件夹，无变化");
            return;
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        AssetDatabase.SaveAssets();
        MyDebug.Log($"Addressable 文件夹自动同步完成：新增 {added}，修改 {changed}，移除 {removed}");
    }

    readonly record struct DesiredEntry(string Address, string Label);
}
