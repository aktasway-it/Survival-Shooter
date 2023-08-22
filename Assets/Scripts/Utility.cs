using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

public static class Utility
{
    public static IList<T> ShuffleList<T>(this IList<T> list, int seed)
    {
        Random random = new Random(seed);
        for (int i = 0; i < list.Count - 1; i++)
        {
            int randomIndex = random.Next(i, list.Count);
            T temp = list[randomIndex];
            list[randomIndex] = list[i];
            list[i] = temp;
        }

        return list;
    }
}
