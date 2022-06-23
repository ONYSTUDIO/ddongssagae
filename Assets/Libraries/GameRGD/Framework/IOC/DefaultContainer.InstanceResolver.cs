using System;

namespace Helen
{
    public sealed partial class DefaultContainer
    {
        private class InstanceResolver : IResolver, IInstanceOption
        {
            public Type Type { get; private set; }

            private object instance;

            public InstanceResolver(Type type, object instance)
            {
                Type = type;

                this.instance = instance;
            }

            public object Resolve(IContainer container)
            {
                Assert.IsFalse(disposedValue);

                if (instance != null)
                    return instance;

                instance = Activator.CreateInstance(Type);
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
                        if (!doNotDisposeValue && instance is IDisposable)
                        {
                            (instance as IDisposable).Dispose();
                            instance = null;
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

            IInstanceOption IInstanceOption.DoNotDispose()
            {
                doNotDisposeValue = true;

                return this;
            }
        }
    }
}