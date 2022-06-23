using System;

namespace Helen
{
    public interface IResolveFactory : IDisposable
    {
        Type ServiceType { get; }

        void Initialize();

        object Resolve(ServiceKey? serviceKey, IContainer container);
    }
}