using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine.Events;


[Serializable]
public class MyList<T>(IEnumerable<T> ie)
    : List<T>(ie)
{
    [JsonConstructor]
    public MyList() : this([]) { }
    public event Action<T>? OnAdd;
    public event Action<T>? OnRemove;
    public event Action? OnClear;
    
    /// Please Call MyAdd() instead
    [Obsolete]
    public new void Add(T item)
    {
        MyDebug.LogError("Please Call MyAdd() instead");
    }

    /// Please Call MyInsert() instead
    [Obsolete]
    public new void Insert(int index, T item)
    {
        MyDebug.LogError("Please Call MyInsert() instead");
    }
    
    /// Please Call MyAddRange() instead
    [Obsolete]
    public new void AddRange(IEnumerable<T> collection)
    {
        MyDebug.LogError("Please Call MyAddRange() instead");
    }
    
    /// Please Call MyRemove() instead
    [Obsolete]
    public new bool Remove(T item)
    {
        MyDebug.LogError("Please Call MyRemove() instead");
        return false;
    }
    
    /// Please Call MyClear() instead
    [Obsolete]
    public new void Clear()
    {
        MyDebug.LogError("Please Call MyClear() instead");
    }
    
    /// Please Call MyRemoveAt() instead
    [Obsolete]
    public new void RemoveAt(int index)
    {
        MyDebug.LogError("Please Call MyRemoveAt() instead");
    }
    
    /// Please Call MyRemoveAll() instead
    [Obsolete]
    public new int RemoveAll(Predicate<T> match)
    {
        MyDebug.LogError("Please Call MyRemoveAll() instead");
        return 0;
    }
    
    public void MyAdd(T item)
    {
        base.Add(item);
        OnAdd?.Invoke(item);
    }
    public void MyInsert(int index, T item)
    {
        base.Insert(index, item);
        OnAdd?.Invoke(item);
    }
    public void MyAddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
            MyAdd(item);
    }
    public void MyRemove(T item)
    {
        base.Remove(item);
        OnRemove?.Invoke(item);
    }
    public void MyRemoveAt(int index)
    {
        var item = this[index];
        base.RemoveAt(index);
        OnRemove?.Invoke(item);
    }

    public void MyRemoveAll(Predicate<T> match)
    {
        var allRemoved = this.Where(x => match(x));
        allRemoved.ForEach(MyRemove);
    }
    public void MyClear()
    {
        while (Count > 0)
        {
            MyRemoveAt(Count - 1);
        }
        OnClear?.Invoke();
    }
}