using System;

public static class ObjectHelper
{
    public static bool IsNullOrDefault(this object target)
    {
        if (target == null)
            return true;

        if (target.Equals(default))
            return true;

        // deal with non-null nullables
        Type methodType = typeof(object);
        if (Nullable.GetUnderlyingType(methodType) != null)
            return false;

        // deal with boxed value types
        Type argumentType = target.GetType();
        if (argumentType.IsValueType && argumentType != methodType)
            return Activator.CreateInstance(target.GetType()).Equals(target);

        return false;
    }

    public static bool IsNullOrDefault(this string target)
    {
        return string.IsNullOrEmpty(target);
    }

    public static bool IsNullOrDefault<T>(this T target)
    {
        if (target is UnityEngine.Object)
            return !(target as UnityEngine.Object);

        if (target == null)
            return true;

        return Equals(target, default(T));
    }
}