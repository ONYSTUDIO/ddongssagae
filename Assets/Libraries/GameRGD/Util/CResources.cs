using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DoubleuGames.GameRGD
{
    public static class CResources
    {
        public static async Task<T> AsyncLoadTask<T>(string addressablesName) where T : UnityEngine.Object
        {
            var operHandle = AsyncLoad<T>(addressablesName);
            while (operHandle.IsDone == false)
            {
                await Task.Yield();
            }
            if (operHandle.OperationException != null)
                throw operHandle.OperationException;
            if (operHandle.Status == AsyncOperationStatus.Failed)
                throw new System.Exception("AsynchronousOperation failed");
            if (operHandle.Result == null)
                throw new System.Exception(string.Format("Resource Empty Path={0}", addressablesName));
            return operHandle.Result;
        }

        public static AsyncOperationHandle<T> AsyncLoad<T>(string addressablesName, Action<AsyncOperationHandle<T>> _event = null) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> asyncoperationhandle = Addressables.LoadAssetAsync<T>($"{addressablesName}");

            if (_event != null)
            {
                asyncoperationhandle.Completed += _event;
            }

            return asyncoperationhandle;
        }

        public static T Load<T>(string path, string addressablesName = null, Action<AsyncOperationHandle<T>> _event = null) where T : UnityEngine.Object
        {
            T result = null;

            if (CPreferences.IsUseAddressable == true && addressablesName != null)
            {
                AsyncOperationHandle<T> asyncoperationhandle = Addressables.LoadAssetAsync<T>($"{addressablesName}");
                asyncoperationhandle.Completed += _event;
                asyncoperationhandle.Completed += (handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Failed)
                    {
                        Debug.LogError(addressablesName + " Load Error");
                    }
                }
                );
                //return Addressables.LoadAssetAsync<T>($"Addressable/{path}").Result;
            }
            else
            {
                result = Resources.Load(path) as T;
            }

            return result;
        }

        public static AsyncOperationHandle<T> AddressableLoad<T>(string addressablesName) where T : UnityEngine.Object
        {
            return Addressables.LoadAssetAsync<T>($"{addressablesName}");
        }
    }
}
