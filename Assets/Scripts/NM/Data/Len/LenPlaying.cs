using General;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Data;

public class LenPlaying : MonoBehaviour
{
    [ShowInInspector] public static MyOption<GamePlaying> Playing => GameRoot.GetStateOptional<GamePlaying>();
    public void Save() => Playing.MatchA(some => Saver.Save(NameC.SlotFolder, some.PlayerName, some));

    // [ShowInInspector]
    // public List<ICanAwait> DelayDo =>
    //     (from playing in Playing.ToIEnumerable()
    //         from spin in playing.GetStateOptional<PlayingSpin>().ToIEnumerable()
    //         from add in spin.DoList
    //         select add).ToList();
}