using System;
using UnityEngine;
public static class MathHelper
{
    public static string ToSIString(this double number, string unitSymbol = null, int groupWeight = 1000, string unitMultipliers = " kMGTPEZY", string unitDivider = " mµnpfazy")
    {
        var numberWeigth = (int)System.Math.Floor(System.Math.Log(number) / System.Math.Log(groupWeight));

        char? unitWeigthSymbol = null;
        if (numberWeigth > 0)
        {
            numberWeigth = Math.Min(numberWeigth, unitMultipliers.Length - 1);
            unitWeigthSymbol = unitMultipliers[numberWeigth];
        }
        else
        {
            numberWeigth = -Math.Min(Math.Abs(numberWeigth), unitDivider.Length - 1);
            unitWeigthSymbol = unitDivider[-numberWeigth];
        }
        number = number / Math.Pow(groupWeight, numberWeigth);

        if (string.IsNullOrEmpty(unitSymbol))
            return string.Format("{0:0.###} {1}", number, unitWeigthSymbol);
        else
            return string.Format("{0:0.###} {1}{2}", number, unitWeigthSymbol, unitSymbol);
    }

    public static long Clamp(long value, long min, long max)
    {
        if (value < min)
            return min;
        else if (value > max)
            return max;

        return value;
    }

    public static Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        //p0 - start;
        //p1 - start tangent;
        //p2 - end tangent;
        //p3 - end;

        float u = 1f - t;
        float t2 = t * t;
        float u2 = u * u;
        float u3 = u2 * u;
        float t3 = t2 * t;

        Vector3 result =
            (u3) * p0 +
            (3f * u2 * t) * p1 +
            (3f * u * t2) * p2 +
            (t3) * p3;

        return result;
    }
}