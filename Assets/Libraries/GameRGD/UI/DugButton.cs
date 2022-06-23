using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    public class DugButton : Button
    {
        protected override void Start()
        {
            base.Start();
            GetComponent<DugButtonWithDisabledOnChange>()?.SetDisabled(interactable == false);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            if (state == SelectionState.Disabled)
            {
                GetComponent<DugButtonWithDisabledOnChange>()?.SetDisabled(true);
            }
            else if (state == SelectionState.Normal)
            {
                GetComponent<DugButtonWithDisabledOnChange>()?.SetDisabled(false);
            }
        }
    }
}