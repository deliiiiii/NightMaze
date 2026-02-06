using System;
using System.IO;
using System.Linq;
using System.Reflection;
using General;
using GeneralProj;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace NM.Editor
{
    

    internal class AddressableBatchProcessor : EditorWindow
    {
        AddressableBatchConfig config;
        Vector2 scrollPosition;
        string[] fieldNames;
        string[] fieldValues;
        
        [MenuItem("Tools/" + NameC.Name + "/" + nameof(AddressableBatchProcessor))]
        public static void ShowWindow()
        {
            GetWindow<AddressableBatchProcessor>("Addressable Tool");
        }

        public static void ShowWindowWithArg(AddressableBatchConfig config)
        {
            var window = GetWindow<AddressableBatchProcessor>("Addressable Tool");
            window.config = config;
        }

        void OnEnable()
        {
            var fieldInfoList = typeof(NameC)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.Name.EndsWith("Tag"))
                .ToList();
            fieldNames = fieldInfoList.Select(f => $"{f.Name} (= \"{f.GetValue(null)}\")").ToArray();
            fieldValues = fieldInfoList.Select(f => f.GetValue(null) as string).ToArray();
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

            // GUI.backgroundColor = Color.green;
            // if (GUILayout.Button("Process All Rules", GUILayout.Height(40)))
            // {
            //     ProcessAllRules();
            // }
            // GUI.backgroundColor = Color.white;
        }

        void DrawRuleItem(int index)
        {
            var rule = config.RuleList[index];

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

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(AddressableBatchConfig)}");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var cfg = AssetDatabase.LoadAssetAtPath<AddressableBatchConfig>(path);
                    if (cfg != null)
                    {
                        ProcessConfig(cfg);
                    }
                }
            }
        }

        void ProcessAllRules()
        {
            if (config is null)
            {
                Debug.LogWarning("没有配置规则可执行。");
                return;
            }
            ProcessConfig(config);
        }

        public static void ProcessConfig(AddressableBatchConfig configToProcess)
        {
            if (configToProcess == null || configToProcess.RuleList.Count == 0) return;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                MyDebug.LogError("Addressable Settings 未找到，请确保 Addressable 包已安装并初始化");
                return;
            }

            foreach (var rule in configToProcess.RuleList.Where(rule => rule.Enable))
            {
                if (!string.IsNullOrEmpty(rule.TagName) && !settings.GetLabels().Contains(rule.TagName))
                {
                    settings.AddLabel(rule.TagName);
                }
            }

            int totalProcessed = 0;

            foreach (var rule in configToProcess.RuleList.Where(rule => rule.Enable))
            {
                if (string.IsNullOrEmpty(rule.FolderPath) || string.IsNullOrEmpty(rule.TagName))
                {
                    MyDebug.LogWarning($"跳过无效规则: 路径或 Tag 为空");
                    continue;
                }

                totalProcessed += MarkFolderAsAddressable(rule.FolderPath, rule.TagName, settings);
            }

            if (totalProcessed > 0)
            {
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
                AssetDatabase.SaveAssets();
                MyDebug.Log($"AddressableBatchProcessor: (Auto) Processed {totalProcessed} files.");
            }
        }

        static int MarkFolderAsAddressable(string targetFolderPath, string labelName, AddressableAssetSettings settings)
        {
            if (!Directory.Exists(targetFolderPath))
            {
                MyDebug.LogError($"文件夹不存在: {targetFolderPath}");
                return 0;
            }
            string[] allFiles = Directory.GetFiles(targetFolderPath, "*.*", SearchOption.AllDirectories);
            int count = 0;
            foreach (string file in allFiles)
            {
                if (file.EndsWith(".meta")) continue;
                string assetPath = file.Replace("\\", "/");
                if (assetPath.StartsWith(Application.dataPath))
                    assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                if (MarkSingleAssetAsAddressable(assetPath, settings, labelName))
                    count++;
            }
            return count;
        }
        static bool MarkSingleAssetAsAddressable(string assetPath, AddressableAssetSettings settings, string labelName)
        {
            try
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid)) return false;
                AddressableAssetEntry entry = settings.FindAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                if (entry != null)
                {
                    string address = Path.GetFileNameWithoutExtension(assetPath);
                    entry.address = address;
                    entry.labels.Add(labelName);
                    return true;
                }
            }
            catch (Exception e)
            {
                MyDebug.LogError($"处理资源时出错 {assetPath}: {e.Message}");
            }
            return false;
        }
    }
}
