#pragma warning disable 0618

using System.Text;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public partial class CMiscFunc
    {
        public static StringBuilder mStringBuilder = new StringBuilder(1024);

        // Color => #FF00AA
        public static string GetColorToStringRGB(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        // Color => #FF00AA
        public string GetColorToStringRGBA(Color color)
        {
            return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
        }
    }
}