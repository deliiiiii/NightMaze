using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace General.Editor
{
    public class MyAsset
    {
        public static bool TryLoadFirstAsset<T>([CanBeNull][NotNullWhen(true)] out T foundAsset) 
            where T : ScriptableObject
        {
            foundAsset = null;
            string typeName = typeof(T).Name;
            string prefsKey = $"General_Editor_{typeName}_GUID";
            string cachedGuid = EditorPrefs.GetString(prefsKey, string.Empty);
            
            if (!string.IsNullOrEmpty(cachedGuid))
            {
                string cachedPath = AssetDatabase.GUIDToAssetPath(cachedGuid);
                if (!string.IsNullOrEmpty(cachedPath))
                {
                    foundAsset = AssetDatabase.LoadAssetAtPath<T>(cachedPath);
                    if (foundAsset != null) return true;
                }
            }

            string[] guids = AssetDatabase.FindAssets($"t:{typeName}");
            if (guids.Length > 0)
            {
                string guid = guids[0];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                foundAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                
                if (foundAsset != null)
                {
                    EditorPrefs.SetString(prefsKey, guid);
                    return true;
                }
            }
            return false;
        }
    }
}