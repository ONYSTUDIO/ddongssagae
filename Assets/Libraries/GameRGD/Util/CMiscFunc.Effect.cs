#pragma warning disable 0618

using System;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public partial class CMiscFunc
    {
        public static float GetDuration(Vector3 from, Vector3 to, float velocity)
        {
            float _result = 0f;
            // 부하 발생시, 주석 풀고 교체
            // float _distance = (to - from).sqrMagnitude; 
            float _distance = Vector3.Distance(from, to);
            _result = _distance / velocity;
            return _result;
        }

        public static float GetDuration(Vector2 from, Vector2 to, float velocity)
        {
            float _result = 0f;
            // 부하 발생시, 주석 풀고 교체
            // float _distance = (to - from).sqrMagnitude;
            float _distance = Vector2.Distance(from, to);
            _result = _distance / velocity;
            return _result;
        }

        // Image의 fillAmount연출용, from에서 to까지 채워 지기 위한 시간값
        // totalDuration은 0~1까지 채워지는 토탈 시간
        public static float GetDurationByFillAmount(float from, float to, float totalDuration)
        {
            if (totalDuration <= 0f) throw new ArgumentOutOfRangeException(nameof(totalDuration));
            if (from == to) return 0f;

            float _result = totalDuration;
            _result = (to - from) * totalDuration;
            if (_result < 0) _result *= -1f;
            return _result;
        }

        // TweenToAmount() 사용시,
        // 전체 변화량 기준으로 amount값을 리턴
        // max = 최대값, duration = 0 ~ max까지 변하는 시간
        public static int GetAmountFromDuration(int max, float duration)
        {
            if (max == 0) throw new ArgumentOutOfRangeException(nameof(max));

            int _result = max;
            if (duration == 0) return _result;

            _result = (int)((float)max / duration);
            if (_result <= 0) _result = max;
            return _result;
        }

        // TweenToAmount() 사용시,
        // 전체 변화량 기준으로 amount값을 리턴
        // max = 최대값, duration = 0 ~ max까지 변하는 시간
        public static long GetAmountFromDuration(long max, float duration)
        {
            if (max == 0) throw new ArgumentOutOfRangeException(nameof(max));

            long _result = max;
            if (duration == 0) return _result;

            _result = (long)((float)max / duration);
            if (_result <= 0) _result = max;
            return _result;
        }

        public static Vector2 GetCanvasPosition(Vector3 worldPos, RectTransform canvasRect, Camera camera)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            if (canvasRect == null) throw new ArgumentNullException(nameof(canvasRect));

            var _viewportPos = camera.WorldToViewportPoint(worldPos);
            Vector2 _screenPos = new Vector2(
                (_viewportPos.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                (_viewportPos.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
            return _screenPos;
        }

        // dependency 이슈로 인해 게임 모듈에서 처리
        // public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
        // {
        //     return DG.Tweening.DOTweenModuleUI.Utils.SwitchToRectTransform(from, to);
        // }

        // 변형된 형태가 필요하여 DG.Tweening.DOTweenModuleUI.Utils.SwitchToRectTransform 이용하여 만듬
        public static Vector2 SwitchToRectTransform(Rect fromRect, Vector3 fromPosition, RectTransform to)
        {
            Vector2 _fromPivotDerivedOffset = new Vector2(fromRect.width * 0.5f + fromRect.xMin, fromRect.height * 0.5f + fromRect.yMin);
            Vector2 _screenP = RectTransformUtility.WorldToScreenPoint(null, fromPosition);
            _screenP += _fromPivotDerivedOffset;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(to, _screenP, null, out var _localPoint);
            Vector2 _pivotDerivedOffset = new Vector2(to.rect.width * 0.5f + to.rect.xMin, to.rect.height * 0.5f + to.rect.yMin);
            return to.anchoredPosition + _localPoint - _pivotDerivedOffset;
        }

        public static void SetLayerMaskAndSortingOrder(SpriteRenderer spriteRenderer, string layerName, int sortingOrder)
        {
            spriteRenderer.gameObject.layer = LayerMask.NameToLayer(layerName);
            spriteRenderer.sortingLayerName = layerName;
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}