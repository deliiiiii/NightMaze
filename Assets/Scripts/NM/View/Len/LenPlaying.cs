using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class LenPlaying : MonoBehaviour
{
    // [ShowInInspector, ListDrawerSettings(DefaultExpandedState = true)]
    // public List<string> DelayDo =>(
            // from spin in PlaySpinData.ToIEnumerable()
            // from add in spin.ToDoList
            // select add.ToString()).ToList();
    
    
    [ShowInInspector][GUIColor(1f,1f,1f)]
    [MultiLineProperty(Lines = 20)]
    public string DelayDo2 => string.Join("\n\n",
        from spin in PlaySpinData.ToIEnumerable()
        from add in spin.ToDoList
        select Bus.FormatRecordDetails(add.ToString()));
}