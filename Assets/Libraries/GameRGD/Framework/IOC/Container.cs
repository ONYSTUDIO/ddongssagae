using System;

namespace Helen
{
    public interface IContainer : IDisposable
    {
        void Initialize();

        IContainer CreateSubContainer();

        // manage singleton
        IInstanceOption RegisterInstance<TService, TDeclare>()
            where TDeclare : TService;
        IInstanceOption RegisterInstance<TService, TDeclare>(TDeclare instance)
            where TDeclare : TService;
        IInstanceOption RegisterInstance<TService, TDeclare>(ServiceKey serviceKey)
            where TDeclare : TService;
        IInstanceOption RegisterInstance<TService, TDeclare>(TDeclare instance, ServiceKey serviceKey)
            where TDeclare : TService;
        IInstanceOption RegisterInstance<TService>();
        IInstanceOption RegisterInstance<TService>(TService instance);
        IInstanceOption RegisterInstance<TService>(ServiceKey serviceKey);
        IInstanceOption RegisterInstance<TService>(TService instance, ServiceKey serviceKey);
        IInstanceOption RegisterInstance(Type serviceType, Type declareType, object instance, ServiceKey? serviceKey);

        // manage weak reference
        IReferenceOption RegisterReference<TService, TDeclare>()
            where TDeclare : TService;
        IReferenceOption RegisterReference<TService, TDeclare>(ServiceKey serviceKey)
            where TDeclare : TService;
        IReferenceOption RegisterReference<TService>();
        IReferenceOption RegisterReference<TService>(ServiceKey serviceKey);
        IReferenceOption RegisterReference(Type serviceType, Type declareType, ServiceKey? serviceKey);

        // always new
        IRegisterationOption Register<TService, TDeclare>()
            where TDeclare : TService;
        IRegisterationOption Register<TService, TDeclare>(ServiceKey serviceKey)
            where TDeclare : TService;
        IRegisterationOption Register<TService>();
        IRegisterationOption Register<TService>(ServiceKey serviceKey);
        IRegisterationOption Register(Type serviceType, Type declareType, ServiceKey? serviceKey);

        void Unregister<TService>();
        void Unregister<TService>(ServiceKey serviceKey);
        void Unregister(Type serviceType, ServiceKey? serviceKey);

        T Resolve<T>();
        T Resolve<T>(ServiceKey serviceKey);
        object Resolve(Type serviceType, ServiceKey? serviceKey);

        void Sealed();
    }
}