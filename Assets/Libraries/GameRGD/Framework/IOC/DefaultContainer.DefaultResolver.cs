using System;

namespace Helen
{
    public sealed partial class DefaultContainer
    {
        private class DefaultResolver : IResolver, IRegisterationOption
        {
            public Type Type { get; private set; }

            public DefaultResolver(Type type)
            {
                Type = type;
            }

            public object Resolve(IContainer container)
            {
                Assert.IsFalse(disposedValue);

                object instance = Activator.CreateInstance(Type);
                container.Inject(instance, customParams);
                return instance;
            }

            #region IDisposable

            protected bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                    disposedValue = true;
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
        }
    }
}