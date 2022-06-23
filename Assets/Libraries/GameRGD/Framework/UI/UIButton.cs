using SelectionState;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu(UIConst.groupName + "/Button" + UIConst.suffixUI, 30)]
public class UIButton : UnityEngine.UI.Button, ISelectionStateGroup
{
    //[SerializeField]
    private bool m_PublishWhenInactive = false;

    public bool publishWhenInactive
    {
        get => m_PublishWhenInactive;
        set => m_PublishWhenInactive = value;
    }

    protected override void Awake()
    {
        base.Awake();

        var navigation = UnityEngine.UI.Navigation.defaultNavigation;
        navigation.mode = UnityEngine.UI.Navigation.Mode.None;
        this.navigation = navigation;
    }

    private void PressWhenDisable()
    {
        if (!IsActive())
            return;

        UISystemProfilerApi.AddMarker("Button.onClick", this);
        onClick.Invoke();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (publishWhenInactive && !IsInteractable())
            PressWhenDisable();
        else
            base.OnPointerClick(eventData);
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (publishWhenInactive && !IsInteractable())
            PressWhenDisable();
        else
            base.OnSubmit(eventData);
    }

    protected override void InstantClearState()
    {
        base.InstantClearState();

        foreach (var item in m_SelectionStateCache)
            item.InstantClearState();
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        foreach (var item in m_SelectionStateCache)
            item.DoStateTransition((this as ISelectionStateGroup).currentSelectionState, instant);
    }

    GameObject ISelectionStateGroup.gameObject
    {
        get { return gameObject; }
    }

    SelectionStateType ISelectionStateGroup.currentSelectionState
    {
        get
        {
            switch (currentSelectionState)
            {
                case SelectionState.Pressed:
                    return SelectionStateType.Pressed;

                case SelectionState.Disabled:
                    return SelectionStateType.Disabled;

                default:
                    return SelectionStateType.Normal;
            }
        }
    }

    private readonly List<ISelectionState> m_SelectionStateCache = new List<ISelectionState>();

    void ISelectionStateGroup.RegisterSelectionState(ISelectionState state)
    {
        m_SelectionStateCache.Add(state);
    }

    void ISelectionStateGroup.UnregisterSelectionState(ISelectionState state)
    {
        m_SelectionStateCache.Remove(state);
    }
}