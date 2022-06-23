using UnityEngine;
using UnityEngine.UI;

public class ChildSizeFitter : MonoBehaviour
{
    [SerializeField] private RectTransform m_ReferChild = default;

    void Update()
    {
        if (m_ReferChild == default) return;

        var w = LayoutUtility.GetPreferredWidth(m_ReferChild);
        var h = LayoutUtility.GetPreferredHeight(m_ReferChild);

        this.GetRectTransform().sizeDelta = new Vector2(w, h);
    }
}
