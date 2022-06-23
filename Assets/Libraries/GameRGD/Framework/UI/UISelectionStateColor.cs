using System;
using SelectionState;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[AddComponentMenu(UIConst.groupName + "/SelectionState/SelectionStateColor - State" + UIConst.suffixUI, 31)]
public class UISelectionStateColor : UIBehaviour, ISelectionState
{
    [NonSerialized]
    private Graphic m_TargetGraphic;
    private Graphic targetGraphic
    {
        get
        {
            if (m_TargetGraphic == null)
                m_TargetGraphic = GetComponent<Graphic>();

            return m_TargetGraphic;
        }
    }

    [SerializeField]
    private ColorSetData m_ColorSetData = null;

    protected UISelectionStateColor()
    {
    }

    protected override void Awake()
    {
        base.Awake();

        SetSelectionStateGroup(GetComponentInParent<ISelectionStateGroup>());
    }

    protected override void OnDestroy()
    {
        SetSelectionStateGroup(null);

        base.OnDestroy();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_SelectionStateGroup != null)
            DoStateTransition(m_SelectionStateGroup.currentSelectionState, true);
        else
            InstantClearState();
    }

    protected override void OnDisable()
    {
        InstantClearState();

        base.OnDisable();
    }

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();

        SetSelectionStateGroup(GetComponentInParent<ISelectionStateGroup>());
    }

    private void InstantClearState()
    {
        StartColorTween(Color.white, 0.0f);
    }

    private void DoStateTransition(SelectionStateType state, bool instant)
    {
        if (!IsActive())
            return;

        if (!m_ColorSetData)
            return;

        Color tintColor;
        switch (state)
        {
            case SelectionStateType.Normal:
                tintColor = m_ColorSetData.normalColor;
                break;

            case SelectionStateType.Pressed:
                tintColor = m_ColorSetData.pressedColor;
                break;

            case SelectionStateType.Disabled:
                tintColor = m_ColorSetData.disabledColor;
                break;

            default:
                tintColor = Color.white;
                break;
        }
        StartColorTween(tintColor * m_ColorSetData.colorMultiplier, instant ? 0f : m_ColorSetData.fadeDuration);
    }

    private void StartColorTween(Color targetColor, float fadeDuration)
    {
        if (targetGraphic != null)
            targetGraphic.CrossFadeColor(targetColor, fadeDuration, true, true);
    }

    private ISelectionStateGroup m_SelectionStateGroup = null;

    private void SetSelectionStateGroup(ISelectionStateGroup group)
    {
        if (m_SelectionStateGroup != group)
        {
            if (m_SelectionStateGroup != null)
                m_SelectionStateGroup.UnregisterSelectionState(this);

            m_SelectionStateGroup = group;

            if (m_SelectionStateGroup != null)
                m_SelectionStateGroup.RegisterSelectionState(this);
        }

        if (m_SelectionStateGroup != null)
            DoStateTransition(m_SelectionStateGroup.currentSelectionState, true);
        else
            InstantClearState();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!Application.isPlaying)
            SetSelectionStateGroup(GetComponentInParent<ISelectionStateGroup>());
    }
#endif

    void ISelectionState.InstantClearState()
    {
        InstantClearState();
    }

    void ISelectionState.DoStateTransition(SelectionStateType state, bool instant)
    {
        DoStateTransition(state, instant);
    }
}