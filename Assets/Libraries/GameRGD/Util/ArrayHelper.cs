using System;

namespace DoubleuGames.GameRGD
{
    public static class ArrayHelper
    {
        public static T Random<T>(this T[] array)
        {
            if (array == null)
                return default;

            if (array.Length == 0)
                return default;

            if (array.Length == 1)
                return array[0];

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static void Shuffle<T>(this T[] array)
        {
            int rnd1, rnd2;
            T temp;

            for (int i = 0; i < array.Length; ++i)
            {
                rnd1 = UnityEngine.Random.Range(0, array.Length);
                rnd2 = UnityEngine.Random.Range(0, array.Length);

                temp = array[rnd1];
                array[rnd1] = array[rnd2];
                array[rnd2] = temp;
            }
        }
    }
}