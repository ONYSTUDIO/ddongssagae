using System.Collections.Generic;
using SelectionState;
using UnityEngine;

[AddComponentMenu(UIConst.groupName + "/Toggle" + UIConst.suffixUI, 31)]
[RequireComponent(typeof(RectTransform))]
public class UIToggle : UnityEngine.UI.Toggle, ISelectionStateGroup
{
    public GameObject stateOn;
    public GameObject stateOff;

    protected override void Awake()
    {
        base.Awake();

        var navigation = UnityEngine.UI.Navigation.defaultNavigation;
        navigation.mode = UnityEngine.UI.Navigation.Mode.None;
        this.navigation = navigation;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        onValueChanged.AddListener(OnValueChanged);

        OnValueChanged(isOn);
    }

    protected override void OnDisable()
    {
        onValueChanged.RemoveListener(OnValueChanged);

        base.OnDisable();
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

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        OnValueChanged(isOn);
    }
#endif

    private void OnValueChanged(bool value)
    {
        if (stateOn)
            stateOn.SetActive(value);

        if (stateOff)
            stateOff.SetActive(!value);
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