using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class UIColorContainer : MonoBehaviour
    {
        [SerializeField] private List<Color> Colors = new List<Color>();

        public Color GetColor(int index)
        {
            if (Colors == null) throw new NullReferenceException(nameof(Colors));
            if (index < 0 || Colors.Count <= index) throw new ArgumentOutOfRangeException(nameof(index));

            return Colors[index];
        }

        public string GetColorToStringRGB(int index)
        {
            var _color = GetColor(index);
            return $"#{ColorUtility.ToHtmlStringRGB(_color)}";
        }

        public string GetColorToStringRGBA(int index)
        {
            var _color = GetColor(index);
            return $"#{ColorUtility.ToHtmlStringRGBA(_color)}";
        }
    }
}
