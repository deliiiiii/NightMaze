using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using UnityEngine;

namespace NM.Config;




public static class ItemResLoader
{
    // 进入 Play Mode
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
// #if UNITY_EDITOR
    // 打开项目或代码重新编译
    // [UnityEditor.InitializeOnLoadMethod]
// #endif
    static void Bind()
    {
        async UniTask<(ELogLevel, string)> Func(CancellationToken ct)
        {
            var (tempList, eLogLevel, item3) = 
                await Resourcer.LoadAssetsByTagAsync<Sprite>(Const.Res.AddrTag.ItemSpriteTag, ct);
            #if UNITY_EDITOR
            // spriteList
            //     .Where(sprite => int.TryParse(sprite.name, out _))
            //     .Select(UnityEditor.AssetDatabase.GetAssetPath)
            //     .Select(UnityEditor.AssetImporter.GetAtPath)
            //     .OfType<UnityEditor.TextureImporter>().ForEach(importer =>
            //     {
            //         importer.spritePixelsPerUnit = 256;
            //         importer.filterMode = FilterMode.Point;
            //         importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
            //         importer.SaveAndReimport();
            //     });
            #endif
            spriteDic = (
                    from sprite in tempList 
                    where int.TryParse(sprite.name, out _) 
                    orderby int.Parse(sprite.name) 
                    select sprite)
                .ToDictionary(sprite => int.Parse(sprite.name), sprite => sprite);
            return (eLogLevel, item3);
        }

        Loader.OnLoad += Func;
#if UNITY_EDITOR
        Resourcer.OnReloadEditorResource += ct => Func(ct);
#endif
    }
    static Dictionary<int, Sprite> spriteDic = [];
    public static Sprite Acquire(int id)
    {
        return spriteDic.FirstOrDefault(p => p.Key == id).Value
               ?? spriteDic.FirstOrDefault().Value
               ?? throw new KeyNotFoundException($"没有找到任何一个物体的贴图.");
    }
}