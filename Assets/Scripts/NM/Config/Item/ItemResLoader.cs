using System.Collections.Generic;
using System.Linq;
using General;
using UnityEngine;

namespace NM.View;

public class ItemResLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bind()
    {
        Loader.OnLoad += async ct =>
        {
            var spriteList = await Resourcer.LoadAssetsAsyncByLabel<Sprite>(Const.Res.AddrTag.ItemSpriteTag, ct);
            spriteDic = 
                (from sprite in spriteList
                where int.TryParse(sprite.name, out _)
                orderby int.Parse(sprite.name)
                select sprite)
                .ToDictionary(sprite => int.Parse(sprite.name), sprite => sprite);
        };
    }

    static Dictionary<int, Sprite> spriteDic = [];
    public static Sprite Acquire(int id) =>
        spriteDic.FirstOrDefault(p => p.Key == id).Value
            ?? spriteDic.FirstOrDefault().Value
            ?? throw new KeyNotFoundException($"没有找到任何一个物体的贴图.");
}