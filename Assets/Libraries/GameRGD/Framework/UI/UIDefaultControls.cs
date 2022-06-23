using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR

using UnityEditor;

#endif

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:사용하지 않는 매개 변수를 제거하세요.", Justification = "<보류 중>")]
public static class UIDefaultControls
{    
    public static GameObject CreateTMPText(DefaultControls.Resources resources)
    {
        GameObject go = CreateUIElementRoot("Text", s_ThickElementSize, typeof(TMPText));

        TMPText lbl = go.GetComponent<TMPText>();
        lbl.text = "New Text";
        SetDefaultTextValues(lbl);

        return go;
    }

    public static GameObject CreateTMPButton(DefaultControls.Resources resources)
    {
        GameObject buttonRoot = CreateUIElementRoot("Button", s_ThickElementSize, typeof(UIImage), typeof(UIButton));

        GameObject childText = CreateUIObject("Text", buttonRoot, typeof(TMPText));

        UIImage image = buttonRoot.GetComponent<UIImage>();
        image.sprite = resources.standard;
        image.type = Image.Type.Sliced;
        image.color = s_DefaultSelectableColor;

        UIButton bt = buttonRoot.GetComponent<UIButton>();
        SetDefaultColorTransitionValues(bt);

        TMPText text = childText.GetComponent<TMPText>();
        text.text = "Button";
        text.alignment = TextAlignmentOptions.Center;
        SetDefaultTextValues(text);

        RectTransform textRectTransform = childText.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;

        return buttonRoot;
    }

    public static GameObject CreateTMPInputField(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("InputField", s_ThickElementSize, typeof(UIImage), typeof(TMPInputField));

        GameObject childPlaceholder = CreateUIObject("Placeholder", root, typeof(TMPText));
        GameObject childText = CreateUIObject("Text", root, typeof(TMPText));

        UIImage image = root.GetComponent<UIImage>();
        image.sprite = resources.inputField;
        image.type = Image.Type.Sliced;
        image.color = s_DefaultSelectableColor;

        TMPInputField inputField = root.GetComponent<TMPInputField>();
        SetDefaultColorTransitionValues(inputField);

        TMPText text = childText.GetComponent<TMPText>();
        text.text = "";
        text.richText = false;
        SetDefaultTextValues(text);

        TMPText placeholder = childPlaceholder.GetComponent<TMPText>();
        placeholder.text = "Enter text...";
        placeholder.fontStyle = FontStyles.Italic;
        // Make placeholder color half as opaque as normal text color.
        Color placeholderColor = text.color;
        placeholderColor.a *= 0.5f;
        placeholder.color = placeholderColor;

        RectTransform textRectTransform = childText.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;
        textRectTransform.offsetMin = new Vector2(10, 6);
        textRectTransform.offsetMax = new Vector2(-10, -7);

        RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
        placeholderRectTransform.anchorMin = Vector2.zero;
        placeholderRectTransform.anchorMax = Vector2.one;
        placeholderRectTransform.sizeDelta = Vector2.zero;
        placeholderRectTransform.offsetMin = new Vector2(10, 6);
        placeholderRectTransform.offsetMax = new Vector2(-10, -7);

        inputField.textComponent = text;
        inputField.placeholder = placeholder;

        return root;
    }

    public static GameObject CreateTMPDropdown(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("Dropdown", s_ThickElementSize, typeof(UIImage), typeof(TMPDropdown));

        GameObject label = CreateUIObject("Label", root, typeof(TMPText));
        GameObject arrow = CreateUIObject("Arrow", root, typeof(UIImage));
        GameObject template = CreateUIObject("Template", root, typeof(UIImage), typeof(UIScrollRect));
        GameObject viewport = CreateUIObject("Viewport", template, typeof(UIImage), typeof(UIMask));
        GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));
        GameObject item = CreateUIObject("Item", content, typeof(UIToggle));
        GameObject itemBackground = CreateUIObject("Item Background", item, typeof(UIImage));
        GameObject itemCheckmark = CreateUIObject("Item Checkmark", item, typeof(UIImage));
        GameObject itemLabel = CreateUIObject("Item Label", item, typeof(TMPText));

        // Sub controls.

        GameObject scrollbar = CreateScrollbar(resources);
        scrollbar.name = "Scrollbar";
        SetParentAndAlign(scrollbar, template);

        UIScrollbar scrollbarScrollbar = scrollbar.GetComponent<UIScrollbar>();
        scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);

        RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
        vScrollbarRT.anchorMin = Vector2.right;
        vScrollbarRT.anchorMax = Vector2.one;
        vScrollbarRT.pivot = Vector2.one;
        vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

        // Setup item UI components.

        TMPText itemLabelText = itemLabel.GetComponent<TMPText>();
        SetDefaultTextValues(itemLabelText);
        itemLabelText.alignment = TextAlignmentOptions.MidlineLeft;

        UIImage itemBackgroundImage = itemBackground.GetComponent<UIImage>();
        itemBackgroundImage.color = new Color32(245, 245, 245, 255);

        UIImage itemCheckmarkImage = itemCheckmark.GetComponent<UIImage>();
        itemCheckmarkImage.sprite = resources.checkmark;

        UIToggle itemToggle = item.GetComponent<UIToggle>();
        itemToggle.targetGraphic = itemBackgroundImage;
        itemToggle.graphic = itemCheckmarkImage;
        itemToggle.isOn = true;

        // Setup template UI components.

        UIImage templateImage = template.GetComponent<UIImage>();
        templateImage.sprite = resources.standard;
        templateImage.type = Image.Type.Sliced;

        UIScrollRect templateScrollRect = template.GetComponent<UIScrollRect>();
        templateScrollRect.content = content.GetComponent<RectTransform>();
        templateScrollRect.viewport = viewport.GetComponent<RectTransform>();
        templateScrollRect.horizontal = false;
        templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
        templateScrollRect.verticalScrollbar = scrollbarScrollbar;
        templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        templateScrollRect.verticalScrollbarSpacing = -3;

        UIMask scrollRectMask = viewport.GetComponent<UIMask>();
        scrollRectMask.showMaskGraphic = false;

        UIImage viewportImage = viewport.GetComponent<UIImage>();
        viewportImage.sprite = resources.mask;
        viewportImage.type = Image.Type.Sliced;

        // Setup dropdown UI components.

        TMPText labelText = label.GetComponent<TMPText>();
        SetDefaultTextValues(labelText);
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        UIImage arrowImage = arrow.GetComponent<UIImage>();
        arrowImage.sprite = resources.dropdown;

        UIImage backgroundImage = root.GetComponent<UIImage>();
        backgroundImage.sprite = resources.standard;
        backgroundImage.color = s_DefaultSelectableColor;
        backgroundImage.type = Image.Type.Sliced;

        TMPDropdown dropdown = root.GetComponent<TMPDropdown>();
        dropdown.targetGraphic = backgroundImage;
        SetDefaultColorTransitionValues(dropdown);
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.captionText = labelText;
        dropdown.itemText = itemLabelText;

        // Setting default Item list.
        itemLabelText.text = "Option A";
        dropdown.options.Add(new TMPDropdown.OptionData { text = "Option A" });
        dropdown.options.Add(new TMPDropdown.OptionData { text = "Option B" });
        dropdown.options.Add(new TMPDropdown.OptionData { text = "Option C" });
        dropdown.RefreshShownValue();

        // Set up RectTransforms.

        RectTransform labelRT = label.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(10, 6);
        labelRT.offsetMax = new Vector2(-25, -7);

        RectTransform arrowRT = arrow.GetComponent<RectTransform>();
        arrowRT.anchorMin = new Vector2(1, 0.5f);
        arrowRT.anchorMax = new Vector2(1, 0.5f);
        arrowRT.sizeDelta = new Vector2(20, 20);
        arrowRT.anchoredPosition = new Vector2(-15, 0);

        RectTransform templateRT = template.GetComponent<RectTransform>();
        templateRT.anchorMin = new Vector2(0, 0);
        templateRT.anchorMax = new Vector2(1, 0);
        templateRT.pivot = new Vector2(0.5f, 1);
        templateRT.anchoredPosition = new Vector2(0, 2);
        templateRT.sizeDelta = new Vector2(0, 150);

        RectTransform viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = new Vector2(0, 0);
        viewportRT.anchorMax = new Vector2(1, 1);
        viewportRT.sizeDelta = new Vector2(-18, 0);
        viewportRT.pivot = new Vector2(0, 1);

        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1);
        contentRT.anchorMax = new Vector2(1f, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = new Vector2(0, 0);
        contentRT.sizeDelta = new Vector2(0, 28);

        RectTransform itemRT = item.GetComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0, 0.5f);
        itemRT.anchorMax = new Vector2(1, 0.5f);
        itemRT.sizeDelta = new Vector2(0, 20);

        RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
        itemBackgroundRT.anchorMin = Vector2.zero;
        itemBackgroundRT.anchorMax = Vector2.one;
        itemBackgroundRT.sizeDelta = Vector2.zero;

        RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
        itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
        itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
        itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
        itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

        RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
        itemLabelRT.anchorMin = Vector2.zero;
        itemLabelRT.anchorMax = Vector2.one;
        itemLabelRT.offsetMin = new Vector2(20, 1);
        itemLabelRT.offsetMax = new Vector2(-10, -2);

        template.SetActive(false);

        return root;
    }

    public static GameObject CreateText(DefaultControls.Resources resources)
    {
        GameObject go = CreateUIElementRoot("Text", s_ThickElementSize, typeof(UIText));

        UIText lbl = go.GetComponent<UIText>();
        lbl.text = "New Text";
        SetDefaultTextValues(lbl);

        return go;
    }

    public static GameObject CreateImage(DefaultControls.Resources resources)
    {
        GameObject go = CreateUIElementRoot("Image", s_ImageElementSize, typeof(UIImage));
        return go;
    }

    public static GameObject CreateRawImage(DefaultControls.Resources resources)
    {
        GameObject go = CreateUIElementRoot("RawImage", s_ImageElementSize, typeof(UIRawImage));
        return go;
    }

    public static GameObject CreateButton(DefaultControls.Resources resources)
    {
        GameObject buttonRoot = CreateUIElementRoot("Button", s_ThickElementSize, typeof(UIImage), typeof(UIButton));

        GameObject childText = CreateUIObject("Text", buttonRoot, typeof(UIText));

        UIImage image = buttonRoot.GetComponent<UIImage>();
        image.sprite = resources.standard;
        image.type = Image.Type.Sliced;
        image.color = s_DefaultSelectableColor;

        UIButton bt = buttonRoot.GetComponent<UIButton>();
        SetDefaultColorTransitionValues(bt);

        UIText text = childText.GetComponent<UIText>();
        text.text = "Button";
        text.alignment = TextAnchor.MiddleCenter;
        SetDefaultTextValues(text);

        RectTransform textRectTransform = childText.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;

        return buttonRoot;
    }

    public static GameObject CreateToggle(DefaultControls.Resources resources)
    {
        // Set up hierarchy
        GameObject toggleRoot = CreateUIElementRoot("Toggle", s_ThinElementSize, typeof(UIToggle));

        GameObject background = CreateUIObject("Background", toggleRoot, typeof(UIImage));
        GameObject checkmark = CreateUIObject("Checkmark", background, typeof(UIImage));
        GameObject childLabel = CreateUIObject("Label", toggleRoot, typeof(UIText));

        // Set up components
        UIToggle toggle = toggleRoot.GetComponent<UIToggle>();
        toggle.isOn = true;

        UIImage bgImage = background.GetComponent<UIImage>();
        bgImage.sprite = resources.standard;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = s_DefaultSelectableColor;

        UIImage checkmarkImage = checkmark.GetComponent<UIImage>();
        checkmarkImage.sprite = resources.checkmark;

        UIText label = childLabel.GetComponent<UIText>();
        label.text = "Toggle";
        SetDefaultTextValues(label);

        toggle.graphic = checkmarkImage;
        toggle.targetGraphic = bgImage;
        SetDefaultColorTransitionValues(toggle);

        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.anchoredPosition = new Vector2(10f, -10f);
        bgRect.sizeDelta = new Vector2(kThinHeight, kThinHeight);

        RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchoredPosition = Vector2.zero;
        checkmarkRect.sizeDelta = new Vector2(20f, 20f);

        RectTransform labelRect = childLabel.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(23f, 1f);
        labelRect.offsetMax = new Vector2(-5f, -2f);

        return toggleRoot;
    }

    public static GameObject CreateSlider(DefaultControls.Resources resources)
    {
        // Create GOs Hierarchy
        GameObject root = CreateUIElementRoot("Slider", s_ThinElementSize, typeof(UISlider));

        GameObject background = CreateUIObject("Background", root, typeof(UIImage));
        GameObject fillArea = CreateUIObject("Fill Area", root, typeof(RectTransform));
        GameObject fill = CreateUIObject("Fill", fillArea, typeof(UIImage));
        GameObject handleArea = CreateUIObject("Handle Slide Area", root, typeof(RectTransform));
        GameObject handle = CreateUIObject("Handle", handleArea, typeof(UIImage));

        // Background
        UIImage backgroundImage = background.GetComponent<UIImage>();
        backgroundImage.sprite = resources.background;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.color = s_DefaultSelectableColor;
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0, 0.25f);
        backgroundRect.anchorMax = new Vector2(1, 0.75f);
        backgroundRect.sizeDelta = new Vector2(0, 0);

        // Fill Area
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.anchoredPosition = new Vector2(-5, 0);
        fillAreaRect.sizeDelta = new Vector2(-20, 0);

        // Fill
        UIImage fillImage = fill.GetComponent<UIImage>();
        fillImage.sprite = resources.standard;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = s_DefaultSelectableColor;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(10, 0);

        // Handle Area
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.sizeDelta = new Vector2(-20, 0);
        handleAreaRect.anchorMin = new Vector2(0, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);

        // Handle
        UIImage handleImage = handle.GetComponent<UIImage>();
        handleImage.sprite = resources.knob;
        handleImage.color = s_DefaultSelectableColor;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);

        // Setup slider component
        UISlider slider = root.GetComponent<UISlider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        SetDefaultColorTransitionValues(slider);

        return root;
    }

    public static GameObject CreateMaskSlider(DefaultControls.Resources resources)
    {
        // Create GOs Hierarchy
        GameObject root = CreateUIElementRoot("Slider", s_ThinElementSize, typeof(UIMaskSlider));

        GameObject background = CreateUIObject("Background", root, typeof(UIImage));
        GameObject fillArea = CreateUIObject("Fill Area", root, typeof(UIImage), typeof(UIMask));
        GameObject fill = CreateUIObject("Fill", fillArea, typeof(UIImage));
        GameObject handleArea = CreateUIObject("Handle Slide Area", root, typeof(RectTransform));
        GameObject handle = CreateUIObject("Handle", handleArea, typeof(UIImage));

        // Background
        UIImage backgroundImage = background.GetComponent<UIImage>();
        backgroundImage.sprite = resources.background;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.color = s_DefaultSelectableColor;
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0, 0.25f);
        backgroundRect.anchorMax = new Vector2(1, 0.75f);
        backgroundRect.sizeDelta = new Vector2(0, 0);

        // Fill Area
        UIImage fillAreaMaskImage = fillArea.GetComponent<UIImage>();
        fillAreaMaskImage.sprite = resources.standard;
        fillAreaMaskImage.type = UIImage.Type.Filled;
        fillAreaMaskImage.fillMethod = UIImage.FillMethod.Horizontal;
        fillAreaMaskImage.fillOrigin = (int)UIImage.OriginHorizontal.Left;
        UIMask fillAreaMask = fillArea.GetComponent<UIMask>();
        fillAreaMask.showMaskGraphic = false;

        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.anchoredPosition = new Vector2(0, 0);
        fillAreaRect.sizeDelta = new Vector2(0, 0);

        // Fill
        UIImage fillImage = fill.GetComponent<UIImage>();
        fillImage.sprite = resources.standard;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = s_DefaultSelectableColor;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(0, 0);

        // Handle Area
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.sizeDelta = new Vector2(-20, 0);
        handleAreaRect.anchorMin = new Vector2(0, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);

        // Handle
        UIImage handleImage = handle.GetComponent<UIImage>();
        handleImage.sprite = resources.knob;
        handleImage.color = s_DefaultSelectableColor;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);

        // Setup slider component
        UIMaskSlider slider = root.GetComponent<UIMaskSlider>();
        slider.maskRect = fillArea.GetComponent<RectTransform>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.maskValue = 10;
        slider.targetGraphic = handleImage;
        slider.direction = UIMaskSlider.Direction.LeftToRight;
        SetDefaultColorTransitionValues(slider);

        return root;
    }

    public static GameObject CreateScrollbar(DefaultControls.Resources resources)
    {
        // Create GOs Hierarchy
        GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", s_ThinElementSize, typeof(UIImage), typeof(UIScrollbar));

        GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot, typeof(RectTransform));
        GameObject handle = CreateUIObject("Handle", sliderArea, typeof(UIImage));

        UIImage bgImage = scrollbarRoot.GetComponent<UIImage>();
        bgImage.sprite = resources.background;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = s_DefaultSelectableColor;

        UIImage handleImage = handle.GetComponent<UIImage>();
        handleImage.sprite = resources.standard;
        handleImage.type = Image.Type.Sliced;
        handleImage.color = s_DefaultSelectableColor;

        RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
        sliderAreaRect.sizeDelta = new Vector2(-20, -20);
        sliderAreaRect.anchorMin = Vector2.zero;
        sliderAreaRect.anchorMax = Vector2.one;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);

        UIScrollbar scrollbar = scrollbarRoot.GetComponent<UIScrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;
        SetDefaultColorTransitionValues(scrollbar);

        return scrollbarRoot;
    }

    public static GameObject CreateDropdown(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("Dropdown", s_ThickElementSize, typeof(UIImage), typeof(UIDropdown));

        GameObject label = CreateUIObject("Label", root, typeof(UIText));
        GameObject arrow = CreateUIObject("Arrow", root, typeof(UIImage));
        GameObject template = CreateUIObject("Template", root, typeof(UIImage), typeof(UIScrollRect));
        GameObject viewport = CreateUIObject("Viewport", template, typeof(UIImage), typeof(UIMask));
        GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));
        GameObject item = CreateUIObject("Item", content, typeof(UIToggle));
        GameObject itemBackground = CreateUIObject("Item Background", item, typeof(UIImage));
        GameObject itemCheckmark = CreateUIObject("Item Checkmark", item, typeof(UIImage));
        GameObject itemLabel = CreateUIObject("Item Label", item, typeof(UIText));

        // Sub controls.

        GameObject scrollbar = CreateScrollbar(resources);
        scrollbar.name = "Scrollbar";
        SetParentAndAlign(scrollbar, template);

        UIScrollbar scrollbarScrollbar = scrollbar.GetComponent<UIScrollbar>();
        scrollbarScrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);

        RectTransform vScrollbarRT = scrollbar.GetComponent<RectTransform>();
        vScrollbarRT.anchorMin = Vector2.right;
        vScrollbarRT.anchorMax = Vector2.one;
        vScrollbarRT.pivot = Vector2.one;
        vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

        // Setup item UI components.

        UIText itemLabelText = itemLabel.GetComponent<UIText>();
        SetDefaultTextValues(itemLabelText);
        itemLabelText.alignment = TextAnchor.MiddleLeft;

        UIImage itemBackgroundImage = itemBackground.GetComponent<UIImage>();
        itemBackgroundImage.color = new Color32(245, 245, 245, 255);

        UIImage itemCheckmarkImage = itemCheckmark.GetComponent<UIImage>();
        itemCheckmarkImage.sprite = resources.checkmark;

        UIToggle itemToggle = item.GetComponent<UIToggle>();
        itemToggle.targetGraphic = itemBackgroundImage;
        itemToggle.graphic = itemCheckmarkImage;
        itemToggle.isOn = true;

        // Setup template UI components.

        UIImage templateImage = template.GetComponent<UIImage>();
        templateImage.sprite = resources.standard;
        templateImage.type = Image.Type.Sliced;

        UIScrollRect templateScrollRect = template.GetComponent<UIScrollRect>();
        templateScrollRect.content = content.GetComponent<RectTransform>();
        templateScrollRect.viewport = viewport.GetComponent<RectTransform>();
        templateScrollRect.horizontal = false;
        templateScrollRect.movementType = ScrollRect.MovementType.Clamped;
        templateScrollRect.verticalScrollbar = scrollbarScrollbar;
        templateScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        templateScrollRect.verticalScrollbarSpacing = -3;

        UIMask scrollRectMask = viewport.GetComponent<UIMask>();
        scrollRectMask.showMaskGraphic = false;

        UIImage viewportImage = viewport.GetComponent<UIImage>();
        viewportImage.sprite = resources.mask;
        viewportImage.type = Image.Type.Sliced;

        // Setup dropdown UI components.

        UIText labelText = label.GetComponent<UIText>();
        SetDefaultTextValues(labelText);
        labelText.alignment = TextAnchor.MiddleLeft;

        UIImage arrowImage = arrow.GetComponent<UIImage>();
        arrowImage.sprite = resources.dropdown;

        UIImage backgroundImage = root.GetComponent<UIImage>();
        backgroundImage.sprite = resources.standard;
        backgroundImage.color = s_DefaultSelectableColor;
        backgroundImage.type = Image.Type.Sliced;

        UIDropdown dropdown = root.GetComponent<UIDropdown>();
        dropdown.targetGraphic = backgroundImage;
        SetDefaultColorTransitionValues(dropdown);
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.captionText = labelText;
        dropdown.itemText = itemLabelText;

        // Setting default Item list.
        itemLabelText.text = "Option A";
        dropdown.options.Add(new UIDropdown.OptionData { text = "Option A" });
        dropdown.options.Add(new UIDropdown.OptionData { text = "Option B" });
        dropdown.options.Add(new UIDropdown.OptionData { text = "Option C" });
        dropdown.RefreshShownValue();

        // Set up RectTransforms.

        RectTransform labelRT = label.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(10, 6);
        labelRT.offsetMax = new Vector2(-25, -7);

        RectTransform arrowRT = arrow.GetComponent<RectTransform>();
        arrowRT.anchorMin = new Vector2(1, 0.5f);
        arrowRT.anchorMax = new Vector2(1, 0.5f);
        arrowRT.sizeDelta = new Vector2(20, 20);
        arrowRT.anchoredPosition = new Vector2(-15, 0);

        RectTransform templateRT = template.GetComponent<RectTransform>();
        templateRT.anchorMin = new Vector2(0, 0);
        templateRT.anchorMax = new Vector2(1, 0);
        templateRT.pivot = new Vector2(0.5f, 1);
        templateRT.anchoredPosition = new Vector2(0, 2);
        templateRT.sizeDelta = new Vector2(0, 150);

        RectTransform viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = new Vector2(0, 0);
        viewportRT.anchorMax = new Vector2(1, 1);
        viewportRT.sizeDelta = new Vector2(-18, 0);
        viewportRT.pivot = new Vector2(0, 1);

        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1);
        contentRT.anchorMax = new Vector2(1f, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = new Vector2(0, 0);
        contentRT.sizeDelta = new Vector2(0, 28);

        RectTransform itemRT = item.GetComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0, 0.5f);
        itemRT.anchorMax = new Vector2(1, 0.5f);
        itemRT.sizeDelta = new Vector2(0, 20);

        RectTransform itemBackgroundRT = itemBackground.GetComponent<RectTransform>();
        itemBackgroundRT.anchorMin = Vector2.zero;
        itemBackgroundRT.anchorMax = Vector2.one;
        itemBackgroundRT.sizeDelta = Vector2.zero;

        RectTransform itemCheckmarkRT = itemCheckmark.GetComponent<RectTransform>();
        itemCheckmarkRT.anchorMin = new Vector2(0, 0.5f);
        itemCheckmarkRT.anchorMax = new Vector2(0, 0.5f);
        itemCheckmarkRT.sizeDelta = new Vector2(20, 20);
        itemCheckmarkRT.anchoredPosition = new Vector2(10, 0);

        RectTransform itemLabelRT = itemLabel.GetComponent<RectTransform>();
        itemLabelRT.anchorMin = Vector2.zero;
        itemLabelRT.anchorMax = Vector2.one;
        itemLabelRT.offsetMin = new Vector2(20, 1);
        itemLabelRT.offsetMax = new Vector2(-10, -2);

        template.SetActive(false);

        return root;
    }

    public static GameObject CreateInputField(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("InputField", s_ThickElementSize, typeof(UIImage), typeof(UIInputField));

        GameObject childPlaceholder = CreateUIObject("Placeholder", root, typeof(UIText));
        GameObject childText = CreateUIObject("Text", root, typeof(UIText));

        UIImage image = root.GetComponent<UIImage>();
        image.sprite = resources.inputField;
        image.type = Image.Type.Sliced;
        image.color = s_DefaultSelectableColor;

        UIInputField inputField = root.GetComponent<UIInputField>();
        SetDefaultColorTransitionValues(inputField);

        UIText text = childText.GetComponent<UIText>();
        text.text = "";
        text.supportRichText = false;
        SetDefaultTextValues(text);

        UIText placeholder = childPlaceholder.GetComponent<UIText>();
        placeholder.text = "Enter text...";
        placeholder.fontStyle = FontStyle.Italic;
        // Make placeholder color half as opaque as normal text color.
        Color placeholderColor = text.color;
        placeholderColor.a *= 0.5f;
        placeholder.color = placeholderColor;

        RectTransform textRectTransform = childText.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;
        textRectTransform.offsetMin = new Vector2(10, 6);
        textRectTransform.offsetMax = new Vector2(-10, -7);

        RectTransform placeholderRectTransform = childPlaceholder.GetComponent<RectTransform>();
        placeholderRectTransform.anchorMin = Vector2.zero;
        placeholderRectTransform.anchorMax = Vector2.one;
        placeholderRectTransform.sizeDelta = Vector2.zero;
        placeholderRectTransform.offsetMin = new Vector2(10, 6);
        placeholderRectTransform.offsetMax = new Vector2(-10, -7);

        inputField.textComponent = text;
        inputField.placeholder = placeholder;

        return root;
    }

    public static GameObject CreatePanel(DefaultControls.Resources resources)
    {
        GameObject panelRoot = CreateUIElementRoot("Panel", s_ThickElementSize, typeof(UIImage));

        // Set RectTransform to stretch
        RectTransform rectTransform = panelRoot.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        Image image = panelRoot.GetComponent<UIImage>();
        image.sprite = resources.background;
        image.type = Image.Type.Sliced;
        image.color = s_PanelColor;

        return panelRoot;
    }

    public static GameObject CreateScrollView(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("Scroll View", new Vector2(200, 200), typeof(UIImage), typeof(UIScrollRect));

        GameObject viewport = CreateUIObject("Viewport", root, typeof(UIImage), typeof(UIMask));
        GameObject content = CreateUIObject("Content", viewport, typeof(RectTransform));

        // Sub controls.

        GameObject hScrollbar = CreateScrollbar(resources);
        hScrollbar.name = "Scrollbar Horizontal";
        SetParentAndAlign(hScrollbar, root);
        RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
        hScrollbarRT.anchorMin = Vector2.zero;
        hScrollbarRT.anchorMax = Vector2.right;
        hScrollbarRT.pivot = Vector2.zero;
        hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);

        GameObject vScrollbar = CreateScrollbar(resources);
        vScrollbar.name = "Scrollbar Vertical";
        SetParentAndAlign(vScrollbar, root);
        vScrollbar.GetComponent<UIScrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
        RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
        vScrollbarRT.anchorMin = Vector2.right;
        vScrollbarRT.anchorMax = Vector2.one;
        vScrollbarRT.pivot = Vector2.one;
        vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);

        // Setup RectTransforms.

        // Make viewport fill entire scroll view.
        RectTransform viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = Vector2.zero;
        viewportRT.pivot = Vector2.up;

        // Make context match viewpoprt width and be somewhat taller.
        // This will show the vertical scrollbar and not the horizontal one.
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = Vector2.up;
        contentRT.anchorMax = Vector2.one;
        contentRT.sizeDelta = new Vector2(0, 300);
        contentRT.pivot = Vector2.up;

        // Setup UI components.

        UIScrollRect scrollRect = root.GetComponent<UIScrollRect>();
        scrollRect.content = contentRT;
        scrollRect.viewport = viewportRT;
        scrollRect.horizontalScrollbar = hScrollbar.GetComponent<UIScrollbar>();
        scrollRect.verticalScrollbar = vScrollbar.GetComponent<UIScrollbar>();
        scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.horizontalScrollbarSpacing = -3;
        scrollRect.verticalScrollbarSpacing = -3;

        UIImage rootImage = root.GetComponent<UIImage>();
        rootImage.sprite = resources.background;
        rootImage.type = Image.Type.Sliced;
        rootImage.color = s_PanelColor;

        UIMask viewportMask = viewport.GetComponent<UIMask>();
        viewportMask.showMaskGraphic = false;

        UIImage viewportImage = viewport.GetComponent<UIImage>();
        viewportImage.sprite = resources.mask;
        viewportImage.type = Image.Type.Sliced;

        return root;
    }

    public static DefaultControls.IFactoryControls Factory
    {
        get => DefaultControls.factory;
    }

    private const float kWidth = 160f;
    private const float kThickHeight = 30f;
    private const float kThinHeight = 20f;
    private static Vector2 s_ThickElementSize = new Vector2(kWidth, kThickHeight);
    private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
    private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);
    private static Color s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
    private static Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);
    private static Color s_TextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

    public static GameObject CreateUIElementRoot(string name, Vector2 size, params Type[] components)
    {
        GameObject child = Factory.CreateGameObject(name, components);
        RectTransform rectTransform = child.GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        return child;
    }

    public static GameObject CreateUIObject(string name, GameObject parent, params Type[] components)
    {
        GameObject go = Factory.CreateGameObject(name, components);
        SetParentAndAlign(go, parent);
        return go;
    }

    public static GameObject CreateUIObject(string name, GameObject parent)
    {
        GameObject go = Factory.CreateGameObject(name);
        SetParentAndAlign(go, parent);

        RectTransform rectTransform = go.AddComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        return go;
    }

    private static void SetDefaultTextValues(Text lbl)
    {
        // Set text values we want across UI elements in default controls.
        // Don't set values which are the same as the default values for the Text component,
        // since there's no point in that, and it's good to keep them as consistent as possible.
        lbl.color = s_TextColor;

        // Reset() is not called when playing. We still want the default font to be assigned
        lbl.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void SetDefaultTextValues(TMPText lbl)
    {
        // Set text values we want across UI elements in default controls.
        // Don't set values which are the same as the default values for the Text component,
        // since there's no point in that, and it's good to keep them as consistent as possible.
        lbl.color = s_TextColor;
        lbl.fontSize = 14;
    }

    private static void SetDefaultColorTransitionValues(Selectable slider)
    {
        ColorBlock colors = slider.colors;
        colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
        colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
        colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
    }

    private static void SetParentAndAlign(GameObject child, GameObject parent)
    {
        if (parent == null)
            return;

#if UNITY_EDITOR
        Undo.SetTransformParent(child.transform, parent.transform, "");
#else
        child.transform.SetParent(parent.transform, false);
#endif
        SetLayerRecursively(child, parent.layer);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }
}