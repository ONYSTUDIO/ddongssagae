using System;
using SelectionState;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[AddComponentMenu(UIConst.groupName + "/SelectionState/SelectionStateSprite - State" + UIConst.suffixUI, 31)]
public class UISelectionStateSprite : UIBehaviour, ISelectionState
{
    [SerializeField]
    private Sprite m_NormalSprite = null;

    [SerializeField]
    private Sprite m_DisabledSprite = null;

    [NonSerialized]
    private Image m_TargetImage;
    private Image targetImage
    {
        get
        {
            if (m_TargetImage == null)
                m_TargetImage = GetComponent<Image>();

            return m_TargetImage;
        }
    }

    protected UISelectionStateSprite()
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
    }

    private void DoStateTransition(SelectionStateType state, bool instant)
    {
        if (!IsActive())
            return;

        bool value = state != SelectionStateType.Disabled;
        if (targetImage != null)
            targetImage.sprite = value ? m_NormalSprite : m_DisabledSprite;
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