using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    public class PanAndZoomObserver : MonoBehaviour
    {
        private Subject<Vector2> m_OnSwipeStream = new Subject<Vector2>();
        public IObservable<Vector2> OnSwipeAsObservable() => m_OnSwipeStream.AsObservable();

        private Subject<float> m_OnPinchStream = new Subject<float>();
        public IObservable<float> OnPinchAsObservable
        {
            get { return m_OnPinchStream.AsObservable(); }
        }

        private PointerEventData m_UIPointerEventData;
        private GraphicRaycaster[] m_UIRaycasters;
        private EventSystem m_EventSystem;

        void Start()
        {
            m_UIRaycasters = GameObject.FindObjectsOfType<GraphicRaycaster>();
            m_EventSystem = GameObject.FindObjectOfType<EventSystem>();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0)) { }
            else if (Input.GetMouseButton(0))
            {
                MapSwipe();
            }
            else if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0f)
            {
                MapPinch();
            }
#elif UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 0) {}
            else  if (Input.touchCount == 1)
            {
                MapSwipe();
            }
            else  if (Input.touchCount >= 2)
            {
                MapPinch();
            }
#endif
        }

        private bool IsAllowActive()
        {
            m_UIPointerEventData = new PointerEventData(m_EventSystem);
            m_UIPointerEventData.position = Input.mousePosition;
            int blockCount = 0;
            foreach (GraphicRaycaster raycaster in m_UIRaycasters)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(m_UIPointerEventData, results);
                blockCount += results.Count;
            }

            return blockCount == 0;
        }

        private void MapSwipe()
        {
            if (IsAllowActive() == false) return;

#if UNITY_EDITOR
            var drag = Observable.EveryUpdate().Select(pos => Input.mousePosition);
            var stop = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonUp(0)).First();
#elif UNITY_IOS || UNITY_ANDROID
            var drag = Observable.EveryUpdate().Where(_ => Input.touchCount == 1).Select(pos => Input.GetTouch(0).position);
            var stop = Observable.EveryUpdate().Where(_ => Input.touchCount != 1).First();
#else
            var drag = Observable.EveryUpdate().Select(pos => Input.mousePosition);
            var stop = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonUp(0)).First();
#endif

            IDisposable onDrag = drag.Buffer(3)
                .TakeUntil(stop)
                .Subscribe(colPos =>
                {
                    Vector3 delta = colPos.First() - colPos.Last();
                    m_OnSwipeStream.OnNext(delta);
                }).AddTo(this);
        }

        private void MapPinch()
        {
            if (IsAllowActive() == false) return;

#if UNITY_EDITOR
            var pinch = Observable.EveryUpdate().Select(pos_dist => Input.GetAxis("Mouse ScrollWheel"));
#elif UNITY_IOS || UNITY_ANDROID
            var pinch = Observable.EveryUpdate().Where(_ => Input.touchCount >= 2)
                .Select(pos_dist => Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position));
            var stop = Observable.EveryUpdate().Where(_ => Input.touchCount != 2).First();
#else
            var pinch = Observable.EveryUpdate().Select(pos_dist => Input.mousePosition.x);
            var stop = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonUp(0)).First();
#endif

#if UNITY_EDITOR
            IDisposable onPinch = pinch
                .Subscribe(dist =>
                {
                    if (Mathf.Abs(dist) > 0f) m_OnPinchStream.OnNext(dist * 5f);
                }).AddTo(this);
#elif UNITY_IOS || UNITY_ANDROID
            IDisposable onPinch = pinch.Buffer(3)
                .TakeUntil(stop)
                .Subscribe(dist =>
                {
                    float diff = dist.Last() - dist.First();
                    if (Mathf.Abs(diff) > 0f) m_OnPinchStream.OnNext(diff);
                }).AddTo(this);
#endif
        }
    }
}