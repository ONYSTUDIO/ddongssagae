using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Operators;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    ///////////////////////////////////////////////////////
    //BlockWhen에 걸려있는 Observable를 통제하기 위한 클래스
    ///////////////////////////////////////////////////////
    public class StreamBlocker
    {
        // OperatorObservable의 OnNext호출을 위해 인터페이스를 구현함.
        public interface _IPassNotifierReceiver
        {
            void SetPass(bool value);
        }
        // TypeID에 따라 BlockWhen를 패스시켜야 하기 때문에 Dictionary로 해당되는 필터를 저장해둠.
        private static List<int> notifyListenerFilterIDList = new List<int>();
        private static List<_IPassNotifierReceiver> notifyListenerList = new List<_IPassNotifierReceiver>();

        // <summary>
        // SetPass를 true로 주면 BlockWhen에 걸려있는 Observable이 통과되고,
        // false면 통과시키지 않는다.
        // </summary>
        private static int currentBlockFlag;
        public static void Toggle(int flagID)
        {
            int currentFlag = currentBlockFlag & flagID;
            //bool isOn = false;
            if (currentFlag > 0)
            {
                //isOn = true;
                UnBlock(flagID);
            }else
            {
                Block(flagID);
            }
        }
        public static void Block(int flagID)
        {
            currentBlockFlag = currentBlockFlag | flagID;
            UpdatePass();
            Debug.LogFormat("Blocked={0}",flagID);
        }
        public static void UnBlock(int flagID)
        {
            currentBlockFlag = currentBlockFlag & (~flagID);
            UpdatePass();
            Debug.LogFormat("UnBlock={0}",flagID);
        }
        private static void UpdatePass()
        {
            for (int i = 0 ; i < notifyListenerList.Count ; i++)
            {
                int typeID = notifyListenerFilterIDList[i];
                //어느하나라도 막혀있다면..
                if ( (currentBlockFlag & typeID) > 0)
                {
                    notifyListenerList[i].SetPass(false);
                    continue;
                }
                notifyListenerList[i].SetPass(true);
            }
        }




        // 내부 작동을 위한 클래스
        public static void _AddToListener(int flagID, _IPassNotifierReceiver listener)
        {
            notifyListenerFilterIDList.Add(flagID);
            notifyListenerList.Add(listener);
        }
        // 내부 작동을 위한 클래스
        public static void _RemoveListener(_IPassNotifierReceiver listener)
        {
            int idxVal = notifyListenerList.IndexOf(listener);
            if (idxVal == -1)
            {
                throw new System.Exception("BlockWhen Cannot find listener");
            }
            notifyListenerFilterIDList.RemoveAt(idxVal);
            notifyListenerList.RemoveAt(idxVal);
        }
        
    }

    ////////////////////////////////////////////////////////////////
    // Observable에서 BlockWhen를 접근할수 있게 해주는 익스텐션
    ////////////////////////////////////////////////////////////////
    public static partial class _CustomObservableExtension
    {
        public static IObservable<T> BlockWhen<T>(this IObservable<T> source, int type, bool disposeEventWhenBlocked = true)
        {
            return new _PassOnlyWhenNotifiedObservable<T>(source, type, disposeEventWhenBlocked);
        }
    }

    ////////////////////////////////////////////////////////////////
    // Observable에서 BlockWhen를 접근할수 있게 해주는 익스텐션
    ////////////////////////////////////////////////////////////////
    public class _PassOnlyWhenNotifiedObservable<T>:OperatorObservableBase<T>
    {
        readonly IObservable<T> source;
        int typeID;
        bool disposeEventWhenBlocked;
        public _PassOnlyWhenNotifiedObservable(IObservable<T> source, int typeID, bool disposeEventWhenBlocked)
                : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.typeID = typeID;
            this.disposeEventWhenBlocked = disposeEventWhenBlocked;
        }
        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            var created = new _PassOnlyWhenNotified(observer,cancel,disposeEventWhenBlocked);
            StreamBlocker._AddToListener(typeID,created);
            return source.Subscribe(created);
        }
        class _PassOnlyWhenNotified : OperatorObserverBase<T, T>, StreamBlocker._IPassNotifierReceiver
        {
            Queue<T> queued;
            bool isOpened;
            bool disposeEventWhenBlocked;
            public _PassOnlyWhenNotified(IObserver<T> observer, IDisposable cancel,bool disposeEventWhenBlocked)
                : base(observer, cancel)
            {
                queued = new Queue<T>();
                isOpened = true;
                this.disposeEventWhenBlocked = disposeEventWhenBlocked;
            }
            public void SetPass(bool value)
            {
                isOpened = value;
                if (isOpened == false)
                    return;
                if (queued.Count > 0)
                {
                    while(queued.Peek() != null)
                    {
                        var item = queued.Dequeue();
                        base.observer.OnNext(item);
                        if (queued.Count <= 0)
                            break;
                    }
                }
            }

            public override void OnNext(T value)
            {
                if (isOpened == true)
                {
                    base.observer.OnNext(value);
                    return;
                }
                if (disposeEventWhenBlocked == false)
                    queued.Enqueue(value);
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { 
                    StreamBlocker._RemoveListener(this);
                    Dispose();
                    }
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); }
                finally { 
                    StreamBlocker._RemoveListener(this);
                    Dispose();
                    }
            }
        }
    }
}