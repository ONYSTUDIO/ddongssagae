using System;
using System.Collections.Generic;

namespace Helen
{
    public sealed partial class DefaultContainer
    {
        private class DefaultFactory : IResolveFactory
        {
            public Type ServiceType
            {
                get; private set;
            }

            private readonly static ServiceKey defaultServiceKey =
                new ServiceKey(null);

            private readonly Dictionary<ServiceKey, IResolver> resolvers =
                new Dictionary<ServiceKey, IResolver>();

            public DefaultFactory(Type serviceType)
            {
                ServiceType = serviceType;
            }

            public void Initialize()
            {
                Assert.IsFalse(disposedValue);

                foreach (IResolver resolver in resolvers.Values)
                    resolver.Dispose();
                resolvers.Clear();
            }

            public object Resolve(ServiceKey? serviceKey, IContainer container)
            {
                if (resolvers.TryGetValue(serviceKey ?? defaultServiceKey, out IResolver resolver))
                    return resolver.Resolve(container);
                return null;
            }

            public IRegisterationOption Register(ServiceKey? serviceKey, Type declareType)
            {
                Assert.IsTrue(ServiceType.IsAssignableFrom(declareType));
                Assert.IsFalse(resolvers.ContainsKey(serviceKey ?? defaultServiceKey));

                DefaultResolver resolver = new DefaultResolver(declareType);
                resolvers.Add(serviceKey ?? defaultServiceKey, resolver);
                return resolver;
            }

            public IReferenceOption RegisterReference(ServiceKey? serviceKey, Type declareType)
            {
                Assert.IsTrue(ServiceType.IsAssignableFrom(declareType));
                Assert.IsFalse(resolvers.ContainsKey(serviceKey ?? defaultServiceKey));

                ReferenceResolver resolver = new ReferenceResolver(declareType);
                resolvers.Add(serviceKey ?? defaultServiceKey, resolver);
                return resolver;
            }

            public IInstanceOption RegisterInstance(ServiceKey? serviceKey, object instance, Type declareType)
            {
                Assert.IsTrue(ServiceType.IsAssignableFrom(declareType));
                Assert.IsFalse(resolvers.ContainsKey(serviceKey ?? defaultServiceKey));

                InstanceResolver resolver = new InstanceResolver(declareType, instance);
                resolvers.Add(serviceKey ?? defaultServiceKey, resolver);
                return resolver;
            }

            public bool Unregister(ServiceKey? serviceKey)
            {
                if (resolvers.TryGetValue(serviceKey ?? defaultServiceKey, out IResolver resolver))
                    resolver.Dispose();
                return resolvers.Remove(serviceKey ?? defaultServiceKey);
            }

            #region IDisposable

            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    disposedValue = true;
                    if (disposing)
                    {
                        foreach (IResolver resolver in resolvers.Values)
                            resolver.Dispose();
                        resolvers.Clear();
                    }
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion IDisposable
        }
    }
}