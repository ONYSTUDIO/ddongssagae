using System;

namespace Helen
{
    public sealed partial class DefaultContainer
    {
        private class ReferenceResolver : IResolver, IReferenceOption
        {
            public Type Type { get; private set; }

            private WeakReference<object> reference;

            public ReferenceResolver(Type type)
            {
                Type = type;
            }

            public object Resolve(IContainer container)
            {
                Assert.IsFalse(disposedValue);

                if (reference == null)
                    reference = new WeakReference<object>(null);

                if (reference.TryGetTarget(out object instance))
                    return instance;

                instance = Activator.CreateInstance(Type);
                reference.SetTarget(instance);
                container.Inject(instance, customParams);
                return instance;
            }

            #region IDisposable

            protected bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    disposedValue = true;
                    if (disposing)
                    {
                        if (reference != null &&
                            reference.TryGetTarget(out object instance))
                        {
                            if (!doNotDisposeValue && instance is IDisposable)
                                (instance as IDisposable).Dispose();
                            reference.SetTarget(null);
                            reference = null;
                        }
                    }
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion IDisposable

            private object[] customParams;

            IRegisterationOption IRegisterationOption.WithParams(params object[] customParams)
            {
                this.customParams = customParams;

                return this;
            }

            private bool doNotDisposeValue = false;

            IReferenceOption IReferenceOption.DoNotDispose()
            {
                doNotDisposeValue = true;

                return this;
            }
        }
    }
}