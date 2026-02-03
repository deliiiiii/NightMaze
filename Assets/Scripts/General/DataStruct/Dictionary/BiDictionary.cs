using System.Collections;
using System.Collections.Generic;

public class BiDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> forward = new();
    private readonly Dictionary<TValue, TKey> backward = new();

    public TValue this[TKey key]
    {
        get => forward[key];
        set
        {
            forward[key] = value;
            backward[value] = key;
        }
    }

    public TKey this[TValue key]
    {
        get => backward[key];
        set
        {
            backward[key] = value;
            forward[value] = key;
        }
    }

    public ICollection<TKey> Keys => forward.Keys;
    public ICollection<TValue> Values => forward.Values;
    public int Count => forward.Count;
    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        forward.Add(key, value);
        backward.Add(value, key);
    }

    public void Add(TValue key, TKey value)
    {
        backward.Add(key, value);
        forward.Add(value, key);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        forward.Add(item.Key, item.Value);
        backward.Add(item.Value, item.Key);
    }

    public void Clear()
    {
        forward.Clear();
        backward.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => forward.ContainsKey(item.Key);

    public bool ContainsKey(TKey key) => forward.ContainsKey(key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (var item in forward)
            array[arrayIndex++] = item;
    }

    public void Dispose() { }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => forward.GetEnumerator();

    public bool Remove(TKey key)
    {
        if (!forward.TryGetValue(key, out var val)) return false;
        backward.Remove(val);
        return forward.Remove(key);
    }

    public bool Remove(TValue key)
    {
        if (!backward.TryGetValue(key, out var val)) return false;
        forward.Remove(val);
        return backward.Remove(key);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (!forward.TryGetValue(item.Key, out var val)) return false;
        backward.Remove(val);
        return forward.Remove(item.Key);
    }

    public bool TryGetValue(TKey key, out TValue value)
        => forward.TryGetValue(key, out value);

    public bool TryGetValue(TValue key, out TKey value)
        => backward.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => forward.GetEnumerator();
}