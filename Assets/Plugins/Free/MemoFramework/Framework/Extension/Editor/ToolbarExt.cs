using ToolbarExtension;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemoFramework.Extension
{
    public static class ToolbarExt
    {
        [Toolbar(OnGUISide.Left, 0)]
        static void OnLaunchToolbarGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            {
                if (GUILayout.Button("Launch", EditorStyles.toolbarButton))
                {
                    SceneHelper.StartScene(SceneHelper.EntryScenePath);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private static class SceneHelper
        {
            public static readonly string EntryScenePath = "Assets/Scenes/Scene0.unity";
            private const string UnityEditorSceneToOpenKey = "UnityEditorSceneToOpen";

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void OnBeforeSceneLoad()
            {
                if (EditorPrefs.HasKey(UnityEditorSceneToOpenKey))
                {
                    string scenePath = EditorPrefs.GetString(UnityEditorSceneToOpenKey);
                    if (!SceneManager.GetActiveScene().path.Equals(scenePath))
                    {
                        SceneManager.LoadScene(scenePath);
                    }
                }
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            private static void OnAfterSceneLoad()
            {
                if (EditorPrefs.HasKey(UnityEditorSceneToOpenKey))
                {
                    EditorPrefs.DeleteKey(UnityEditorSceneToOpenKey);
                }
            }

            public static void StartScene(string scenePathName)
            {
                if (EditorApplication.isPlaying)
                {
                    return;
                }

                EditorPrefs.SetString(UnityEditorSceneToOpenKey, scenePathName);
                EditorApplication.isPlaying = true;
            }
        }
    }
}