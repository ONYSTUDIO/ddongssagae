using System;
using System.Collections.Generic;
using System.Linq;

public static class CollectionsHelper
{
    public static T LastOrDefault<T>(this IList<T> list)
    {
        return list.Count <= 0 ? default : list[list.Count - 1];
    }

    public static int Length<T>(this T[] array)
    {
        return array == null ? 0 : array.Length;
    }

    public static bool ContainsAll<T>(this IEnumerable<T> enumerable, IEnumerable<T> values)
    {
        foreach (T value in values)
        {
            if (!enumerable.Contains(value))
                return false;
        }
        return true;
    }

    public static T RandOrDefault<T>(this IEnumerable<T> enumerable, T exclude)
    {
        if (enumerable == null)
            return default;

        enumerable =
            enumerable.Where(item => !item.Equals(exclude));

        int count = enumerable.Count();
        if (count == 0)
            return default;

        return enumerable.ElementAt(UnityEngine.Random.Range(0, count));
    }

    public static T RandOrDefault<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
            return default;

        int count = enumerable.Count();
        if (count == 0)
            return default;

        return enumerable.ElementAt(UnityEngine.Random.Range(0, count));
    }

    public static T RandOrDefault<T>(this T[] collection)
    {
        if (collection == null || collection.Length == 0)
            return default;
        return collection[UnityEngine.Random.Range(0, collection.Length)];
    }

    public static T RandOrDefault<T>(this IList<T> collection)
    {
        if (collection == null || collection.Count == 0)
            return default;
        return collection[UnityEngine.Random.Range(0, collection.Count)];
    }

    public static bool Contains<T>(this IEnumerable<T> enumerable, Func<T, bool> compare)
    {
        foreach (var entry in enumerable)
        {
            if (compare(entry))
                return true;
        }
        return false;
    }

    public static bool Contains<T>(this T[] array, T compareValue, IEqualityComparer<T> equalityComparer)
    {
        foreach (var entry in array)
        {
            if (equalityComparer.Equals(entry, compareValue))
                return true;
        }
        return false;
    }

    public static bool Contains<T>(this IList<T> list, Func<T, bool> compare)
    {
        return IndexOf(list, compare) >= 0;
    }

    public static T GetValueOrDefault<T>(this T[] array, int index)
    {
        if (array == null)
            return default;
        if (index < array.Length)
            return array[index];
        return default;
    }

    public static int IndexOf<T>(this T[] array, T value)
    {
        if (array == null)
            return -1;
        return Array.IndexOf<T>(array, value);
    }

    public static int IndexOf<T>(this IList<T> list, Func<T, bool> compare)
    {
        if (list == null)
            return -1;

        for (int i = 0; i < list.Count; ++i)
        {
            if (!compare(list[i]))
                continue;
            return i;
        }
        return -1;
    }

    public static bool Remove<T>(this IList<T> list, Func<T, bool> compare)
    {
        int index = IndexOf(list, compare);
        if (index >= 0)
        {
            list.RemoveAt(index);
            return true;
        }
        return false;
    }

    public static bool RemoveFast<T>(this IList<T> list, Func<T, bool> compare)
    {
        return RemoveAtFast(list, IndexOf(list, compare));
    }

    public static bool RemoveFast<T>(this IList<T> list, T item)
    {
        return RemoveAtFast(list, list.IndexOf(item));
    }

    public static bool RemoveAtFast<T>(this IList<T> list, int index)
    {
        if (index >= 0)
        {
            T temp = list[index];
            list[index] = list[list.Count - 1];
            list[list.Count - 1] = temp;

            list.RemoveAt(list.Count - 1);

            return true;
        }
        return false;
    }

    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        if (list != null)
        {
            foreach (var item in list)
                action.Invoke(item);
        }
    }
}