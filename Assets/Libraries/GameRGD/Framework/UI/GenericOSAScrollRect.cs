// using Com.TheFallenGames.OSA.Core;
// using Com.TheFallenGames.OSA.CustomParams;
// using UnityEngine;

// namespace Helen
// {
//     public interface IGenericOSAScrollRectImpl
//     {
//         BaseItemViewsHolder GetNewViewsHolder();

//         void OnCellActive(BaseItemViewsHolder holder);

//         void OnCellDeactive(BaseItemViewsHolder holder);
//     }

//     public class GenericOSAScrollRect : OSA<BaseParamsWithPrefab, BaseItemViewsHolder>
//     {
//         private IGenericOSAScrollRectImpl scrollRectImpl;

//         public void Init(IGenericOSAScrollRectImpl impl)
//         {
//             scrollRectImpl = impl;

//             Init();
//         }

//         public void Refresh(int count, bool resetScroll = true)
//         {
//             ResetItems(count);

//             if (resetScroll)
//             {
//                 if (_Params.IsHorizontal)
//                     SetNormalizedPosition(0f);
//                 else
//                     SetNormalizedPosition(1f);
//             }
//         }

//         public void Terminate()
//         {
//             ResetItems(0);
//         }

//         protected override BaseItemViewsHolder CreateViewsHolder(int itemIndex)
//         {
//             if (scrollRectImpl != null)
//             {
//                 var instance = scrollRectImpl.GetNewViewsHolder();
//                 if (instance != null)
//                 {
//                     instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

//                     if (Parameters.SetForceItemSizeDelta)
//                         instance.root.sizeDelta = Parameters.itemSizeDelta;
//                 }

//                 return instance;
//             }

//             return null;
//         }

//         protected override void UpdateViewsHolder(BaseItemViewsHolder newOrRecycled)
//         {
//             if (scrollRectImpl != null)
//                 scrollRectImpl.OnCellActive(newOrRecycled);
//         }

//         protected override void OnBeforeRecycleOrDisableViewsHolder(BaseItemViewsHolder inRecycleBinOrVisible, int newItemIndex)
//         {
//             base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);

//             if (scrollRectImpl != null)
//                 scrollRectImpl.OnCellDeactive(inRecycleBinOrVisible);
//         }

//         public void ScheduleComputeVisibility()
//         {
//             ScheduleComputeVisibilityTwinPass(true);
//         }

// #if UNITY_EDITOR
//         [ContextMenu("Convert Grid To OSA")]
//         public void ConvertUnityGridToOSA()
//         {
//             var layoutGroup = GetComponentInChildren<UnityEngine.UI.LayoutGroup>(true);
//             if (!layoutGroup)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Layout Group is null.",
//                     "?????? Horizontal, ???? Vertical Layout Group?? ????????.",
//                     "????"))
//                     return;
//             }

//             var scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();
//             if (scrollRect)
//             {
//                 _Params.Viewport = scrollRect.viewport;
//                 _Params.Content = scrollRect.content;

//                 if (scrollRect.horizontal)
//                     _Params.Orientation = BaseParams.OrientationEnum.HORIZONTAL;
//                 if (scrollRect.vertical)
//                     _Params.Orientation = BaseParams.OrientationEnum.VERTICAL;
//             }

//             if (!Viewport)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Viewport null.",
//                     "ScrollRect?? Viewport?? ???????? ???? ????????.",
//                     "????"))
//                     return;
//             }

//             var mask = Viewport.GetComponent<UnityEngine.UI.Mask>();
//             if (mask)
//                 DestroyImmediate(mask);
//             Viewport.GetOrAddComponent<UnityEngine.UI.RectMask2D>();

//             if (!Content)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Content null.",
//                     "ScrollRect?? Content?? ???????? ???? ????????.",
//                     "????"))
//                     return;
//             }

//             if (!Parameters.ItemPrefab)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "CellPrefab null.",
//                     "GenericOSAScrollRect?? ItemPrefab?? ???????? ???? ????????.",
//                     "????"))
//                     return;
//             }

//             var contentSizeFitter = GetComponentInChildren<UnityEngine.UI.ContentSizeFitter>(true);
//             if (contentSizeFitter)
//                 contentSizeFitter.enabled = false;
//             scrollRect.enabled = false;
//             layoutGroup.enabled = false;

//             Parameters.SetForceItemSizeDelta = false;
//             Parameters.itemSizeDelta = Vector2.zero;

//             if (Parameters.IsHorizontal)
//             {
//                 Parameters.ContentPadding.left = layoutGroup.padding.left;
//                 Parameters.ContentPadding.right = layoutGroup.padding.right;
//                 Parameters.ContentPadding.top = -1;
//                 Parameters.ContentPadding.bottom = -1;
//             }
//             else
//             {
//                 Parameters.ContentPadding.left = -1;
//                 Parameters.ContentPadding.right = -1;
//                 Parameters.ContentPadding.top = layoutGroup.padding.top;
//                 Parameters.ContentPadding.bottom = layoutGroup.padding.bottom;
//             }

//             Parameters.ItemTransversalSize = -1;
//             Parameters.AlsoControlItemTransversalSize = false;

//             if (layoutGroup is UnityEngine.UI.GridLayoutGroup gridLayoutGroup)
//             {
//                 Parameters.SetForceItemSizeDelta = true;
//                 Parameters.itemSizeDelta = gridLayoutGroup.cellSize;

//                 Parameters.ContentSpacing = (Parameters.IsHorizontal) ?
//                     gridLayoutGroup.spacing.x : gridLayoutGroup.spacing.y;
//             }
//             else if (layoutGroup is UnityEngine.UI.HorizontalLayoutGroup horizontallayoutGroup)
//             {
//                 Parameters.ContentSpacing = horizontallayoutGroup.spacing;

//                 if (horizontallayoutGroup.childControlHeight)
//                 {
//                     Parameters.ContentPadding.top = layoutGroup.padding.top;
//                     Parameters.ContentPadding.bottom = layoutGroup.padding.bottom;
//                     Parameters.ItemTransversalSize = 0;
//                     Parameters.AlsoControlItemTransversalSize = false;
//                 }
//             }
//             else if (layoutGroup is UnityEngine.UI.VerticalLayoutGroup verticallayoutGroup)
//             {
//                 Parameters.ContentSpacing = verticallayoutGroup.spacing;

//                 if (verticallayoutGroup.childControlWidth)
//                 {
//                     Parameters.ContentPadding.left = layoutGroup.padding.left;
//                     Parameters.ContentPadding.right = layoutGroup.padding.right;
//                     Parameters.ItemTransversalSize = 0;
//                     Parameters.AlsoControlItemTransversalSize = false;
//                 }
//             }

//             int childCount = Content.transform.childCount;
//             for (int i = childCount - 1; i >= 0; --i)
//                 GameObject.DestroyImmediate(Content.GetChild(i).gameObject);

//             UnityEditor.EditorUtility.SetDirty(this);
//         }

//         [ContextMenu("Convert OSA To Grid")]
//         public void ConvertOSAToGrid()
//         {
//             var layoutGroup = GetComponentInChildren<UnityEngine.UI.LayoutGroup>(true);
//             if (!layoutGroup)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Layout Group is null.",
//                     "Layout Group?? ????????.",
//                     "????"))
//                     return;
//             }

//             if (!Content)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Content null.",
//                     "GenericOSAGrid?? Content?? ???????? ???? ????????.",
//                     "????"))
//                     return;
//             }

//             if (!Parameters.ItemPrefab)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "CellPrefab null.",
//                     "GenericOSAScrollRect?? ItemPrefab?? ???????? ???? ????????.",
//                     "????"))
//                     return;
//             }

//             var scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();
//             if (scrollRect)
//                 scrollRect.enabled = true;
//             var contentSizeFilter = GetComponentInChildren<UnityEngine.UI.ContentSizeFitter>(true);
//             if (contentSizeFilter)
//                 contentSizeFilter.enabled = true;
//             layoutGroup.enabled = true;

//             int childCount = Content.transform.childCount;
//             for (int i = childCount - 1; i >= 0; --i)
//                 GameObject.DestroyImmediate(Content.GetChild(i).gameObject);

//             for (int i = 0; i < 20; i++)
//                 GameObject.Instantiate(Parameters.ItemPrefab, Content);

//             UnityEditor.EditorUtility.SetDirty(this);
//         }
// #endif
//     }
// }