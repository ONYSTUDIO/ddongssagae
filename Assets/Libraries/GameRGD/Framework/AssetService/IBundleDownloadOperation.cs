using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;
using System.Collections.Generic;

using UniRx;
using System.IO;

namespace Helen
{
    public static class AssetServiceNetworkErrorHolder
    {
        private static ReactiveProperty<bool> existError = new ReactiveProperty<bool>();
        public static string ErrorOwnerKey { get; private set; }

        public static IReactiveProperty<bool> ErrorAwaiter { get => existError; }
        public static void SetError(string owner) { existError.Value = true; ErrorOwnerKey = owner; }
        public static void Clear() { existError.Value = false; ErrorOwnerKey = string.Empty; }
        public static bool HasError() { return existError.Value; }
        public static bool IsOwner(string owner) { return ErrorOwnerKey == owner; }
    }

    // 에셋번들 다운로드 명령
    internal interface IBundleDownloadOperation
    {
        int Key { get; }
        long SizeKBytes { get; }
        long CurrentSizeKBytes { get; }
        int Count { get; }
        int CurrentCount { get; }
        bool IsDone { get; }
        bool IsSucceeded { get; }
        bool IsMainAsset { get; }
        AssetLoadHandle Handle { get; }
        List<AsyncOperationHandle> AddressableHandles { get; }
        AsyncOperationHandle MainAddressableHandle { get; }
        AssetLoadHandle CreateExternalHandle(UniTask task);
        void ReleaseAddressableHandle();

        bool ApplicationBundle { get; set; }
    }

    internal abstract class BundleDownloadTypeOperation<TObject> : IBundleDownloadOperation
    {
        public abstract int Key { get; protected set; }
        public abstract long SizeKBytes { get; protected set; }
        public abstract long CurrentSizeKBytes { get; protected set; }
        public abstract int Count { get; protected set; }
        public abstract int CurrentCount { get; protected set; }
        public abstract bool IsDone { get; protected set; }
        public abstract bool IsSucceeded { get; protected set; }
        public virtual bool IsMainAsset { get { return true; } protected set { } }
        public AssetLoadHandle Handle { get; protected set; }
        public abstract List<AsyncOperationHandle> AddressableHandles { get; }
        // protected DownloadingOverlayBarMediator downloadUI;
        private bool isBattlePausedOnDownload = false;

        public virtual AsyncOperationHandle MainAddressableHandle
        {
            get
            {
                return AddressableHandles.Count > 0 ? AddressableHandles[0] : default;
            }
        }

        public bool ApplicationBundle { get; set; } = false;

        public virtual AssetLoadHandle CreateExternalHandle(UniTask task)
        {
            return Handle = new AssetLoadHandle(this, task);
        }
        public abstract void ReleaseAddressableHandle();

        protected Action<TObject> completed;
        public void AddCompleted(Action<TObject> inCompleted)
        {
            if (null != inCompleted)
                completed += inCompleted;
        }
        public virtual void Completed()
        {
            completed?.Invoke((TObject)MainAddressableHandle.Result);
            completed = null;
        }

        // protected void EnableBundleDownloadOverlayUI()
        // {
        //     downloadUI = UIService.DownloadingOverlayBar(true);

        //     if (false == isBattlePausedOnDownload)
        //     {
        //         BattleContent battleContent = Context.Container.Resolve<BattleContent>();
        //         if (null != battleContent)
        //         {
        //             isBattlePausedOnDownload = true;
        //             battleContent.SetPause(true);
        //         }
        //     }
        // }

        // protected void DisableBundleDownloadOverlayUI()
        // {
        //     UIService.DownloadingOverlayBar(false);

        //     if (true == isBattlePausedOnDownload)
        //     {
        //         BattleContent battleContent = Context.Container.Resolve<BattleContent>();
        //         if (null != battleContent)
        //         {
        //             isBattlePausedOnDownload = false;
        //             battleContent.SetPause(false);
        //         }
        //     }
        // }

        // protected void EnableRetryDownloadOverlayUI()
        // {
        //     UIService.DownloadingOverlayBar(true, true);
        // }

        // protected void UpdateBundleDownloadOverlayUI(long totalSize, long curSize, string locationName)
        // {
        //     string name = Path.GetFileName(locationName);
        //     downloadUI?.OnChangedProgress(totalSize, curSize);
        //     downloadUI?.OnChangedText($"DOWNLOADING...\n{curSize}KB/{totalSize}KB");
        // }

        // protected async UniTask<bool> ShowErrorMessageBox(string locationKey, string errorMsg)
        // {
        //     string title = LocalizationService.Get("title_downloading_error");

        //     if (true == errorMsg.Contains("404"))
        //     {
        //         string keyMsg = LocalizationService.Get("download_error_404_file_not_found");
        //         errorMsg = keyMsg + " :\n" + locationKey;
        //     }
        //     else if (true == errorMsg.Contains("Cannot connect to"))
        //     {
        //         string keyMsg = LocalizationService.Get("download_error_cannot_connect_to_server");
        //         errorMsg = keyMsg + " :\n" + locationKey;
        //     }
        //     else if (true == errorMsg.Contains("Request timeout"))
        //     {
        //         string keyMsg = LocalizationService.Get("download_error_request_time_out");
        //         errorMsg = keyMsg + " :\n" + locationKey;
        //     }
        //     else if (true == errorMsg.Contains("Failed to receive data"))
        //     {
        //         string keyMsg = LocalizationService.Get("download_error_failed_to_receive_data");
        //         errorMsg = keyMsg + " :\n" + locationKey;
        //     }
        //     // 무시할 메세지들
        //     else if (true == errorMsg.Contains("Dependency Exception")) // 이미 관련 번들 다운중 에러가 출력되었을 것이므로 중복 출력하지 않음
        //     {
        //         //msgKey = "download_error_dependency_exception :\n" + locationKey;
        //     }
        //     else
        //     {
        //         int newLine = 50;
        //         while (newLine < errorMsg.Length)
        //         {
        //             errorMsg = errorMsg.Insert(newLine, "\n");
        //             newLine += 50;
        //         }
        //     }

        //     string retry = LocalizationService.Get("download_retry");
        //     string close = LocalizationService.Get("application_quit");
        //     return await MessageService.ConfirmMessage(title, errorMsg, retry, close);
        // }

        // public static bool IsOpenedErrorMessageBox()
        // {
        //     var msgPopup = UIService.Reference<CommonConfirmSimplePopup>();
        //     return null != msgPopup && msgPopup.isActiveAndEnabled;
        // }
    }

    internal class GroupedBundleDownloadOperation<TObject> : BundleDownloadTypeOperation<TObject>
    {
        public override int Key { get; protected set; }
        public override long SizeKBytes
        {
            get
            {
                long size = 0;
                foreach (IBundleDownloadOperation op in operations)
                    size += op.SizeKBytes;
                return size;
            }
            protected set => throw new NotImplementedException();
        }
        public override long CurrentSizeKBytes
        {
            get
            {
                long size = 0;
                foreach (IBundleDownloadOperation op in operations)
                    size += op.CurrentSizeKBytes;
                return size;
            }
            protected set => throw new NotImplementedException();
        }
        public override int Count
        {
            get
            {
                int count = 0;
                foreach (IBundleDownloadOperation op in operations)
                    count += op.Count;
                return count;
            }
            protected set => throw new NotImplementedException();
        }
        public override int CurrentCount
        {
            get
            {
                int count = 0;
                foreach (IBundleDownloadOperation op in operations)
                    count += op.CurrentCount;
                return count;
            }
            protected set => throw new NotImplementedException();
        }
        public override bool IsDone
        {
            get
            {
                foreach (IBundleDownloadOperation op in operations)
                {
                    if (false == op.IsDone)
                        return false;
                }
                return true;
            }
            protected set => throw new NotImplementedException();
        }
        public override bool IsSucceeded
        {
            get
            {
                if (operations.Count == 0)
                    return false;
                foreach (IBundleDownloadOperation op in operations)
                {
                    if (false == op.IsSucceeded)
                        return false;
                }
                return true;
            }
            protected set => throw new NotImplementedException();
        }
        public override List<AsyncOperationHandle> AddressableHandles
        {
            get
            {
                List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
                foreach (IBundleDownloadOperation op in operations)
                    handles.AddRange(op.AddressableHandles);
                return handles;
            }
        }
        public override AsyncOperationHandle MainAddressableHandle
        {
            get
            {
                foreach (IBundleDownloadOperation op in operations)
                {
                    if (true == op.IsMainAsset)
                        return op.MainAddressableHandle;
                }
                return default;
            }
        }

        public List<IBundleDownloadOperation> operations = new List<IBundleDownloadOperation>();
        public GroupedBundleDownloadOperation(int key) { Key = key; }
        public override void ReleaseAddressableHandle()
        {
            foreach (IBundleDownloadOperation op in operations)
                op.ReleaseAddressableHandle();
        }
    }

    internal class BundleDownloadOperation<TObject> : BundleDownloadTypeOperation<TObject>
    {
        public override int Key { get; protected set; }
        public override long SizeKBytes { get; protected set; }
        public override long CurrentSizeKBytes { get; protected set; }
        public override int Count { get; protected set; }
        public override int CurrentCount { get; protected set; }
        public override bool IsDone { get; protected set; }
        public override bool IsSucceeded { get; protected set; }
        public override bool IsMainAsset { get; protected set; }
        public override List<AsyncOperationHandle> AddressableHandles { get; } = new List<AsyncOperationHandle>();
        public BundleDownloadOperation(int key, bool isMainAsset) { Key = key; IsMainAsset = isMainAsset; Valid = true; }

        public bool Valid { get; private set; } = false;

        public override void ReleaseAddressableHandle()
        {
            Valid = false;
            completed = null;
            foreach (AsyncOperationHandle handle in AddressableHandles)
                Addressables.Release(handle);
            AddressableHandles.Clear();
        }

        public async UniTask LoadAsync(List<IResourceLocation> locations, Action<TObject> onHandleComplete = null, bool showDownloadUI = true)
        {
            IsDone = false;
            IsSucceeded = false;
            Count = 0;
            AddCompleted(onHandleComplete);

            if (locations == null || locations.Count == 0)
            {
                IsSucceeded = true;
                IsDone = true;
                return;
            }

            Count = locations.Count;
            long downSize = 0;
            foreach (IResourceLocation location in locations)
            {
                var sizeData = location.Data as ILocationSizeData;
                if (sizeData != null)
                {
                    downSize = sizeData.ComputeSize(location, Addressables.ResourceManager) / 1024;
                    SizeKBytes += downSize;

#if DEV_TOOLS
                    if( downSize > 0 )
                    {
                        string id = location.InternalId;
                        id = Path.GetFileNameWithoutExtension(id);
                        Debug.Log($"<color=orange>Download Asset {id}, DownSize : {downSize}</color>");
                    }
#endif
                }
            }

            bool isAvailableSpace = await AssetService.WaitForStorageAvailableSpace(SizeKBytes * 1024);
            if (isAvailableSpace == false)
            {
                Application.Quit();
                return;
            }

            showDownloadUI &= SizeKBytes > 0;
            if (true == showDownloadUI && false == AssetServiceNetworkErrorHolder.HasError())
            {
                // EnableBundleDownloadOverlayUI();
            }

            // 리트라이 시 화면 가리기
            bool enabledRetryOverlayUI = false;
            for (int i = 0; i < locations.Count; ++i, ++CurrentCount)
            {
                AsyncOperationHandle<TObject> opHandle = Addressables.LoadAssetAsync<TObject>(locations[i]);
                while (!opHandle.IsDone)
                {
                    if (!Valid)
                    {
                        break;
                    }

                    float rate = (i + opHandle.PercentComplete) / locations.Count;
                    CurrentSizeKBytes = (long)(SizeKBytes * rate);
                    if (true == showDownloadUI)
                    {
                        // UpdateBundleDownloadOverlayUI(SizeKBytes, CurrentSizeKBytes, locations[i].PrimaryKey);
                    }
                    await UniTask.Yield();
                }

                if (!Valid)
                {
                    Log.Trace($"BundleDownloadOperation is Invalid {locations[i].InternalId}");
                    Addressables.Release(opHandle);
                    IsDone = true;
                    IsSucceeded = false;
                    if (true == enabledRetryOverlayUI)
                        // DisableBundleDownloadOverlayUI();
                        return;
                }

                if (opHandle.Result == null || opHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    // 일반 에러에 대한 처리!!
                    Addressables.Release(opHandle);
                    // DisableBundleDownloadOverlayUI();

                    bool isRetry = true;
                    if (true == AssetServiceNetworkErrorHolder.HasError() &&
                        false == AssetServiceNetworkErrorHolder.IsOwner(locations[i].PrimaryKey))
                    {
                        await AssetServiceNetworkErrorHolder.ErrorAwaiter;
                    }
                    else
                    {
                        AssetServiceNetworkErrorHolder.SetError(locations[i].PrimaryKey);
                        // isRetry = await ShowErrorMessageBox(locations[i].PrimaryKey, opHandle.OperationException?.Message);
                    }

                    if (true == isRetry)
                    {
                        enabledRetryOverlayUI = true;
                        // EnableRetryDownloadOverlayUI();
                        --i;
                        --CurrentCount;
                    }
                    else
                    {
                        Application.Quit(); // quit or ?
                        break;
                    }
                }
                else
                {
                    if (true == enabledRetryOverlayUI)
                        // DisableBundleDownloadOverlayUI();
                        AssetServiceNetworkErrorHolder.Clear();
                    AddressableHandles.Add(opHandle);
                }
            }

            if (true == showDownloadUI)
                // DisableBundleDownloadOverlayUI();

                IsSucceeded = true;
            IsDone = true;
        }
    }

    internal class SceneDownloadOperation : BundleDownloadTypeOperation<Scene>
    {
        public override int Key { get; protected set; }
        public override long SizeKBytes { get; protected set; }
        public override long CurrentSizeKBytes { get; protected set; }
        public override int Count { get; protected set; }
        public override int CurrentCount { get; protected set; }
        public override bool IsDone { get; protected set; }
        public override bool IsSucceeded { get; protected set; }
        public override List<AsyncOperationHandle> AddressableHandles { get; } = new List<AsyncOperationHandle>();
        public SceneDownloadOperation(int key) { Key = key; Valid = true; }
        public bool Valid { get; private set; } = false;
        public override void ReleaseAddressableHandle()
        {
            Valid = false;
            completed = null;
            foreach (AsyncOperationHandle handle in AddressableHandles)
            {
                if (true == handle.IsValid())
                    Addressables.Release(handle);
            }

            AddressableHandles.Clear();
        }

        public async UniTask LoadAsync(List<IResourceLocation> locations, LoadSceneMode mode, Action<Scene> onHandleComplete = null, bool showDownloadUI = true)
        {
            if (locations == null || locations.Count <= 0)
                return;

            Count = locations.Count;
            AddCompleted(onHandleComplete);

            var sizeData = locations[0].Data as ILocationSizeData;
            if (sizeData != null)
                SizeKBytes = sizeData.ComputeSize(locations[0], Addressables.ResourceManager) / 1024;

            showDownloadUI &= SizeKBytes > 0;
            // if (true == showDownloadUI && false == AssetServiceNetworkErrorHolder.HasError())
            //     EnableBundleDownloadOverlayUI();

            // 리트라이 시 화면 가리기
            bool enabledRetryOverlayUI = false;
            while (true)
            {
                AsyncOperationHandle<SceneInstance> opHandle = Addressables.LoadSceneAsync(locations[0], mode);
                while (!opHandle.IsDone)
                {
                    if (!Valid)
                    {
                        break;
                    }

                    CurrentSizeKBytes = (long)(SizeKBytes * opHandle.PercentComplete);
                    // if (true == showDownloadUI)
                    // {
                    //     UpdateBundleDownloadOverlayUI(SizeKBytes, CurrentSizeKBytes, locations[0].PrimaryKey);
                    // }
                    CurrentSizeKBytes = (long)(SizeKBytes * opHandle.PercentComplete);
                    await UniTask.Yield();
                }

                if (!Valid)
                {
                    IsDone = true;
                    IsSucceeded = false;
                    Log.Trace($"SceneDownloadOperation Op is Invalid {locations[0].InternalId}");
                    Addressables.Release(opHandle);
                    // if (true == showDownloadUI || true == enabledRetryOverlayUI)
                    //     DisableBundleDownloadOverlayUI();

                    return;
                }

                if (AsyncOperationStatus.Succeeded != opHandle.Status)
                {
                    Addressables.Release(opHandle);
                    // DisableBundleDownloadOverlayUI();

                    bool isRetry = true;
                    if (true == AssetServiceNetworkErrorHolder.HasError() &&
                        false == AssetServiceNetworkErrorHolder.IsOwner(locations[0].PrimaryKey))
                    {
                        await AssetServiceNetworkErrorHolder.ErrorAwaiter;
                    }
                    else
                    {
                        AssetServiceNetworkErrorHolder.SetError(locations[0].PrimaryKey);
                        // isRetry = await ShowErrorMessageBox(locations[0].PrimaryKey, opHandle.OperationException?.Message);
                    }

                    if (true == isRetry)
                    {
                        enabledRetryOverlayUI = true;
                        // EnableRetryDownloadOverlayUI();
                    }
                    else
                    {
                        Application.Quit(); // quit or ?
                    }
                }
                else
                {
                    AssetServiceNetworkErrorHolder.Clear();
                    if (true == showDownloadUI || true == enabledRetryOverlayUI)
                        // DisableBundleDownloadOverlayUI();

                        CurrentCount = Count;
                    IsSucceeded = opHandle.Status == AsyncOperationStatus.Succeeded;
                    AddressableHandles.Add(opHandle);
                    IsDone = true;
                    break;
                }
            }
        }

        public override void Completed()
        {
            completed?.Invoke(((SceneInstance)MainAddressableHandle.Result).Scene);
            completed = null;
        }
    }
}

