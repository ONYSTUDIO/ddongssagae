using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/ColorSetData")]
public class ColorSetData : ScriptableObject
{
    public Color normalColor = new Color32(255, 255, 255, 255);
    public Color pressedColor = new Color32(200, 200, 200, 255);
    public Color disabledColor = new Color32(200, 200, 200, 128);

    public float colorMultiplier = 1.0f;
    public float fadeDuration = 0.1f;
}
