using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    public class DugText : Text
    {
        private Color m_BeforeColor = default;
        private bool m_IsSetDisabled = false;

        new void Awake()
        {
            var _button = GetComponentInParent<DugButtonHasText>();
            if (_button != default)
            {
                _button.OnChageStateAsObservable().Subscribe(x => OnChageState(x, _button.colors.disabledColor)).AddTo(this);
            }
        }

        private void OnChageState(int state, Color disabledColor)
        {
            if (m_BeforeColor != default && state == 0)
            {
                // Normal
                color = m_BeforeColor;
                m_BeforeColor = default;
            }
            else if (m_BeforeColor == default && state == 4)
            {
                // Disabled
                m_BeforeColor = color;
                color -= disabledColor;
            }

        }
    }
}