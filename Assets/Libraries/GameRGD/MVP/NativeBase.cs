using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace DoubleuGames.GameRGD
{
    public class NativeBase : IBase
    {
        private CompositeDisposable mDisposable = new CompositeDisposable();
        private Subject<IBase> mOnDispose;
        public IObservable<IBase> OnDisposeAsObservable()
        {
            return mOnDispose ?? (mOnDispose = new Subject<IBase>());
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ClearDisposable()
        {
            mDisposable?.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DisposeDependency()
        {
            mOnDispose?.OnNext(this);

            ClearDisposable();
        }

        #region IDisposable Support
        public bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (mOnDispose != null)
                {
                    mOnDispose.OnNext(this);
                    mOnDispose.OnCompleted();
                }

                mDisposable?.Dispose();

                IsDisposed = true;
            }
        }

        ~NativeBase()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
        #endregion IDisposable Support

        #region ICollection<IDisposable>
        int ICollection<IDisposable>.Count => mDisposable.Count;
        bool ICollection<IDisposable>.IsReadOnly => mDisposable.IsReadOnly;

        void ICollection<IDisposable>.Add(IDisposable item)
        {
            if (mDisposable.IsDisposed)
            {
                CLogger.Log("Disposed");
            }
            mDisposable.Add(item);
        }

        void ICollection<IDisposable>.Clear()
        {
            mDisposable.Clear();
        }

        bool ICollection<IDisposable>.Contains(IDisposable item)
        {
            return mDisposable.Contains(item);
        }

        void ICollection<IDisposable>.CopyTo(IDisposable[] array, int arrayIndex)
        {
            mDisposable.CopyTo(array, arrayIndex);
        }

        bool ICollection<IDisposable>.Remove(IDisposable item)
        {
            return mDisposable.Remove(item);
        }

        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator()
        {
            return mDisposable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mDisposable.GetEnumerator();
        }
        #endregion ICollection<IDisposable>
    }
}

namespace UniRx
{
    public static partial class CustomExtensions
    {
        public static T AddTo<T>(this T disposable, DoubleuGames.GameRGD.NativeBase nativeBase)
        where T : IDisposable
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (nativeBase == null) throw new ArgumentNullException(nameof(nativeBase));

            var container = nativeBase as ICollection<IDisposable>;
            container.Add(disposable);

            return disposable;
        }
    }
}