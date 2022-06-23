// using Com.TheFallenGames.OSA.CustomAdapters.GridView;
// using UnityEngine;

// namespace Helen
// {
//     public interface IGenericOSAGridImpl
//     {
//         BaseCellViewsHolder GetNewCellViewsHolder();

//         void OnCellActive(BaseCellViewsHolder holder);

//         void OnCellDeactive(BaseCellViewsHolder holder);
//     }

//     public class GenericCellGroupViewsHolder : CellGroupViewsHolder<BaseCellViewsHolder>
//     {
//         private GenericOSAGrid genericGrid;

//         public GenericCellGroupViewsHolder(GenericOSAGrid genericGrid) : base()
//         {
//             this.genericGrid = genericGrid;
//         }

//         protected override BaseCellViewsHolder GetNewCellViewsHolders()
//         {
//             if (genericGrid && genericGrid.gridImpl != null)
//                 return genericGrid.gridImpl.GetNewCellViewsHolder();
//             return null;
//         }

//         public override void CollectViews()
//         {
//             base.CollectViews();

//             for (int i = 0; i < _Capacity; ++i)
//             {
//                 if (genericGrid.Parameters.SetForceItemSizeDelta)
//                     ContainingCellViewsHolders[i].root.sizeDelta = genericGrid.Parameters.itemSizeDelta;
//             }
//         }

//         public override int NumActiveCells
//         {
//             get { return _NumActiveCells; }
//             set
//             {
//                 if (_NumActiveCells != value || genericGrid.UseDynamicLayout)
//                 {
//                     _NumActiveCells = value;

//                     for (int i = 0; i < _Capacity; ++i)
//                     {
//                         var cellVH = ContainingCellViewsHolders[i];
//                         var viewsGO = cellVH.views.gameObject;
//                         var active = i < _NumActiveCells;
//                         if (viewsGO.activeSelf != active)
//                             viewsGO.SetActive(active);

//                         bool isLayoutActive = true;

//                         if (genericGrid.UseDynamicLayout && !active)
//                             isLayoutActive = (genericGrid.CellsCount >= _Capacity);

//                         if (cellVH.rootLayoutElement.ignoreLayout == isLayoutActive)
//                             cellVH.rootLayoutElement.ignoreLayout = !isLayoutActive;
//                     }
//                 }
//             }
//         }
//     }

//     public class GenericOSAGrid : GridAdapter<GridParams, BaseCellViewsHolder>
//     {
//         public bool UseDynamicLayout = false;

//         internal IGenericOSAGridImpl gridImpl;

//         public void Init(IGenericOSAGridImpl impl, GameObject prefab = null)
//         {
//             gridImpl = impl;

//             if (prefab)
//                 SetCellPrefab(prefab.transform as RectTransform);

//             Init();
//         }

//         public void SetCellPrefab(RectTransform prefafb)
//         {
//             Parameters.Grid.CellPrefab = prefafb;
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

//         protected override CellGroupViewsHolder<BaseCellViewsHolder> GetNewCellGroupViewsHolder()
//         {
//             return new GenericCellGroupViewsHolder(this);
//         }

//         protected override void UpdateCellViewsHolder(BaseCellViewsHolder newOrRecycled)
//         {
//             if (gridImpl != null)
//                 gridImpl.OnCellActive(newOrRecycled);
//         }

//         protected override void OnBeforeRecycleOrDisableCellViewsHolder(BaseCellViewsHolder inRecycleBinOrVisible, int newItemIndex)
//         {
//             base.OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible, newItemIndex);

//             if (gridImpl != null)
//             {
//                 gridImpl.OnCellDeactive(inRecycleBinOrVisible);

//                 inRecycleBinOrVisible.root.gameObject.SetActive(true);
//             }
//         }

//         public void ScheduleComputeVisibility()
//         {
//             ScheduleComputeVisibilityTwinPass(true);
//         }

// #if UNITY_EDITOR

//         [ContextMenu("Convert Grid To OSA")]
//         public void ConvertUnityGridToOSA()
//         {
//             var gridLayoutGroup = GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>(true);
//             if (!gridLayoutGroup)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Layout Group is null.",
//                     "참고할 Grid Layout Group이 없습니다.",
//                     "확인"))
//                     return;
//             }

//             if (gridLayoutGroup.constraintCount == 1)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "constraintCount is 1.",
//                     "constraintCount가 1입니다. GenericOSAScrollRect를 사용하세요.",
//                     "확인"))
//                     return;
//             }

//             var scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();
//             if (!scrollRect)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "scroll rect null.",
//                     "참고할 scroll rect가 없습니다.",
//                     "확인"))
//                     return;
//             }

//             if (scrollRect)
//             {
//                 _Params.Viewport = scrollRect.viewport;
//                 _Params.Content = scrollRect.content;

//                 if (scrollRect.horizontal)
//                     _Params.Orientation = Com.TheFallenGames.OSA.Core.BaseParams.OrientationEnum.HORIZONTAL;
//                 if (scrollRect.vertical)
//                     _Params.Orientation = Com.TheFallenGames.OSA.Core.BaseParams.OrientationEnum.VERTICAL;
//             }

//             if (!Viewport)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Viewport null.",
//                     "ScrollRect에 Viewport가 연결되어 있지 않습니다.",
//                     "확인"))
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
//                     "ScrollRect에 Content가 연결되어 있지 않습니다.",
//                     "확인"))
//                     return;
//             }

//             if (!Parameters.Grid.CellPrefab)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "CellPrefab null.",
//                     "GenericOSAGrid에 CellPrefab이 연결되어 있지 않습니다.",
//                     "확인"))
//                     return;
//             }

//             if (!Parameters.Grid.CellPrefab.GetComponent<UnityEngine.UI.LayoutElement>())
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "LayoutElement null.",
//                     $"CellPrefab({Parameters.Grid.CellPrefab.name})에 LayoutElemt가 없습니다.",
//                     "확인"))
//                     return;
//             }

//             if (Parameters.Grid.CellPrefab.childCount != 1)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "View error.",
//                     $"CellPrefab({Parameters.Grid.CellPrefab.name}) 하위에는 하나의 뷰 오브젝트만 존재 해야 하며, 뷰 안에 오브젝트들이 배치되어 있어야 합니다.",
//                     "확인"))
//                     return;
//             }

//             var contentSizeFitter = GetComponentInChildren<UnityEngine.UI.ContentSizeFitter>(true);
//             if (contentSizeFitter)
//                 contentSizeFitter.enabled = false;

//             scrollRect.enabled = false;
//             gridLayoutGroup.enabled = false;

//             if (gridLayoutGroup.constraint == UnityEngine.UI.GridLayoutGroup.Constraint.Flexible)
//             {
//                 Parameters.Grid.MaxCellsPerGroup = -1;
//                 Parameters.Grid.DynamicMaxCellsPerGroup = -1;
//             }
//             else
//             {
//                 Parameters.Grid.DynamicMaxCellsPerGroup = gridLayoutGroup.constraintCount;
//                 Parameters.Grid.MaxCellsPerGroup = gridLayoutGroup.constraintCount;
//             }

//             Parameters.ContentSpacing = Parameters.IsHorizontal ?
//                 gridLayoutGroup.spacing.x : gridLayoutGroup.spacing.y;

//             Parameters.Grid.SpacingInGroup = Parameters.IsHorizontal ?
//                 gridLayoutGroup.spacing.y : gridLayoutGroup.spacing.x;

//             Parameters.SetForceItemSizeDelta = true;
//             Parameters.itemSizeDelta = gridLayoutGroup.cellSize;

//             Parameters.Grid.CellWidthForceExpandInGroup = false;
//             Parameters.Grid.CellHeightForceExpandInGroup = false;

//             UseDynamicLayout = (
//                 Parameters.Grid.AlignmentOfCellsInGroup == TextAnchor.LowerCenter ||
//                 Parameters.Grid.AlignmentOfCellsInGroup == TextAnchor.MiddleCenter ||
//                 Parameters.Grid.AlignmentOfCellsInGroup == TextAnchor.UpperCenter);

//             Parameters.ContentPadding.top = gridLayoutGroup.padding.top;
//             Parameters.ContentPadding.bottom = gridLayoutGroup.padding.bottom;
//             Parameters.ContentPadding.left = 0;
//             Parameters.ContentPadding.right = 0;

//             Parameters.Grid.GroupPadding.top = 0;
//             Parameters.Grid.GroupPadding.bottom = 0;
//             Parameters.Grid.GroupPadding.left = gridLayoutGroup.padding.left;
//             Parameters.Grid.GroupPadding.right = gridLayoutGroup.padding.right;
//             Parameters.Grid.AlignmentOfCellsInGroup = gridLayoutGroup.childAlignment;

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
//                     "Layout Group이 없습니다.",
//                     "확인"))
//                     return;
//             }

//             if (!Content)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "Content null.",
//                     "GenericOSAGrid에 Content가 연결되어 있지 않습니다.",
//                     "확인"))
//                     return;
//             }

//             if (!Parameters.Grid.CellPrefab)
//             {
//                 if (UnityEditor.EditorUtility.DisplayDialog(
//                     "CellPrefab null.",
//                     "GenericOSAGrid에 CellPrefab이 연결되어 있지 않습니다.",
//                     "확인"))
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
//                 GameObject.Instantiate(Parameters.Grid.CellPrefab, Content);

//             UnityEditor.EditorUtility.SetDirty(this);
//         }

// #endif
//     }

//     public class BaseCellViewsHolder : CellViewsHolder
//     {
//     }
// }