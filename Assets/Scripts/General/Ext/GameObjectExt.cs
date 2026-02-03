using UnityEngine;

public static class GameObjectExt
{
    public static T GetOrAddComponent<T>(this GameObject self) where T : Component
    {
        return self.GetComponent<T>() ?? self.AddComponent<T>();
    }
    
    public static T GetOrAddComponent<T>(this Transform self) where T : Component
    {
        return self.GetComponent<T>() ?? self.gameObject.AddComponent<T>();
    }

    public static GameObject ClearChildren(this GameObject self)
    {
        for(int i = self.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(self.transform.GetChild(i).gameObject);
        }
        return self;
    }
    
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
}