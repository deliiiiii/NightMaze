using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using UnityEditor;
using UnityEngine;

namespace NM.View;

public class ItemResLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#endif
    static void Bind()
    {
        async UniTask Func(CancellationToken ct)
        {
            var spriteList = await Resourcer.LoadAssetsAsyncByLabel<Sprite>(Const.Res.AddrTag.ItemSpriteTag, ct);
            spriteDic = (from sprite in spriteList where int.TryParse(sprite.name, out _) orderby int.Parse(sprite.name) select sprite).ToDictionary(sprite => int.Parse(sprite.name), sprite => sprite);
        }

        Loader.OnLoad += Func;
#if UNITY_EDITOR
        Resourcer.OnReloadEditorResource += Func;
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