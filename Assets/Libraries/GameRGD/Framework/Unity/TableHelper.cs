using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Helen;
public static class TableHelper
{
    public static int RandomRangeByMinMax(this int[] array)
    {
        Assert.IsFalse(array.Length < 2);

        int min = array[0];
        int max = array[1];

        return Random.Range(min, max);
    }

    public static float RandomRangeByMinMax(this float[] array)
    {
        Assert.IsFalse(array.Length < 2);

        float min = array[0];
        float max = array[1];

        return Random.Range(min, max);
    }
}
