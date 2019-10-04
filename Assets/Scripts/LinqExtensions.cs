using System;
using System.Collections.Generic;

public static class LinqExtensions
{
    public static T FirstOrFallback<T>(this IEnumerable<T> items, T fallback)
    {
        foreach (var item in items)
        {
            return item;
        }
        return fallback;
    }

    public static T ArgMax<T>(this IEnumerable<T> items, Func<T, float> score)
    {
        T maxItem = default(T);
        float maxScore = float.MinValue;
        foreach (var item in items)
        {
            var s = score(item);
            if (s > maxScore)
            {
                maxScore = s;
                maxItem = item;
            }
        }
        return maxItem;
    }

    public static T ArgMaxTie<T>(this IEnumerable<T> items, Func<T, int> score)
    {
        int numTied = 0;
        T maxItem = default(T);
        int maxScore = int.MinValue;
        foreach (var item in items)
        {
            var s = score(item);
            if (s > maxScore)
            {
                maxScore = s;
                maxItem = item;
                numTied = 1;
            }
            else if (s == maxScore)
            {
                numTied++;
                if (UnityEngine.Random.value < (1f / numTied))
                {
                    maxItem = item;
                }
            }
        }
        return maxItem;
    }
}
