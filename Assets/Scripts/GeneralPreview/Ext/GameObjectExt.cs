using System.Diagnostics;
using UnityEngine;

namespace GeneralPreview;
[DebuggerStepThrough]
public static class GameObjectExt
{
    extension(GameObject self)
    {
        public void SetActiveTrue() => self.SetActive(true);
        public void SetActiveFalse() => self.SetActive(false);
        public MyOption<T> MyGetCom<T>() where T : Component
        {
            var com = self.GetComponent<T>();
            return com != null ? com : None;
        }
    }
}