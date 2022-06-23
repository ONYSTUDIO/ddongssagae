using System;

namespace Helen
{
    public sealed partial class DefaultContainer
    {
        private interface IResolver : IDisposable
        {
            Type Type { get; }

            object Resolve(IContainer container);
        }
    }
}