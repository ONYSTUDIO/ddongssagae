using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu(UIConst.groupName + "/DissolveImage" + UIConst.suffixUI, 11)]
public class UIDissolveImage : UIImage
{
    public float DissolveProcess = 1.0f;

    protected override void UpdateMaterial()
    {
        base.UpdateMaterial();

        if (material.HasProperty("_DissolveProgress"))
            material.SetFloat("_DissolveProgress", DissolveProcess);
    }
}
