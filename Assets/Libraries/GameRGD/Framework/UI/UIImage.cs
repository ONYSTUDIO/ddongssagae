using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu(UIConst.groupName + "/Image" + UIConst.suffixUI, 11)]
public class UIImage : UnityEngine.UI.Image
{
    [SerializeField]
    private bool m_Tooltip = true;
    [SerializeField]
    private string m_TooltipKey;
    [SerializeField]
    private RectTransform m_TooltipTransform;

    private IDisposable m_TooltipDisposable;

    public new Sprite sprite
    {
        get
        {
            return base.sprite;
        }
        set
        {
            m_TooltipKey = null;

            base.sprite = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (!m_Tooltip)
            m_TooltipDisposable = Disposable.Empty;

        if (!string.IsNullOrEmpty(m_TooltipKey))
            SetupTooltipHandler();
    }

    protected override void OnDestroy()
    {
        m_TooltipDisposable?.Dispose();

        base.OnDestroy();
    }

    public void SetSprite(Sprite sprite, string tooltip)
    {
        base.sprite = sprite;
        m_TooltipKey = tooltip;

        SetupTooltipHandler();
    }

    private void SetupTooltipHandler()
    {
        if (m_TooltipDisposable != null)
            return;

        // #FIXME 주석 제거 필요
        // if (!Helen.LocalizationService.ExistsTooltip(m_TooltipKey))
        //     return;

        if (m_TooltipTransform == null)
            m_TooltipTransform = this.rectTransform;

        m_TooltipDisposable = m_TooltipTransform
            .GetOrAddComponent<ObservablePointerClickTrigger>()
            .OnPointerClickAsObservable()
            .Subscribe(OnTooltip);
    }

    private static Subject<string> onTooltip = new Subject<string>();

    public static IObservable<string> OnTooltipAsObservable()
    {
        return onTooltip;
    }

    private void OnTooltip(PointerEventData eventData)
    {
        onTooltip.OnNext(m_TooltipKey);
    }
}