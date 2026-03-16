using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;

public static class ListExt
{
    // public static int RandomIndexWeighted(this List<int> list, Random seed = null)
    //     => list.IndexOf(list.RandomItemWeighted(weightFunc: x => x, filter: null, seed: seed));
    
    extension<T>(List<T> self)
    {
        public MyOption<T> RandomItemOptional(
            List<int> weightList,
            Func<T, bool>? filter = null,
            Random? seed = null)
            => self.RandomItemWeightedOptional(x => weightList[self.IndexOf(x)], filter, seed);

        public MyOption<T> RandomItemOptional(
            Func<T, int>? weightFunc = null, 
            Func<T, bool>? filter = null,
            Random? seed = null)
        {
            filter ??= _ => true;
            if (weightFunc != null)
                return self.RandomItemWeightedOptional(weightFunc, filter, seed);
            var fList = self.Where(filter).ToList();
            if(fList.Count == 0)
                return None;
            return fList[seed?.Next(0, fList.Count) ?? UnityEngine.Random.Range(0, fList.Count)];
        }

        MyOption<T> RandomItemWeightedOptional(
            Func<T, int> weightFunc,
            Func<T, bool>? filter,
            Random? seed = null)
        {
            filter ??= _ => true;
            var fList = self.Where(filter).ToList();
            if (fList.Count == 0)
                return None;
    
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

        public List<T> ShuffleTo(Random? seed = null)
        {
            var shuffledList = self.ToList();
            var n = shuffledList.Count;
            seed ??= new Random();
            while (n > 1)
            {
                var k = seed.Next(n--);
                (shuffledList[n], shuffledList[k]) = (shuffledList[k], shuffledList[n]);
            }
            return shuffledList;
        }
    }

    extension<T>(ImmutableList<T> self)
    {
        public List<T> ShuffleTo(Random? seed = null)
        {
            var shuffledList = self.ToList();
            var n = shuffledList.Count;
            seed ??= new Random();
            while (n > 1)
            {
                var k = seed.Next(n--);
                (shuffledList[n], shuffledList[k]) = (shuffledList[k], shuffledList[n]);
            }
            return shuffledList;
        }
    }
    
    // [CanBeNull]
    // public static LinkedListNode<T> At<T>(this LinkedList<T> list, int index)
    // {
    //     // if ((list?.Count ?? 0) == 0)
    //     //     throw new NullReferenceException();
    //     // if (index < 0 || index >= list.Count)
    //     //     throw new IndexOutOfRangeException();
    //     var current = list.First;
    //     for (int i = 0; i < index; i++)
    //         current = current?.Next;
    //     return current;
    // }
}