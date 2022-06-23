using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace DoubleuGames.GameRGD
{
    [RequireComponent(typeof(Toggle))]
    public class DugToggleTextControl : MonoBase
    {
        [SerializeField] private Color m_SelectedColor = default;
        [SerializeField] private Color m_UnselectedColor = default;
        void Start()
        {
            if (TryGetComponent<Toggle>(out var toggle))
            {
                toggle.OnValueChangedAsObservable().Subscribe(b =>
                {
                    var text = toggle.GetComponentsInChildren<Text>();
                    text.ForAll(t => t.color = b ? m_SelectedColor : m_UnselectedColor);
                });
            }
        }
    }
}