using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeneralPreview;
#if UNITY_EDITOR
public static class AssetDatabaseExt
{
    extension(UnityEditor.AssetDatabase)
    {
        public static List<T> LoadAllAssetsRecursive<T>(string folder) where T : Object =>
            UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}", [folder])
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null)
                .ToList();
    }
}
#endif