using System;
using UniRx;

namespace Helen
{
    public abstract class Model : ICompositeDisposable, IDisposable
    {
        public Guid Guid
        {
            get;
        } = Guid.NewGuid();

        #region ICompositeDisposable

        private readonly CompositeDisposable compositeDisposable =
            new CompositeDisposable();

        void ICompositeDisposable.Add(IDisposable item)
        {
            Assert.IsFalse(disposedValue);

            compositeDisposable.Add(item);
        }

        bool ICompositeDisposable.Remove(IDisposable item)
        {
            Assert.IsFalse(disposedValue);

            return compositeDisposable.Remove(item);
        }

        void ICompositeDisposable.Clear()
        {
            Assert.IsFalse(disposedValue);

            compositeDisposable.Clear();
        }

        #endregion ICompositeDisposable

        #region IDisposable

        
        protected bool disposedValue = false;
        public bool IsDisposed => disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                if (disposing)
                    OnDispose();
            }
        }

        protected virtual void OnDispose()
        {
            compositeDisposable.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable
    }

    public static class ModelExtensions
    {
        public static T InjectBy<T>(this T model, IContainer container)
            where T : Model
        {
            return container.Inject(model);
        }

        public static void AddTo(this IDisposable disposable, Model model)
        {
            (model as ICompositeDisposable).Add(disposable);
        }
    }
}