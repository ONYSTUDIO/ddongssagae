using System;
using System.Collections.Generic;

/// <summary>
/// 오픈 소스를 활용해보고자 했지만 오버헤드가 클 것 같음.
/// 다른 라이브러리를 참조하면서 성능 및 기능을 개선해 가고자 함.
/// </summary>

namespace Helen
{
    public sealed partial class DefaultContainer : IContainer, IDisposable
    {
        private readonly DefaultContainer parent;

        private readonly Dictionary<Type, IResolveFactory> resolveFactories =
            new Dictionary<Type, IResolveFactory>();

        public DefaultContainer()
            : this(null)
        {
        }

        private DefaultContainer(DefaultContainer parent)
        {
            this.parent = parent;
        }

        private bool sealedValue = true;

        public void Sealed()
        {
            sealedValue = true;
        }

        public void Initialize()
        {
            Assert.IsFalse(disposedValue);

            foreach (IResolveFactory factory in resolveFactories.Values)
                factory.Initialize();

            sealedValue = false;

            RegisterInstance<IContainer>(this).DoNotDispose();
        }

        public IContainer CreateSubContainer()
        {
            return new DefaultContainer(this);
        }

        public IInstanceOption RegisterInstance<TService, TDeclare>()
            where TDeclare : TService
        {
            return RegisterInstance(typeof(TService), typeof(TDeclare), null, null);
        }

        public IInstanceOption RegisterInstance<TService, TDeclare>(TDeclare instance)
            where TDeclare : TService
        {
            return RegisterInstance(typeof(TService), typeof(TDeclare), instance, null);
        }

        public IInstanceOption RegisterInstance<TService, TDeclare>(ServiceKey serviceKey)
            where TDeclare : TService
        {
            return RegisterInstance(typeof(TService), typeof(TDeclare), null, serviceKey);
        }

        public IInstanceOption RegisterInstance<TService, TDeclare>(TDeclare instance, ServiceKey serviceKey)
            where TDeclare : TService
        {
            return RegisterInstance(typeof(TService), typeof(TDeclare), instance, serviceKey);
        }

        public IInstanceOption RegisterInstance<TService>()
        {
            return RegisterInstance(typeof(TService), typeof(TService), null, null);
        }

        public IInstanceOption RegisterInstance<TService>(TService instance)
        {
            return RegisterInstance(typeof(TService), typeof(TService), instance, null);
        }

        public IInstanceOption RegisterInstance<TService>(ServiceKey serviceKey)
        {
            return RegisterInstance(typeof(TService), typeof(TService), null, serviceKey);
        }

        public IInstanceOption RegisterInstance<TService>(TService instance, ServiceKey serviceKey)
        {
            return RegisterInstance(typeof(TService), typeof(TService), instance, serviceKey);
        }

        public IInstanceOption RegisterInstance(
            Type serviceType, Type declareType, object instance, ServiceKey? serviceKey)
        {
            Assert.IsFalse(sealedValue);

            if (!resolveFactories.TryGetValue(serviceType, out IResolveFactory factory))
            {
                factory = new DefaultFactory(serviceType);
                resolveFactories.Add(serviceType, factory);
            }

            if (factory is DefaultFactory)
                return (factory as DefaultFactory).RegisterInstance(serviceKey, instance, declareType);

            Assert.Fail();
            return null;
        }

        public IReferenceOption RegisterReference<TService, TDeclare>()
            where TDeclare : TService
        {
            return RegisterReference(typeof(TService), typeof(TDeclare), null);
        }

        public IReferenceOption RegisterReference<TService, TDeclare>(ServiceKey serviceKey)
            where TDeclare : TService
        {
            return RegisterReference(typeof(TService), typeof(TDeclare), serviceKey);
        }

        public IReferenceOption RegisterReference<TService>()
        {
            return RegisterReference(typeof(TService), typeof(TService), null);
        }

        public IReferenceOption RegisterReference<TService>(ServiceKey serviceKey)
        {
            return RegisterReference(typeof(TService), typeof(TService), serviceKey);
        }

        public IReferenceOption RegisterReference(
            Type serviceType, Type declareType, ServiceKey? serviceKey)
        {
            Assert.IsFalse(sealedValue);

            if (!resolveFactories.TryGetValue(serviceType, out IResolveFactory factory))
            {
                factory = new DefaultFactory(serviceType);
                resolveFactories.Add(serviceType, factory);
            }

            if (factory is DefaultFactory)
                return (factory as DefaultFactory).RegisterReference(serviceKey, declareType);

            Assert.Fail();
            return null;
        }

        public IRegisterationOption Register<TService, TDeclare>()
            where TDeclare : TService
        {
            return Register(typeof(TService), typeof(TDeclare), null);
        }

        public IRegisterationOption Register<TService, TDeclare>(ServiceKey serviceKey)
            where TDeclare : TService
        {
            return Register(typeof(TService), typeof(TDeclare), serviceKey);
        }

        public IRegisterationOption Register<TService>()
        {
            return Register(typeof(TService), typeof(TService), null);
        }

        public IRegisterationOption Register<TService>(ServiceKey serviceKey)
        {
            return Register(typeof(TService), typeof(TService), serviceKey);
        }

        public IRegisterationOption Register(
            Type serviceType, Type declareType, ServiceKey? serviceKey)
        {
            Assert.IsFalse(sealedValue);

            if (!resolveFactories.TryGetValue(serviceType, out IResolveFactory factory))
            {
                factory = new DefaultFactory(serviceType);
                resolveFactories.Add(serviceType, factory);
            }

            if (factory is DefaultFactory)
                return (factory as DefaultFactory).Register(serviceKey, declareType);

            Assert.Fail();
            return null;
        }

        public void Unregister<TService>()
        {
            Unregister(typeof(TService), null);
        }

        public void Unregister<TService>(ServiceKey serviceKey)
        {
            Unregister(typeof(TService), serviceKey);
        }

        public void Unregister(Type serviceType, ServiceKey? serviceKey)
        {
            Assert.IsFalse(sealedValue);

            if (resolveFactories.TryGetValue(serviceType, out IResolveFactory factory))
            {
                if (factory is DefaultFactory)
                    (factory as DefaultFactory).Unregister(serviceKey);
                else
                    Assert.Fail();
            }
        }

        public T Resolve<T>()
        {
            return (T)InternalResolve(typeof(T), null, null);
        }

        public T Resolve<T>(ServiceKey serviceKey)
        {
            return (T)InternalResolve(typeof(T), serviceKey, null);
        }

        public object Resolve(Type type, ServiceKey? serviceKey)
        {
            return InternalResolve(type, serviceKey, null);
        }

        private object InternalResolve(Type type, ServiceKey? serviceKey, IContainer container)
        {
            if (resolveFactories.TryGetValue(type, out var factory))
            {
                object instance = factory.Resolve(serviceKey, container ?? this);
                if (instance != null)
                    return instance;
            }

            if (parent != null)
                return parent.InternalResolve(type, serviceKey, container ?? this);

            return null;
        }

        #region IDisposable

        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                if (disposing)
                {
                    foreach (IResolveFactory factory in resolveFactories.Values)
                        factory.Dispose();
                    sealedValue = true;
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