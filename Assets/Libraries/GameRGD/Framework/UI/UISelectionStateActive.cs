using SelectionState;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
[AddComponentMenu(UIConst.groupName + "/SelectionState/SelectionStateActive - State" + UIConst.suffixUI, 31)]
public class UISelectionStateActive : UIBehaviour, ISelectionState
{
    [SerializeField]
    private bool m_ReverseAction = false;

    protected UISelectionStateActive()
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
        bool value = state != SelectionStateType.Disabled;
        if (m_ReverseAction)
            value = !value;
        gameObject.SetActive(value);
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