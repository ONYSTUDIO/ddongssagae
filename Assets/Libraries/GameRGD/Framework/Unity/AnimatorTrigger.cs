using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

[Serializable]
public class AnimatorTrigger
{
    public enum Type
    {
        State,
        Trigger,
    }

    [SerializeField]
#pragma warning disable CA2235 // Mark all non-serializable fields
    private Animator animator = default;
#pragma warning restore CA2235 // Mark all non-serializable fields

    [SerializeField]
    private Type type = default;

    [SerializeField]
    private string trigger = default;

    public void Play()
    {
        switch (type)
        {
            case Type.State:
                animator.Play(trigger, 0, 0);
                break;

            case Type.Trigger:
                animator.SetTrigger(trigger);
                break;
        }
    }
}