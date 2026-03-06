using System.Collections.Generic;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Data;

public class LenPlaying : MonoBehaviour
{
    [ShowInInspector] public static MyOption<GamePlaying> Playing => Root.InState<GamePlaying>();
    public void Save() => Playing.MatchA(some => Saver.Save(NameC.SlotFolder, some.PlayerName, some));
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