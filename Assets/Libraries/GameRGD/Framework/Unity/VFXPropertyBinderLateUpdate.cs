using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEngine.VFX.Utility
{
    public class VFXPropertyBinderLateUpdate : MonoBehaviour
    {
        [System.Serializable]
        public struct PropertyPositionBinder
        {
            public string propertyName;
            public Transform tf;
        }

        [SerializeField]
        List<PropertyPositionBinder> positionBinderList = new List<PropertyPositionBinder>();

        VFX.VisualEffect vfx;

        private void Awake()
        {
            vfx = GetComponent < VisualEffect>();
        }


        private void LateUpdate()
        {
            foreach( var binder in positionBinderList )
            {
                if (binder.tf == null)
                    continue;

                var pos = binder.tf.position;
                vfx.SetVector3(binder.propertyName, pos);
            }
        }
    }
}