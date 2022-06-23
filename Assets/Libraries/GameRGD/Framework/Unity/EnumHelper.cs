using System;
using System.Collections.Generic;

public static partial class EnumHelper
{
    public static int GetCount<T>()
        where T : struct, Enum
    {
        return Enum.GetValues(typeof(T)).Length;
    }

    public static IEnumerable<T> GetValues<T>()
        where T : struct, Enum
    {
        foreach (object enum_object in Enum.GetValues(typeof(T)))
        {
            yield return (T)enum_object;
        }
    }

    public static string ToName<T>(this T value)
        where T : struct, Enum
    {
        return Enum.GetName(typeof(T), value);
    }

    public static T Parse<T>(string value)
        where T : struct, Enum
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static T ToObject<T>(int value)
        where T : struct, Enum
    {
        return (T)Enum.ToObject(typeof(T), value);
    }

    public static bool IsDefined<T>(object obj)
        where T : struct, Enum
    {
        return Enum.IsDefined(typeof(T), obj);
    }
}