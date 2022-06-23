#pragma warning disable 0414

using System;
using DG.Tweening;
using UniRx;
using UniRx.Operators;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public enum UniRxTweenState
    {
        Invalid = -1,
        Init = 0,
        Begin = 1,
        Update = 2,
        End = 3
    }

    ////////////////////////////////////////////////////////////////
    // Observable에서 Tween을 접근할수 있게 해주는 익스텐션
    ////////////////////////////////////////////////////////////////
    public static partial class _CustomObservableExtension
    {
        public static IObservable<Tuple<UniRxTweenState,int>> TweenToConstDuration(this IObservable<int> source, float duration, System.Func<bool> shouldTweenPredicate = null)
        {
            if (duration <= 0)
            {
                throw new System.Exception("duration should be more than 0");
            }
            IObservable<long> converted = source.Select(value=>(long)value);
            var newObservable = new _TweenObservable(converted,_TweenObservable.UniRxTweenToType.ConstDuration,0,duration,shouldTweenPredicate);
            return newObservable.Select(value=>new Tuple<UniRxTweenState, int>(value.Item1,(int)value.Item2));
        } 
        public static IObservable<Tuple<UniRxTweenState,int>> TweenToAmount(this IObservable<int> source, int amount, System.Func<bool> shouldTweenPredicate = null)
        {
            if (amount <= 0)
            {
                throw new System.Exception("amount should be more than 0");
            }
            IObservable<long> converted = source.Select(value=>(long)value);
            var newObservable = new _TweenObservable(converted,_TweenObservable.UniRxTweenToType.DurationDependsOnAmount,amount,0,shouldTweenPredicate);
            return newObservable.Select(value=>new Tuple<UniRxTweenState, int>(value.Item1,(int)value.Item2));
        }
        public static IObservable<Tuple<UniRxTweenState,long>> TweenToConstDuration(this IObservable<long> source, float duration, System.Func<bool> shouldTweenPredicate = null)
        {
            if (duration <= 0)
            {
                throw new System.Exception("duration should be more than 0");
            }
            return new _TweenObservable(source, _TweenObservable.UniRxTweenToType.ConstDuration,0,duration,shouldTweenPredicate);
        }
        public static IObservable<Tuple<UniRxTweenState,long>> TweenToAmount(this IObservable<long> source, long amount, System.Func<bool> shouldTweenPredicate = null)
        {
            if (amount <= 0)
            {
                throw new System.Exception("amount should be more than 0");
            }
            return new _TweenObservable(source, _TweenObservable.UniRxTweenToType.DurationDependsOnAmount,amount,0,shouldTweenPredicate);
        }
    }

    ////////////////////////////////////////////////////////////////
    // Observable을 이용하여 Tween을 가능케 하는 클래스
    ////////////////////////////////////////////////////////////////
    public class _TweenObservable:OperatorObservableBase<Tuple<UniRxTweenState,long>>
    {
        public enum UniRxTweenToType
        {
            ConstDuration,  //주어진 Duration으로 Tweening
            DurationDependsOnAmount //주어진 Amount값을 기준으로 Duration을 계산하여 Tweening
        }
        readonly IObservable<long> source;
        UniRxTweenToType durationType;
        long amount;
        float duration;
        private readonly Func<bool> shouldTweenPredicate;

        public _TweenObservable(IObservable<long> source, UniRxTweenToType durationType, long amount, float duration, System.Func<bool> shouldTweenPredicate = null)
                : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.durationType = durationType;
            this.amount = amount;
            this.duration = duration;
            this.shouldTweenPredicate = shouldTweenPredicate;
        }
        protected override IDisposable SubscribeCore(IObserver<Tuple<UniRxTweenState,long>> observer, IDisposable cancel)
        {
            var created = new _Tween(observer,cancel,durationType,amount,duration,shouldTweenPredicate);
            return source.Subscribe(created);
        }
        class _Tween : OperatorObserverBase<long, Tuple<UniRxTweenState,long>>
        {
            UniRxTweenToType durationType;
            long durationAmount;
            float duration;
            private readonly Func<bool> shouldTweenPredicate;
            bool didInit;
            long currentValue;
            long targetValue;

            bool isTweening = false;
            public _Tween(IObserver<Tuple<UniRxTweenState,long>> observer, IDisposable cancel,UniRxTweenToType durationType, long amount, float duration, System.Func<bool> shouldTweenPredicate = null)
                : base(observer, cancel)
            {
                didInit = false;
                this.durationType = durationType;
                this.durationAmount = amount;
                this.duration = duration;
                this.shouldTweenPredicate = shouldTweenPredicate;
                isTweening = false;
            }
            public override void OnNext(long value)
            {
                //최초에는 트윈할때 시작값이 없기때문에, 바로 업데이트해버리고 끝내버림.
                if (didInit == false)
                {
                    base.observer.OnNext(new Tuple<UniRxTweenState, long>(UniRxTweenState.Init,value));
                    didInit = true;
                    currentValue = value;
                    targetValue = value;
                    return;
                }

                //트윈을 시작하기 전 현재 값을 Begin으로 넘긴다.
                base.observer.OnNext(new Tuple<UniRxTweenState, long>(UniRxTweenState.Begin,currentValue));

                //이전에 트윈중일수도 있기에 취소시켜버림.
                DOTween.Kill(this);
                targetValue = value;
                if (shouldTweenPredicate != null && shouldTweenPredicate() == false)
                {
                    currentValue = targetValue;
                    this.observer.OnNext(new Tuple<UniRxTweenState,long>(UniRxTweenState.End,targetValue));
                    return;
                }

                switch(durationType)
                {
                    case UniRxTweenToType.ConstDuration:
                    {
                        //Duration이 상수이기 때문에 그냥 바로 트윈해버림.
                        DOTween.To(()=>currentValue,x=>currentValue=x,targetValue,duration).OnUpdate(()=>{
                            base.observer.OnNext(new Tuple<UniRxTweenState, long>(UniRxTweenState.Update,currentValue));
                        }).OnComplete(()=>{
                            base.observer.OnNext(new Tuple<UniRxTweenState, long>(UniRxTweenState.End,targetValue));
                        });
                    }
                    break;
                    case UniRxTweenToType.DurationDependsOnAmount:
                    {
                        //변화량 = 목표값 - 현재값
                        long incAmount = targetValue - currentValue;
                        //변화량이 주어진 Amount에 비중이 얼마냐에 따라 지속시간을 산출..
                        float ratio = (float)incAmount / (float)durationAmount;
                        ratio = Mathf.Abs(ratio);
                        DOTween.To(()=>currentValue,x=>currentValue=x,targetValue,ratio).OnUpdate(()=>{
                            base.observer.OnNext(new Tuple<UniRxTweenState, long>(UniRxTweenState.Update,currentValue));
                        }).OnComplete(()=>{
                            base.observer.OnNext(new Tuple<UniRxTweenState, long>(UniRxTweenState.End,targetValue));
                        });
                    }
                    break;
                }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { 
                    DOTween.Kill(this);
                    Dispose();
                    
                    }
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); }
                finally { 
                    DOTween.Kill(this);
                    Dispose();
                    }
            }
        }
    }
}