using UnityEngine;

namespace SelectionState
{
    public enum SelectionStateType
    {
        Normal = 0,
        Pressed = 1,
        Disabled = 2,
    }

    public interface ISelectionState
    {
        void InstantClearState();
        void DoStateTransition(SelectionStateType state, bool instant);
    }

    public interface ISelectionStateGroup
    {
        GameObject gameObject { get; }

        SelectionStateType currentSelectionState { get; }

        void RegisterSelectionState(ISelectionState state);
        void UnregisterSelectionState(ISelectionState state);
    }
}