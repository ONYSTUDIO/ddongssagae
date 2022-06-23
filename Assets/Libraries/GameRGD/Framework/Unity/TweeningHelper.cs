using DG.Tweening;
using UnityEngine;

public static class TweeningHelper
{
    public static Tweener DOFade(this AudioSource target, float endValue, float duration)
    {
        if (endValue < 0f)
        {
            endValue = 0f;
        }
        else if (endValue > 1f)
        {
            endValue = 1f;
        }
        return DOTween.To(() => target.volume, delegate (float x)
        {
            target.volume = x;
        }, endValue, duration).SetTarget(target);
    }
}