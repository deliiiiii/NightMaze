using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class LenPlaying : MonoBehaviour
{
    [ShowInInspector][GUIColor(1f,1f,1f)]
    [MultiLineProperty(10)][LabelWidth(10)]
    public string DelayDo => string.Join("\n\n",
        from play in GamePlayData.ToIEnumerable()
        from todo in play.ToDoList
        select Bus.FormatRecordDetails(todo.ToString()));
    
    [ShowInInspector][GUIColor(1f,1f,1f)]
    [MultiLineProperty(1000)][LabelWidth(10)]
    public string DelayDo2 => string.Join("\n\n",
        from spin in PlaySpinData.ToIEnumerable()
        from todo in spin.ToDoList
        select Bus.FormatRecordDetails(todo.ToString()));
}