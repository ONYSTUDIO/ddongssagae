using UnityEngine;
using UnityEngine.AddressableAssets;
using Helen;

[AddComponentMenu(UIConst.groupName + "/Raw Image" + UIConst.suffixUI, 12)]
public class UIRawImage : UnityEngine.UI.RawImage
{
    public bool IsRenderTextureArea;
    public AssetReference RenderTexturePresetPrefab;
}