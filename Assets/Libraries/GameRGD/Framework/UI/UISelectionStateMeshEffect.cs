using SelectionState;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(UIConst.groupName + "/SelectionState/ButtonTextEffect - State" + UIConst.suffixUI, 14)]
public class UISelectionStateMeshEffect : UIBaseMeshEffect, ISelectionState
{
    [SerializeField]
    private ColorSetData m_ColorSetData = null;

    protected override Color color
    {
        get
        {
            if (m_ColorSetData && m_SelectionStateGroup != null)
            {
                switch (m_SelectionStateGroup.currentSelectionState)
                {
                    case SelectionStateType.Normal: return m_ColorSetData.normalColor;
                    case SelectionStateType.Pressed: return m_ColorSetData.pressedColor;
                    case SelectionStateType.Disabled: return m_ColorSetData.disabledColor;
                }
            }
            return Color.white;
        }
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

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();

        SetSelectionStateGroup(GetComponentInParent<ISelectionStateGroup>());
    }

    private void InstantClearState()
    {
        if (!IsActive())
            return;

        if (m_SelectionStateGroup != null && graphic != null)
            graphic.SetVerticesDirty();
    }

    private void DoStateTransition(SelectionStateType state, bool instant)
    {
        if (!IsActive())
            return;

        if (m_SelectionStateGroup != null && graphic != null)
            graphic.SetVerticesDirty();
    }

    private ISelectionStateGroup m_SelectionStateGroup = null;

    private void SetSelectionStateGroup(ISelectionStateGroup group, bool verticesDirty = true)
    {
        if (m_SelectionStateGroup != group)
        {
            if (m_SelectionStateGroup != null)
                m_SelectionStateGroup.UnregisterSelectionState(this);

            m_SelectionStateGroup = group;

            if (m_SelectionStateGroup != null)
                m_SelectionStateGroup.RegisterSelectionState(this);
        }

        if (graphic != null && verticesDirty)
            graphic.SetVerticesDirty();
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

    public override void ModifyMesh(VertexHelper vh)
    {
        if (m_ColorSetData && m_SelectionStateGroup != null)
            base.ModifyMesh(vh);
    }
}