using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    // this.transform내의 UIButton이 클릭 후에 특정 시간 동안 작동 하지 않도록 하는 Timer 기능
    // TimerSeconds의 시간은 별도로 세팅 해주어야 함
    // TimerSeconds는 필요시 공통으로 세팅
    public class UIButtonTimer : MonoBase
    {
        [SerializeField] private float TimerSeconds = default;

        private void Awake()
        {
            if (TimerSeconds > 0f && this.TryGetComponent<Button>(out var _button))
            {
                _button.OnClickAsObservable().Subscribe(async _ =>
                {
                    _button.enabled = false;
                    await UniTask.Delay(TimeSpan.FromSeconds(TimerSeconds));
                    _button.enabled = true;
                }).AddTo(this);
            }
        }
    }
}