using System;

namespace UniRx
{
    public interface ICompositeDisposable
    {
        void Add(IDisposable item);
        bool Remove(IDisposable item);
        void Clear();
    }

    public static class UniRxHelper
    {
        public static void AddTo(this IDisposable disposable, ICompositeDisposable container)
        {
            container.Add(disposable);
        }
    }
}