// using Com.TheFallenGames.OSA.Core;
// using System;

// namespace Helen
// {
//     public class GenericOSAScrollRectDelegate 
//         : IGenericOSAScrollRectImpl
//     {
//         private Func<BaseItemViewsHolder> getNewViewsHolder;
//         private Action<BaseItemViewsHolder> onCellActive;
//         private Action<BaseItemViewsHolder> onCellDeactive;
//         public GenericOSAScrollRectDelegate(
//             Func<BaseItemViewsHolder> getNewViewsHolder,
//             Action<BaseItemViewsHolder> onCellActive,
//             Action<BaseItemViewsHolder> onCellDeactive)
//         {
//             this.getNewViewsHolder = getNewViewsHolder;
//             this.onCellActive = onCellActive;
//             this.onCellDeactive = onCellDeactive;
//         }

//         BaseItemViewsHolder IGenericOSAScrollRectImpl.GetNewViewsHolder()
//         {
//             return getNewViewsHolder?.Invoke();
//         }

//         void IGenericOSAScrollRectImpl.OnCellActive(BaseItemViewsHolder holder)
//         {
//             onCellActive?.Invoke(holder);
//         }

//         void IGenericOSAScrollRectImpl.OnCellDeactive(BaseItemViewsHolder holder)
//         {
//             onCellDeactive?.Invoke(holder);
//         }
//     }
// }

// namespace Helen
// {
//     public class GenericOSAGridDelegate
//         : IGenericOSAGridImpl
//     {
//         private Func<BaseCellViewsHolder> getNewViewsHolder;
//         private Action<BaseCellViewsHolder> onCellActive;
//         private Action<BaseCellViewsHolder> onCellDeactive;

//         public GenericOSAGridDelegate(
//             Func<BaseCellViewsHolder> getNewViewsHolder,
//             Action<BaseCellViewsHolder> onCellActive,
//             Action<BaseCellViewsHolder> onCellDeactive)
//         {
//             this.getNewViewsHolder = getNewViewsHolder;
//             this.onCellActive = onCellActive;
//             this.onCellDeactive = onCellDeactive;
//         }

//         BaseCellViewsHolder IGenericOSAGridImpl.GetNewCellViewsHolder()
//         {
//             return getNewViewsHolder?.Invoke();
//         }

//         void IGenericOSAGridImpl.OnCellActive(BaseCellViewsHolder holder)
//         {
//             onCellActive?.Invoke(holder);
//         }

//         void IGenericOSAGridImpl.OnCellDeactive(BaseCellViewsHolder holder)
//         {
//             onCellDeactive?.Invoke(holder);
//         }
//     }
// }