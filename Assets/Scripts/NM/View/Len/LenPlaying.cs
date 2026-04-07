using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class LenPlaying : MonoBehaviour
{
    [ShowInInspector] public static MyOption<GamePlaying> Playing => GameRoot.GetStateOptional<GamePlaying>();
    public void Save() => Playing.MatchA(some => Saver.Save(NameC.SlotFolder, some.PlayerName, some));

    [ShowInInspector]
    public List<PlaySpin.IUniAction> DelayDo =>
        (from playing in Playing.ToIEnumerable()
            from spin in playing.GetStateOptional<PlaySpin>().ToIEnumerable()
            from add in spin.ToDoList
            select add).ToList();
}