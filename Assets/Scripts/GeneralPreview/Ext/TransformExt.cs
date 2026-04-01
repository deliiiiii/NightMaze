using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeneralPreview;

public static class TransformExt
{
    extension(Transform self)
    {
        public IEnumerable<Transform> GetChildren()
        {
            for(int i = 0; i < self.childCount; i++)
            {
                yield return self.GetChild(i);
            }
        }
        public Transform ClearChildren()
        {
            for(int i = self.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(self.transform.GetChild(i).gameObject);
            }
            return self;
        }
        public Transform ClearActiveChildren()
        {
            for(int i = self.transform.childCount - 1; i >= 0; i--)
            {
                if(self.transform.GetChild(i).gameObject.activeSelf)
                    Object.Destroy(self.transform.GetChild(i).gameObject);
            }
            return self;
        }
        public void DisableAllChildren()
        {
            for(int i = self.childCount - 1; i >= 0; i--)
            {
                self.GetChild(i).gameObject.SetActive(false);
            }
        }
        public Transform DestroyActiveChildren()
        {
            for(int i = 0; i < self.childCount; i++)
            {
                if (!self.GetChild(i).gameObject.activeSelf)
                    continue;
                GameObject.Destroy(self.GetChild(i).gameObject);
            }
            return self;
        }
        public Transform DestroyChild(Predicate<Transform> filter)
        {
            for(int i = 0; i < self.childCount; i++)
            {
                var child = self.GetChild(i);
                if (!filter(child))
                    continue;
                GameObject.Destroy(child.gameObject);
            }
            return self;
        }
    }
}