using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace DoubleuGames.GameRGD
{
    [InitializeOnLoad]
    public class ProjectWindowHelper
    {
        [MenuItem("CONTEXT/MonoBehaviour/Pin Script", false, 620)]
        private static void PinScript(MenuCommand mc)
        {
            var script = MonoScript.FromMonoBehaviour(mc.context as MonoBehaviour);
            EditorGUIUtility.PingObject(script);
        }

        [MenuItem("CONTEXT/MonoBehaviour/Pin Editor", false, 619)]
        private static void PinEditor(MenuCommand mc)
        {
            Object editorClass;

            var script = MonoScript.FromMonoBehaviour(mc.context as MonoBehaviour);
            var guidStrs = AssetDatabase.FindAssets(script.name + "Editor");
            if (guidStrs.Length == 0)
                guidStrs = AssetDatabase.FindAssets(script.name + "Inspector");

            if (guidStrs.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guidStrs[0]);
                editorClass = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(editorClass);
            }
            else
            {
                Debug.LogWarning("No Editor Class for this script!!!");
            }
        }

        [MenuItem("Assets/Pin Asset", false, 600)]
        private static void PinAsset(MenuCommand mc)
        {
            EditorGUIUtility.PingObject(Selection.activeInstanceID);
        }
    }
}
#endif
