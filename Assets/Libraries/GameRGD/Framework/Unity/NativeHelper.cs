using System.Runtime.InteropServices;
using UnityEngine;

public class NativeHelper
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] public static extern int IsJailBreak();
#endif
}