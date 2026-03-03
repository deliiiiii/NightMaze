using System.Collections.Generic;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Data;

public class LenPlaying : MonoBehaviour
{
    public static MyOption<GamePlaying> Playing => Root.InState<GamePlaying>();
    [ShowInInspector] GamePlaying playing => Playing.Match(Rid, () => null!);
    [ShowInInspector]
    public List<PlayingSpin.UniAction> DelayDo
    {
        get
        {
            var op = 
                from playing in Playing
                from spin in playing.InState<PlayingSpin>()
                select spin.DelayAddList;
            return op.Match(Rid, () => []);
        }
    }
}