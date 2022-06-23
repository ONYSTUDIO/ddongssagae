using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    public class DugButtonHasText : Button
    {
        private Subject<int> m_OnChageState = new Subject<int>();
        public IObservable<int> OnChageStateAsObservable() => m_OnChageState.AsObservable();

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (transition == Transition.ColorTint) m_OnChageState.OnNext((int) state);
        }
    }
}