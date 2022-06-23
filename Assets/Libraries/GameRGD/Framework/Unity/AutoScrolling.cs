using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class AutoScrolling : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float Speed = 100.0f;
    public float StartWithDelay = 1.0f;
    public bool Inertia = false;

    private bool pause;
    private float waitTime;
    private ScrollRect scrollRect;

    private Action result;

    public void Init(Action result)
    {
        if(scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1.0f;
        waitTime = StartWithDelay;

        this.result = result;
    }


    private void Update()
    {
        if (waitTime > 0.0f)
        {
            waitTime -= Time.deltaTime;
        }
        else
        {
            if (scrollRect == null || !scrollRect.isActiveAndEnabled || pause)
                return;

            if (scrollRect.vertical)
            {
                if (scrollRect.verticalNormalizedPosition <= 0.0f)
                {
                    if(result != null)
                    {
                        result();
                        result = null;
                    }
                    return;
                }

                if (Inertia && scrollRect.velocity.y > Speed)
                    return;

                float seconds = scrollRect.content.rect.height / Speed;
                scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(scrollRect.verticalNormalizedPosition - (Time.deltaTime / seconds));
                if (scrollRect.verticalNormalizedPosition <= 0.00001f)
                    scrollRect.verticalNormalizedPosition = -0.01f;
            }
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        pause = false;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        pause = true;
    }
}