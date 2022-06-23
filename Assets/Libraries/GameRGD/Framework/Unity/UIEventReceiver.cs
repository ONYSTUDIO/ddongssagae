using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventReceiver
    : MonoBehaviour
    , IPointerClickHandler
    , IPointerDownHandler
    , IPointerUpHandler
    , IDragHandler
{
    private RectTransform rectTF;

    public IObservable<Vector2> OnClickEvent => onClickEvent;
    private Subject<Vector2> onClickEvent = new Subject<Vector2>();

    public IObservable<Vector2> OnDownEvent => onDownEvent;
    private Subject<Vector2> onDownEvent = new Subject<Vector2>();

    public IObservable<Vector2> OnUpEvent => onUpEvent;
    private Subject<Vector2> onUpEvent = new Subject<Vector2>();

    public struct DragInfo
    {
        public Vector2 viewportPoint;
        public Vector2 delta;
    }

    public IObservable<DragInfo> OnDragEvent => onDragEvent;
    private Subject<DragInfo> onDragEvent = new Subject<DragInfo>();

    public struct PinchInfo
    {
        public Vector2 viewportPoint;
        public float delta;
    }

    public IObservable<PinchInfo> OnPinchEvent => onPinchEvent;
    private Subject<PinchInfo> onPinchEvent = new Subject<PinchInfo>();

    private Canvas rootCanvas
    {
        get
        {
            if (!_rootCanvas)
            {
                _rootCanvas = GetComponentInParent<Canvas>();
                _rootCanvas = _rootCanvas.isRootCanvas ? _rootCanvas : _rootCanvas.rootCanvas;
            }
            return _rootCanvas;
        }
    }
    private Canvas _rootCanvas;

    private void Start()
    {
        rectTF = (transform as RectTransform);
        if (rectTF == null)
        {
            this.enabled = false;
            return;
        }
    }

    private void Update()
    {
        ProcessPinch();
    }

    // https://learn.unity.com/tutorial/getting-mobile-input#5c7f8528edbc2a002053b4af
    private void ProcessPinch()
    {
        float retDelta = 0;
        Vector2 retViewport = Vector2.zero;

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.position.x < 0 ||
                touchZero.position.x >= Screen.width ||
                touchZero.position.y < 0 ||
                touchZero.position.y >= Screen.height)
                return;

            if (touchOne.position.x < 0 ||
                touchOne.position.x >= Screen.width ||
                touchOne.position.y < 0 ||
                touchOne.position.y >= Screen.height)
                return;

            if (!RectTransformUtility.RectangleContainsScreenPoint(
                transform as RectTransform, touchZero.position, rootCanvas.worldCamera ?? Camera.main))
                return;

            if (!RectTransformUtility.RectangleContainsScreenPoint(
                transform as RectTransform, touchOne.position, rootCanvas.worldCamera ?? Camera.main))
                return;

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            retDelta = prevTouchDeltaMag - touchDeltaMag;
            retViewport = GetPosition(
                (touchZero.position + touchOne.position) * 0.5f,
                rootCanvas.worldCamera ?? Camera.main);
        }
        else if (Input.mousePresent && RectTransformUtility.RectangleContainsScreenPoint(
                transform as RectTransform, Input.mousePosition, rootCanvas.worldCamera ?? Camera.main))
        {
            retDelta = Input.GetAxis("Mouse ScrollWheel");
            retViewport = GetPosition(
                Input.mousePosition,
                rootCanvas.worldCamera ?? Camera.main);
        }

        if (retDelta != 0)
        {
            PinchInfo newInfo = new PinchInfo()
            {
                viewportPoint = retViewport,
                delta = retDelta
            };
            onPinchEvent.OnNext(newInfo);
        }
    }

    private Vector2 GetPosition(Vector2 screenPosition, Camera eventCamera)
    {
        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (transform as RectTransform), screenPosition, eventCamera, out pos);

        var rect = rectTF.rect;
        pos.x = (pos.x - rect.xMin) / rect.width;
        pos.y = (pos.y - rect.yMin) / rect.height;
        return pos;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        DragInfo newInfo = new DragInfo()
        {
            viewportPoint = GetPosition(eventData.position, eventData.pressEventCamera),
            delta = eventData.delta
        };
        onDragEvent.OnNext(newInfo);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        onClickEvent.OnNext(GetPosition(eventData.position, eventData.pressEventCamera));
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        onDownEvent.OnNext(GetPosition(eventData.position, eventData.pressEventCamera));
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        onUpEvent.OnNext(GetPosition(eventData.position, eventData.pressEventCamera));
    }
}