using System;
using UnityEngine;
using UnityEngine.Playables;

namespace DoubleuGames.GameRGD
{
    [Serializable]
    public class TransformControlPlayable : PlayableBehaviour
    {
        public GameObject gameObject = null;
        public Vector3 offsetLocation = Vector3.zero;
        public Vector3 offsetRotation = Vector3.zero;
        public Vector3 offsetScale = Vector3.one;

        public static ScriptPlayable<TransformControlPlayable> Create(PlayableGraph graph, GameObject gameObject,
            Vector3 offsetLocation, Vector3 offsetRotation, Vector3 offsetScale)
        {
            if (gameObject == null) return ScriptPlayable<TransformControlPlayable>.Null;

            var handle = ScriptPlayable<TransformControlPlayable>.Create(graph);
            var playable = handle.GetBehaviour();
            playable.gameObject = gameObject;
            playable.offsetLocation = offsetLocation;
            playable.offsetRotation = offsetRotation;
            playable.offsetScale = offsetScale;

            gameObject.transform.rotation *= Quaternion.Euler(offsetRotation);
            gameObject.transform.position += gameObject.transform.rotation * offsetLocation;
            gameObject.transform.localScale = offsetScale;

            return handle;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (gameObject == null)
                return;

            // gameObject.transform.rotation *= Quaternion.Euler(offsetRotation);
            // gameObject.transform.position += gameObject.transform.rotation * offsetLocation;
            // gameObject.transform.rotation = Quaternion.Euler(offsetRotation);
            // gameObject.transform.position = gameObject.transform.rotation * offsetLocation;
            // gameObject.transform.localScale = offsetScale;
        }
    }
}