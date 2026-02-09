using System.Linq;
using General;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace NM.Editor
{
    public class ObservableDrawer<T> : OdinValueDrawer<MyProperty<T>> where T : struct
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var valueChild = Property.Children.FirstOrDefault(c => c.Name == MyProperty<T>.NameOfValue);
            if (valueChild != null)
            {
                valueChild.Draw(label);
            }
            else
            {
                CallNextDrawer(label);
            }
        }
    }
}