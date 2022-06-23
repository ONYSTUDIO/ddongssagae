using System;

namespace Helen
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class StartupAttribute : Attribute
    {
    }
}