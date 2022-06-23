using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[AddComponentMenu(UIConst.groupName + "/Text" + UIConst.suffixUI, 10)]
public class UIText : UnityEngine.UI.Text
{
    private const string m_UndefinedKey = "Undefined";

    [FormerlySerializedAs("LocalizationKey")]
    [SerializeField]
    private string m_LocalizationKey;

    private object[] m_LocalizationArgs;

    [FormerlySerializedAs("TextCase")]
    [SerializeField]
    private TextCaseType m_TextCase = TextCaseType.None;

    [FormerlySerializedAs("ColorSetData")]
    [SerializeField]
    private ColorSetData m_ColorSetData;

    [SerializeField]
    private bool m_Tooltip = true;

    private string m_TooltipKey;

    [SerializeField]
    private RectTransform m_TooltipTransform;

    private IDisposable m_TooltipDisposable;

    public override Color color
    {
        get
        {
            return m_ColorSetData ? m_ColorSetData.normalColor : base.color;
        }
        set
        {
            if (m_ColorSetData)
                m_ColorSetData = null;
            base.color = value;
        }
    }

    public enum TextCaseType
    {
        None = 0,
        Uppper,
        Lower,
    }

    public override string text
    {
        get
        {
            switch (m_TextCase)
            {
                case TextCaseType.Uppper:
                    return base.text.ToUpper();

                case TextCaseType.Lower:
                    return base.text.ToLower();

                default:
                    return base.text;
            }
        }
        set
        {
            this.m_LocalizationKey = null;
            this.m_LocalizationArgs = null;
            this.m_TooltipKey = null;

            base.text = value;
        }
    }

    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(text, settings) / pixelsPerUnit;
        }
    }

    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(text, settings) / pixelsPerUnit;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (!m_Tooltip)
            m_TooltipDisposable = Disposable.Empty;

        if (!string.IsNullOrEmpty(m_LocalizationKey))
            Localize(m_LocalizationKey, m_LocalizationArgs);
    }

    protected override void OnDestroy()
    {
        m_TooltipDisposable?.Dispose();

        base.OnDestroy();
    }

    public void Localize(string key)
    {
        if (string.IsNullOrEmpty(key))
            key = m_UndefinedKey;

        m_LocalizationKey = key;
        m_LocalizationArgs = null;
        m_TooltipKey = key;

        // #FIXME 주석제거 필요
        // base.text = Helen.LocalizationService.Get(m_LocalizationKey);

        SetupTooltipHandler();
    }

    public void Localize(string key, params object[] args)
    {
        if (string.IsNullOrEmpty(key))
            key = m_UndefinedKey;

        m_LocalizationKey = key;
        m_LocalizationArgs = args;
        m_TooltipKey = key;

        // #FIXME 주석제거 필요
        // base.text = Helen.LocalizationService.Get(m_LocalizationKey, m_LocalizationArgs);

        SetupTooltipHandler();
    }

    public void SetText(string text, string tooltip)
    {
        this.m_LocalizationKey = null;
        this.m_LocalizationArgs = null;
        this.m_TooltipKey = tooltip;

        base.text = text;

        SetupTooltipHandler();
    }

    private void SetupTooltipHandler()
    {
        if (m_TooltipDisposable != null)
            return;

        if (m_TooltipKey == m_UndefinedKey)
            return;

        // #FIXME 주석제거 필요
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

    // #FIXME 주석제거 필요
    // public void SetFormatNumberText(double value, Helen.DISPLY_1K_AMOUNT displayType = Helen.DISPLY_1K_AMOUNT.ITEM)
    // {
    //     text = Helen.UIService.GetFormatNumberText(value, displayType);
    // }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        if (!string.IsNullOrEmpty(m_LocalizationKey))
        {
            // #FIXME 주석제거 필요
            // if (!Application.isPlaying && !Application.isBatchMode)
            //     base.text = Helen.EditorOnlyLocalizationService.Get(m_LocalizationKey);
        }
        base.OnValidate();
    }

#endif
}