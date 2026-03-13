using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace General
{
    public static class TransformExt
    {
        public static Transform ClearChildren(this Transform self)
        {
            for(int i = self.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(self.transform.GetChild(i).gameObject);
            }
            return self;
        }
    
        public static Transform ClearActiveChildren(this Transform self)
        {
            for(int i = self.transform.childCount - 1; i >= 0; i--)
            {
                if(self.transform.GetChild(i).gameObject.activeSelf)
                    Object.Destroy(self.transform.GetChild(i).gameObject);
            }
            return self;
        }
    
    

        public static void DisableAllChildren(this Transform self)
        {
            for(int i = self.childCount - 1; i >= 0; i--)
            {
                self.GetChild(i).gameObject.SetActive(false);
            }
        }
        
        public static Transform DestroyActiveChildren(this Transform t)
        {
            for(int i = 0; i < t.childCount; i++)
            {
                if (!t.GetChild(i).gameObject.activeSelf)
                    continue;
                GameObject.Destroy(t.GetChild(i).gameObject);
            }
            return t;
        }

        public static Transform DestroyChild(this Transform t, Predicate<Transform> filter)
        {
            for(int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                if (!filter(child))
                    continue;
                GameObject.Destroy(child.gameObject);
            }
            return t;
        }
    }
}