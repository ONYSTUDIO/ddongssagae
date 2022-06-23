// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Com.TheFallenGames.OSA.Core;
// using Com.TheFallenGames.OSA.CustomParams;
// using Cysharp.Threading.Tasks;

// namespace Helen
// {
//     public interface ICustomOSAScrollRectImpl
//     {
//         BaseItemViewsHolder GetNewViewsHolder();

//         void OnCellActive(BaseItemViewsHolder holder);

//         void OnCellDeactive(BaseItemViewsHolder holder);
//     }

//     public class CustomOSAScrollRect : OSA<BaseParamsWithPrefab, BaseItemViewsHolder>
//     {
//         private ICustomOSAScrollRectImpl scrollRectImpl;

//         public void Init(ICustomOSAScrollRectImpl impl)
//         {
//             scrollRectImpl = impl;

//             Init();
//         }

//         public void Refresh(int count, float position = -50f)
//         {
//             ResetItems(count);

//             if(position > -10f)
//             {
//                 //리셋 후, 바로 맨 밑으로 내려버리면 약간 위로 올라가는 증상이 존재함.
//                 SetNormalizedPosition(position);
//                 SetNormalizedPosition(position);
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
//                     instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
//                 return instance;
//             }

//             return null;
//         }

//         protected override void UpdateViewsHolder(BaseItemViewsHolder newOrRecycled)
//         {
//             if (scrollRectImpl != null)
//                 scrollRectImpl.OnCellActive(newOrRecycled);

//             ScheduleComputeVisibilityTwinPass(true);
//         }

//         protected override void OnBeforeRecycleOrDisableViewsHolder(BaseItemViewsHolder inRecycleBinOrVisible, int newItemIndex)
//         {
//             base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);

//             if (scrollRectImpl != null)
//                 scrollRectImpl.OnCellDeactive(inRecycleBinOrVisible);
//         }
//     }

// }

