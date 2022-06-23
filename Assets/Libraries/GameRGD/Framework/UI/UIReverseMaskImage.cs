using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu(UIConst.groupName + "/ReverseMaskImage" + UIConst.suffixUI, 11)]
public class UIReverseMaskImage : UnityEngine.UI.Image
{
    public override Material materialForRendering
    {
        get
        {
            Material result = Instantiate( base.materialForRendering );
            result.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return result;
        }
    }
}
