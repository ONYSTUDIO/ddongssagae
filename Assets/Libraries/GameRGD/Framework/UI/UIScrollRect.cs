using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(UIConst.groupName + "/Scroll Rect" + UIConst.suffixUI, 37)]
[SelectionBase]
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class UIScrollRect : UnityEngine.UI.ScrollRect
{
    public bool IsChildVisible(int index)
    {
        if (!content || !viewport)
            return false;

        if (index < 0 || index >= content.childCount)
            return false;

        if (!(content.GetChild(index) is RectTransform cell))
            return false;

        var childCornor = new Vector3[4];
        cell.GetWorldCorners(childCornor);

        var viewPortCornor = new Vector3[4];
        viewport.GetWorldCorners(viewPortCornor);

        var lowerLeft = viewPortCornor[0].x <= childCornor[0].x && viewPortCornor[0].y <= childCornor[0].y;
        var upperLeft = viewPortCornor[1].x <= childCornor[1].x && viewPortCornor[1].y >= childCornor[1].y;
        var upperRight = viewPortCornor[2].x >= childCornor[2].x && viewPortCornor[2].y >= childCornor[2].y;
        var lowerRight = viewPortCornor[3].x >= childCornor[3].x && viewPortCornor[3].y <= childCornor[3].y;

        return lowerLeft && upperLeft && upperRight && lowerRight;
    }

    public void MoveScrollToChild(int index, bool animation = false)
    {
        if (vertical)
        {
            Log.Error("vertical move scroll is not supported yet.");
            return;
        }

        if (!content || !viewport)
            return;

        if (index == 0)
        {
            MoveScrollTo(0f, animation);
            return;
        }
        else if (index == content.childCount - 1)
        {
            MoveScrollTo(1f, animation);
            return;
        }

        var spacing = 0f;
        var padding = 0f;
        var startOffset = 0f;
        var contentsWidth = 0f;

        var hOrVLayoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (hOrVLayoutGroup)
        {
            spacing = hOrVLayoutGroup.spacing;
            padding = hOrVLayoutGroup.padding.horizontal;
        }
        else
        {
            var gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup)
            {
                spacing = gridLayoutGroup.spacing.x;
                padding = gridLayoutGroup.padding.horizontal;
            }
        }

        for (int i = 0; i < content.childCount; i++)
        {
            var rectTransform = content.GetChild(i) as RectTransform;
            if (rectTransform)
            {
                contentsWidth += rectTransform.sizeDelta[0];

                if (i < index)
                    startOffset += rectTransform.sizeDelta[0];
            }
        }

        contentsWidth += spacing * (content.childCount - 1) + padding;
        startOffset += spacing * index;

        var viewPortWidth = viewport.rect.width;
        var maxOffset = contentsWidth - viewPortWidth;

        MoveScrollTo(startOffset / maxOffset, animation);
    }

    private void MoveScrollTo(float value, bool animation = false)
    {
        if (coMoveScrollToRef != null)
            StopCoroutine(coMoveScrollToRef);

        if (movementType != MovementType.Unrestricted)
            value = Mathf.Clamp01(value);

        if (animation)
            coMoveScrollToRef = StartCoroutine(CoMoveScrollTo(value));
        else
        {
            horizontalNormalizedPosition = value;
            StopMovement();
        }
    }

    private Coroutine coMoveScrollToRef = null;

    private IEnumerator CoMoveScrollTo(float value)
    {
        var tween = DOTween.To(
            () => horizontalNormalizedPosition,
            t => horizontalNormalizedPosition = t,
            value,
            0.5f).WaitForCompletion();
        yield return tween;

        horizontalNormalizedPosition = value;
    }
}