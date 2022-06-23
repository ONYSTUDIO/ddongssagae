using System;

namespace Helen
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Parameter )]
    public sealed class InjectAttribute : Attribute
    {
        public object Key { get; private set; }

        public InjectAttribute(object key = null)
        {
            Key = key;
        }
    }
}