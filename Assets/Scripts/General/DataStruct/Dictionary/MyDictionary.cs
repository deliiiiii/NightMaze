using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

[Serializable]
public class MyDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    [CanBeNull] public event Action<TValue> OnAdd;
    [CanBeNull] public event Func<TValue, Task> OnAddAsync;
    [CanBeNull] public event Action<TValue> OnRemove;

    public new async Task Add(TKey key, TValue value)
    {
        base.Add(key, value);
        OnAdd?.Invoke(value);
        if(OnAddAsync != null)
            await OnAddAsync.Invoke(value);
    }

    public new bool Remove(TKey key)
    {
        if (ContainsKey(key))
        {
            OnRemove?.Invoke(base[key]);
            base.Remove(key);
            return true;
        }
        return false;
    }
    
    public new void Clear()
    {
        foreach (var value in Values)
        {
            OnRemove?.Invoke(value);
        }
        base.Clear();
    }
}