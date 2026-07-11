using System;
using System.Collections.Generic;
using System.Linq;
using General;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace NM.Editor;

internal sealed class AddressableBatchProcessor : EditorWindow
{
    readonly List<AddressableFolderRule> rules = [];
    Vector2 scrollPosition;

    [MenuItem("Tools/" + Const.Name.Proj + "/" + nameof(AddressableBatchProcessor))]
    public static void ShowWindow()
    {
        GetWindow<AddressableBatchProcessor>("Addressable Folder Rules");
    }

    void OnEnable() => RefreshRules();

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "每个规则资产管理一个 Addressable 文件夹。目录内增删资源不需要新增规则。",
            MessageType.Info);

        using var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition);
        scrollPosition = scroll.scrollPosition;

        foreach (var rule in rules.Where(rule => rule != null))
            DrawRule(rule);

        GUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Create Folder Rule", GUILayout.Height(28)))
                CreateRuleAsset();
            if (GUILayout.Button("Refresh", GUILayout.Width(90), GUILayout.Height(28)))
                RefreshRules();
        }

        GUILayout.Label($"Total Rules: {rules.Count}", EditorStyles.miniLabel);
    }

    static void DrawRule(AddressableFolderRule rule)
    {
        var serializedRule = new SerializedObject(rule);
        serializedRule.Update();

        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(serializedRule.FindProperty(nameof(AddressableFolderRule.Enable)),
                    GUIContent.none, GUILayout.Width(18));
                EditorGUILayout.ObjectField(rule, typeof(AddressableFolderRule), false);
            }
            EditorGUILayout.PropertyField(serializedRule.FindProperty(nameof(AddressableFolderRule.Folder)));
            EditorGUILayout.PropertyField(serializedRule.FindProperty(nameof(AddressableFolderRule.Tag)));
        }

        serializedRule.ApplyModifiedProperties();
    }

    void CreateRuleAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Addressable Folder Rule",
            "AddressableFolderRule",
            "asset",
            "每个资源类别创建一个独立规则资产", path: "Assets/Config/Tags");
        if (string.IsNullOrEmpty(path)) return;

        var rule = CreateInstance<AddressableFolderRule>();
        AssetDatabase.CreateAsset(rule, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = rule;
        RefreshRules();
    }

    void RefreshRules()
    {
        rules.Clear();
        rules.AddRange(LoadRules());
        Repaint();
    }

    static List<AddressableFolderRule> LoadRules() => AssetDatabase
        .FindAssets($"t:{nameof(AddressableFolderRule)}")
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadAssetAtPath<AddressableFolderRule>)
        .Where(rule => rule != null)
        .OrderBy(AssetDatabase.GetAssetPath, StringComparer.Ordinal)
        .ToList();

    [InitializeOnEnterPlayMode]
    static void SyncOnEnterPlayMode() => ProcessRules(LoadRules());

    static void ProcessRules(IReadOnlyCollection<AddressableFolderRule> allRules)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            MyDebug.LogError("Addressable Settings 未找到，请确保 Addressable 包已安装并初始化");
            return;
        }

        var enabledRules = allRules.Where(rule => rule.Enable).ToList();
        var categoryLabels = allRules
            .Where(rule => !string.IsNullOrWhiteSpace(rule.Tag))
            .Select(rule => rule.Tag.Trim())
            .ToHashSet();
        var desiredEntries = new Dictionary<string, DesiredEntry>();
        var addressOwners = new Dictionary<string, string>();

        // 先完整校验，避免错误时只修改一部分 Addressables 配置。
        foreach (var rule in enabledRules)
        {
            string folderPath = rule.FolderPath;
            string label = rule.Tag.Trim();
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(label))
            {
                MyDebug.LogError($"Addressable 文件夹规则无效: {AssetDatabase.GetAssetPath(rule)}");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                MyDebug.LogError($"不是有效的 Unity 资产目录: {folderPath}");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(folderPath);
            if (string.IsNullOrEmpty(guid))
            {
                MyDebug.LogError($"无法取得文件夹 GUID: {folderPath}");
                return;
            }
            if (desiredEntries.TryGetValue(guid, out var previous) && previous.Label != label)
            {
                MyDebug.LogError($"文件夹同时命中多个 Addressable 规则: {folderPath}");
                return;
            }
            if (addressOwners.TryGetValue(label, out var ownerGuid) && ownerGuid != guid)
            {
                MyDebug.LogError($"Addressable 文件夹地址重复: {label}\n" +
                                 $"{AssetDatabase.GUIDToAssetPath(ownerGuid)}\n{folderPath}");
                return;
            }

            desiredEntries[guid] = new DesiredEntry(label, label);
            addressOwners[label] = guid;
        }
        int added = 0;
        int changed = 0;
        int removed = 0;
        foreach (var (guid, desired) in desiredEntries)
        {
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
            if (entry.labels.Count != 1 ||
                !entry.labels.Contains(desired.Label))
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
                             entry.labels.Any(categoryLabels.Contains) &&
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
