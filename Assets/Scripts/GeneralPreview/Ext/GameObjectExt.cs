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

        public T GetOrAddCom<T>() where T : Component => self.GetComponent<T>() ?? self.AddComponent<T>();
    }

    extension(Component self)
    {
        public void SetActiveTrue() => self.gameObject.SetActive(true);
        public void SetActiveFalse() => self.gameObject.SetActive(false);
    }
}