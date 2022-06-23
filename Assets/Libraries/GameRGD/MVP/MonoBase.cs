using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class MonoBase : MonoBehaviour, IBase
    {
        private CompositeDisposable m_Disposable = new CompositeDisposable();
        private Subject<IBase> m_OnDispose;
        public IObservable<IBase> OnDisposeAsObservable()
        {
            return m_OnDispose ?? (m_OnDispose = new Subject<IBase>());
        }

        /// <summary>
        /// 하위 클래스에서 재정의시, base.OnDestroy(); 반드시 호출!!!
        /// </summary>
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ClearDisposable()
        {
            m_Disposable?.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DisposeDependency()
        {
            m_OnDispose?.OnNext(this);

            ClearDisposable();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DestroyGameObject()
        {
            // Dispose();

            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
            }
        }

        #region IDisposable Support
        public bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (m_OnDispose != null)
                {
                    m_OnDispose.OnNext(this);
                    m_OnDispose.OnCompleted();
                }

                m_Disposable?.Dispose();

                IsDisposed = true;
            }
        }

        // ~MonoBase()
        // {
        //     Log();
        //     Dispose();
        // }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
        #endregion IDisposable Support

        #region ICollection<IDisposable>
        int ICollection<IDisposable>.Count => m_Disposable.Count;
        bool ICollection<IDisposable>.IsReadOnly => m_Disposable.IsReadOnly;

        void ICollection<IDisposable>.Add(IDisposable item)
        {
            if (m_Disposable.IsDisposed)
            {
                CLogger.Log("Disposed");
            }
            m_Disposable.Add(item);
        }

        void ICollection<IDisposable>.Clear()
        {
            m_Disposable.Clear();
        }

        bool ICollection<IDisposable>.Contains(IDisposable item)
        {
            return m_Disposable.Contains(item);
        }

        void ICollection<IDisposable>.CopyTo(IDisposable[] array, int arrayIndex)
        {
            m_Disposable.CopyTo(array, arrayIndex);
        }

        bool ICollection<IDisposable>.Remove(IDisposable item)
        {
            return m_Disposable.Remove(item);
        }

        IEnumerator<IDisposable> IEnumerable<IDisposable>.GetEnumerator()
        {
            return m_Disposable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Disposable.GetEnumerator();
        }
        #endregion ICollection<IDisposable>
    }
}

namespace UniRx
{
    public static partial class CustomExtensions
    {
        public static T AddTo<T>(this T disposable, DoubleuGames.GameRGD.MonoBase monoBase)
        where T : IDisposable
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            if (monoBase == null) throw new ArgumentNullException(nameof(monoBase));

            var container = monoBase as ICollection<IDisposable>;
            container.Add(disposable);

            return disposable;
        }
    }
}