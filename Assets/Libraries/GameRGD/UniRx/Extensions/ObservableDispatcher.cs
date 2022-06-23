using System;
using System.Collections.Generic;

namespace UniRx
{
    public class ObservableDispatcher
    {
        private interface IInvoker
        {
            void Invoke(object value);
        }

        private class Invoker<T> : IInvoker
        {
            private readonly Subject<T> subject = new Subject<T>();

            public void Invoke(object value)
            {
                Type type = typeof(T);
                if (type.IsAssignableFrom(value.GetType().UnderlyingSystemType))
                    subject.OnNext((T)value);
            }

            public IObservable<T> AsObservable()
            {
                return ((IObservable<T>)subject).AsObservable();
            }
        }

        private readonly Dictionary<Type, IInvoker> invokers =
            new Dictionary<Type, IInvoker>();

        public void Publish(object value)
        {
            Type type = value.GetType().UnderlyingSystemType;
            if (invokers.TryGetValue(type, out IInvoker invoker))
                invoker.Invoke(value);
        }

        public IObservable<T> Receive<T>()
        {
            Type type = typeof(T);
            if (!invokers.TryGetValue(type, out IInvoker invoker))
            {
                invoker = new Invoker<T>();
                invokers.Add(type, invoker);
            }
            return ((Invoker<T>)invoker).AsObservable();
        }
    }

    public class ObservableDispatcher<TKey, TValue>
    {
        private class Invoker<T>
        {
            private readonly Subject<T> subject = new Subject<T>();

            public void Invoke(object value)
            {
                subject.OnNext((T)value);
            }

            public IObservable<T> AsObservable()
            {
                return ((IObservable<T>)subject).AsObservable();
            }
        }

        private readonly Dictionary<TKey, Invoker<TValue>> invokers =
            new Dictionary<TKey, Invoker<TValue>>();

        public void Publish(TKey key, TValue value)
        {
            if (invokers.TryGetValue(key, out Invoker<TValue> invoker))
                invoker.Invoke(value);
        }

        public IObservable<TValue> Receive(TKey key)
        {
            if (!invokers.TryGetValue(key, out Invoker<TValue> invoker))
            {
                invoker = new Invoker<TValue>();
                invokers.Add(key, invoker);
            }
            return invoker.AsObservable();
        }
    }
}