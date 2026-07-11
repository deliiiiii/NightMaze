using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace General.Editor
{
    internal class MyDebugWindow : EditorWindow
    {
        [CanBeNull] MyDebugConfig config;
        Vector2 scrollPosition;
        bool showGlobalSwitches = true;

        [MenuItem("Tools/General/MyDebug Settings")]
        static void ShowWindow()
        {
            GetWindow<MyDebugWindow>("Debug Config");
        }

        void OnEnable()
        {
            if (config == null)
            {
                MyAsset.TryLoadFirstAsset(out config);
            }
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            config = (MyDebugConfig)EditorGUILayout.ObjectField("Configuration Asset", config, typeof(MyDebugConfig), false);

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

            // 监听 GUI 变化，一有变动立即 Apply
            EditorGUI.BeginChangeCheck();

            SerializedObject so = new SerializedObject(config);
            so.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showGlobalSwitches = EditorGUILayout.Foldout(showGlobalSwitches, "Global Switches", true);
            if (showGlobalSwitches)
            {
                EditorGUILayout.PropertyField(so.FindProperty(nameof(MyDebugConfig.CanLogAll)));
                EditorGUILayout.PropertyField(so.FindProperty(nameof(MyDebugConfig.CanLog)));
                EditorGUILayout.PropertyField(so.FindProperty(nameof(MyDebugConfig.CanLogWarning)));
                EditorGUILayout.PropertyField(so.FindProperty(nameof(MyDebugConfig.CanLogError)));
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.LabelField($"已勾选{config.ActiveLogTypes.Count}个", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, "box");
            
            foreach (var type in (ELogType[])Enum.GetValues(typeof(ELogType)))
            {
                bool isIncluded = config.ActiveLogTypes.Contains(type);
                
                bool newState = EditorGUILayout.Toggle(type.GetLabelText(), isIncluded);

                if (newState != isIncluded)
                {
                    if (newState)
                    {
                        config.ActiveLogTypes.Add(type);
                    }
                    else
                    {
                        config.ActiveLogTypes.Remove(type);
                    }
                    EditorUtility.SetDirty(config);
                }
            }
            
            EditorGUILayout.EndScrollView();

            so.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                ApplySettingsToStatic(config);
            }
        }


        void CreateConfigAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Config", "MyDebugConfig", "asset", "Save Configuration");
            if (string.IsNullOrEmpty(path)) return;

            var newConfig = CreateInstance<MyDebugConfig>();
            
            newConfig.ActiveLogTypes = ((ELogType[])Enum.GetValues(typeof(ELogType))).ToHashSet();

            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            config = newConfig;
            
            ApplySettingsToStatic(config);
        }


        [InitializeOnLoadMethod]
        static void RegisterPlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.delayCall += AutoApplyConfig;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
             if (state is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.EnteredEditMode)
             {
                 AutoApplyConfig();
             }
        }

        static void AutoApplyConfig()
        {
             if (MyAsset.TryLoadFirstAsset<MyDebugConfig>(out var foundConfig)) 
                 ApplySettingsToStatic(foundConfig);
        }


        static void ApplySettingsToStatic(MyDebugConfig configToProcess)
        {
            MyDebug.ApplySettings(
                configToProcess.CanLogAll,
                configToProcess.CanLog,
                configToProcess.CanLogWarning,
                configToProcess.CanLogError,
                configToProcess.ActiveLogTypes.ToHashSet()
            );
        }
    }
}
