using UnityEngine;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    public class DugButtonWithDisabledOnChange : MonoBehaviour
    {
        [SerializeField] private Color m_NormalTextColor;
        [SerializeField] private Color m_DisabledTextColor;

        public void SetDisabled(bool disabled)
        {
            GetComponentsInChildren<Text>(true).ForAll(u =>
            {
                u.color = disabled ? m_DisabledTextColor : m_NormalTextColor;
            });
        }
    }
}