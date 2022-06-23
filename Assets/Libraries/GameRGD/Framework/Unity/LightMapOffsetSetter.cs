using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Helen
{
    public class LightMapOffsetSetter : MonoBehaviour
    {
        public void SetLightMapOffset()
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                Log.Error("Renderer is null");
                return;
            }

            renderer.sharedMaterial.SetVector("_LightMapOffset", renderer.lightmapScaleOffset);
        }
    }
}
