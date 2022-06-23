using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu(UIConst.groupName + "/Text" + UIConst.suffixTMP, 10)]
[ExecuteAlways]
public class TMPText : TMPro.TextMeshProUGUI
{
}