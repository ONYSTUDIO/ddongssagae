using System;

public static class DelegateHelper
{
    public static void SafeInvoke<T>(this Action<T> func, T parameter)
    {
        if (IsValid<T>(func))
            func.Invoke(parameter);
    }

    public static bool IsValid<T>(this Action<T> func)
    {
        if (func == null)
            return false;

        if (func.Method.IsStatic)
            return true;

        return !func.Target.IsNullOrDefault();
    }

    public static bool IsVaild(this Action func)
    {
        if (func == null)
            return false;

        if (func.Method.IsStatic)
            return true;

        return !func.Target.IsNullOrDefault();
    }

    public static void SafeInvoke(this Action func)
    {
        if (IsVaild(func))
            func.Invoke();
    }

    public static bool IsVaild(this Delegate func)
    {
        if (func == null)
            return false;

        if (func.Method.IsStatic)
            return true;

        return !func.Target.IsNullOrDefault();
    }

    public static void SafeInvoke(this Delegate func, params object[] parameters)
    {
        if (IsVaild(func))
            func.DynamicInvoke(parameters);
    }
}