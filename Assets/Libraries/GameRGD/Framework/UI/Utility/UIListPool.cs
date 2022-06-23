using System.Collections.Generic;

// UnityEngine.UI.ListPool
public static class UIListPool<T>
{
    // Object pool to avoid allocations.
    private static readonly UIObjectPool<List<T>> s_ListPool = new UIObjectPool<List<T>>(null, Clear);

    private static void Clear(List<T> l)
    {
        l.Clear();
    }

    public static List<T> Get()
    {
        return s_ListPool.Get();
    }

    public static void Release(List<T> toRelease)
    {
        s_ListPool.Release(toRelease);
    }
}