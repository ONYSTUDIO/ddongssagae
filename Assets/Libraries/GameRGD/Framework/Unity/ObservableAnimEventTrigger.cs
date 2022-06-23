using System;
using UniRx;
using UnityEngine;

public class ObservableAnimEventTrigger : MonoBehaviour
{
    private Subject<string> onAnimEvent = new Subject<string>();

    public IObservable<string> OnAnimEventAsObservable()
    {
        return onAnimEvent;
    }

    private void AnimEvent(string eventName)
    {
        onAnimEvent.OnNext(eventName);
    }
}

