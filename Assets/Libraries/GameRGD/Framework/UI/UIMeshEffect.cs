using UnityEngine;

[AddComponentMenu(UIConst.groupName + "/TextEffect" + UIConst.suffixUI, 14)]
public class UIMeshEffect : UIBaseMeshEffect
{
    [SerializeField]
    private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);

    [SerializeField]
    private ColorSetData m_ColorSetData = null;

    protected override Color color
    {
        get
        {
            if (m_ColorSetData)
                return m_ColorSetData.normalColor;
            return m_EffectColor;
        }
    }
}