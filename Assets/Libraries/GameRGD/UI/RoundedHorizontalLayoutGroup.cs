using UnityEngine;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    [AddComponentMenu("Layout/Rounded Horizontal Layout Group", 153)]
    public class RoundedHorizontalLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        [SerializeField] public float m_VerticalGap;
        
	    public float verticalGap { get { return m_VerticalGap; } set { SetProperty (ref m_VerticalGap, value); } }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, false);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, false);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, false);
        }

        protected new void CalcAlongAxis(int axis, bool isVertical)
        {
            if (transform.childCount == 0)
                return;

            float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);

            float totalMin = combinedPadding;
            float totalPreferred = combinedPadding;
            float totalFlexible = 0;

            bool alongOtherAxis = (isVertical ^ (axis == 1));
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float min = LayoutUtility.GetMinSize(child, axis);
                float preferred = LayoutUtility.GetPreferredSize(child, axis);
                float flexible = LayoutUtility.GetFlexibleSize(child, axis);
                if ((axis == 0 ? childForceExpandWidth : childForceExpandHeight))
                    flexible = Mathf.Max(flexible, 1);

                if (alongOtherAxis)
                {
                    totalMin = Mathf.Max(min + combinedPadding, totalMin);
                    totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
                    totalFlexible = Mathf.Max(flexible, totalFlexible);
                }
                else
                {
                    totalMin += min + spacing;
                    totalPreferred += preferred + spacing;

                    // Increment flexible size with element's flexible size.
                    totalFlexible += flexible;
                }
            }

            if (!alongOtherAxis && rectChildren.Count > 0)
            {
                totalMin -= spacing;
                totalPreferred -= spacing;
            }
            totalPreferred = Mathf.Max(totalMin, totalPreferred);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }

        protected new void SetChildrenAlongAxis(int axis, bool isVertical)
        {
            if (transform.childCount == 0)
                return;

            float size = rectTransform.rect.size[axis];

            bool alongOtherAxis = (isVertical ^ (axis == 1));
            if (alongOtherAxis)
            {
                float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);
                for (int i = 0, len = rectChildren.Count; i < len; i++)
                {
                    RectTransform child = rectChildren[i];
                    float min = LayoutUtility.GetMinSize(child, axis);
                    float preferred = LayoutUtility.GetPreferredSize(child, axis);
                    float flexible = LayoutUtility.GetFlexibleSize(child, axis);
                    if ((axis == 0 ? childForceExpandWidth : childForceExpandHeight))
                        flexible = Mathf.Max(flexible, 1);

                    float requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);
                    float startOffset = GetStartOffset(axis, requiredSpace);
                    if (axis == 1)
                    {
                        int mid = ((len - 1) / 2);
                        startOffset += verticalGap * -Mathf.Abs((i - mid - (i > mid ? (len - 1) % 2 : 0)));
                    }
                    SetChildAlongAxis(child, axis, startOffset, requiredSpace);
                }
            }
            else
            {
                float pos = (axis == 0 ? padding.left : padding.top);
                if (GetTotalFlexibleSize(axis) == 0 && GetTotalPreferredSize(axis) < size)
                    pos = GetStartOffset(axis, GetTotalPreferredSize(axis) - (axis == 0 ? padding.horizontal : padding.vertical));

                float minMaxLerp = 0;
                if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
                    minMaxLerp = Mathf.Clamp01((size - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));

                float itemFlexibleMultiplier = 0;
                if (size > GetTotalPreferredSize(axis))
                {
                    if (GetTotalFlexibleSize(axis) > 0)
                        itemFlexibleMultiplier = (size - GetTotalPreferredSize(axis)) / GetTotalFlexibleSize(axis);
                }

                for (int i = 0; i < rectChildren.Count; i++)
                {
                    RectTransform child = rectChildren[i];
                    float min = LayoutUtility.GetMinSize(child, axis);
                    float preferred = LayoutUtility.GetPreferredSize(child, axis);
                    float flexible = LayoutUtility.GetFlexibleSize(child, axis);
                    if ((axis == 0 ? childForceExpandWidth : childForceExpandHeight))
                        flexible = Mathf.Max(flexible, 1);

                    float childSize = Mathf.Lerp(min, preferred, minMaxLerp);
                    childSize += flexible * itemFlexibleMultiplier;
                    SetChildAlongAxis(child, axis, pos + (axis == 1 ? i * 50 : 0), childSize);
                    pos += childSize + spacing;
                }
            }
        }
    }
}
