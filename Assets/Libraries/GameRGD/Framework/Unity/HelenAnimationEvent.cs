using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Helen
{
    [Serializable]
    public class HelenAnimationEventGroup
    {
        public string clipPath;
        public float length;
        public HelenAnimationEvent[] helenAnimationEvents;
    }

    [Serializable]
    public class HelenAnimationEvent
    {
        public string stringParameter;
        public float floatParameter;
        public int intParameter;
        public string pathParameter;

        public string functionName;
        public float time;

        public int FunctionNameHash { get; private set; }
        public string clipName { get; private set; }

        public void InitFunctionNameHash()
        {
            FunctionNameHash = Animator.StringToHash(functionName);
        }

        public void SetClipName( string clipName )
        {
            this.clipName = clipName;
        }

        public HelenAnimationEvent()
        {

        }



#if UNITY_EDITOR
        public HelenAnimationEvent(AnimationEvent animationEvent)
        {
            time = animationEvent.time;
            functionName = animationEvent.functionName;

            stringParameter = animationEvent.stringParameter;
            floatParameter = animationEvent.floatParameter;
            intParameter = animationEvent.intParameter;

            UnityEngine.Object objectAsset = animationEvent.objectReferenceParameter;
            if (objectAsset != null)
            {
                pathParameter = UnityEditor.AssetDatabase.GetAssetPath(objectAsset).Replace("Assets/Application/Bundles/", "");
            }
        }
#endif
    }
}

