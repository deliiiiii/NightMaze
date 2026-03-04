using System.Collections.Generic;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Data;

public class LenPlaying : MonoBehaviour
{
    public static MyOption<GamePlaying> PlayingOp => Root.InState<GamePlaying>();
    [ShowInInspector] GamePlaying Playing => PlayingOp.Match(Rid, () => null!);

    [Button]
    public void Save() => PlayingOp.MatchA(some => Saver.Save(NameC.SlotFolder, some.PlayerName, some), NoAct);
    [ShowInInspector]
    public List<PlayingSpin.UniAction> DelayDo
    {
        get
        {
            var op = 
                from playing in PlayingOp
                from spin in playing.InState<PlayingSpin>()
                select spin.DelayAddList;
            return op.Match(Rid, () => []);
        }
    }
}