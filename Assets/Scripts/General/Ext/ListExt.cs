using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

public static class ListExt
{
    public static int RandomIndexWeighted(this List<int> list, Random seed = null)
        => list.IndexOf(list.RandomItemWeighted(weightFunc: x => x, filter: null, seed: seed));

    public static T RandomItem<T>(this List<T> list
        , List<int> weightList
        , [CanBeNull] Func<T, bool> filter = null
        , [CanBeNull] Random seed = null)
        => RandomItemWeighted(list, x => weightList[list.IndexOf(x)], filter, seed);
    
    public static T RandomItem<T>(this List<T> list
        , [CanBeNull] Func<T, int> weightFunc = null
        , [CanBeNull] Func<T, bool> filter = null
        , [CanBeNull] Random seed = null)
    {
        filter ??= _ => true;
        if (weightFunc != null)
            return RandomItemWeighted(list, weightFunc, filter, seed);
        var fList = list.Where(filter).ToList();
        if(fList.Count == 0)
            return default;
        return fList[seed?.Next(0, fList.Count) ?? UnityEngine.Random.Range(0, fList.Count)];
    }
    static T RandomItemWeighted<T>(this List<T> list, Func<T, int> weightFunc, [CanBeNull] Func<T, bool> filter
        , [CanBeNull] Random seed = null)
    {
        filter ??= _ => true;
        var fList = list.Where(filter).ToList();
        if (fList.Count == 0)
            return default;

        var weights = fList.Select(weightFunc).ToList();
        var totalWeight = weights.Sum();
        var randomValue = (seed?.Next(0, totalWeight) ?? UnityEngine.Random.Range(0, totalWeight)) + 1;
        var curWeight = 0;

        for (var i = 0; i < fList.Count; i++)
        {
            curWeight += weights[i];
            if (curWeight >= randomValue)
            {
                return fList[i];
            }
            
        }

        return fList[^1];
    }

    [CanBeNull]
    public static LinkedListNode<T> At<T>(this LinkedList<T> list, int index)
    {
        // if ((list?.Count ?? 0) == 0)
        //     throw new NullReferenceException();
        // if (index < 0 || index >= list.Count)
        //     throw new IndexOutOfRangeException();
        var current = list.First;
        for (int i = 0; i < index; i++)
            current = current?.Next;
        return current;
    }
    
    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        var rnd = new Random();
        while (n > 1)
        {
            var k = rnd.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
