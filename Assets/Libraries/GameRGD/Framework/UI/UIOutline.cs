using UnityEngine;

[AddComponentMenu(UIConst.groupName + "/Effects/Outline" + UIConst.suffixUI, 15)]
public class UIOutline : UnityEngine.UI.Outline
{
#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        if (GetComponent<UIBaseMeshEffect>())
        {
            if (UnityEditor.EditorUtility.DisplayDialog(
                    "UI Effect 중복 사용",
                    "Outline과 TextEffect를 같이 사용할 수 없습니다. MeshEffect를 삭제 후 컴포넌트를 추가해주세요.",
                    "확인"))
            {
                DestroyImmediate(this);
            }
        }
    }
#endif
}