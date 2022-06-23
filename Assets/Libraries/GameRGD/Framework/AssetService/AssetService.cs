#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;

#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using System.IO;

namespace Helen
{
    public class AssetService : MonoBehaviour
    {
        // 어드레서블 내부에서 사용하고 있는 스트링, 숨겨져 있어서 동일한 것으로 선언
        public const string BuiltInDataGroupName = "Built In Data";

        public const string DefaultLocalGroupName = "defaultlocalgroup_assets_all.bundle";

        public static bool HasDefaultGroup { get; private set; }
        private static bool initialized = false;

        // IBundleDownloadOperation 의 고유 인덱스
        private static int operationKeyCache = 0;

        // 캐싱 된 리소스 다운로드 명령(AsyncOperationHandle 소유)
        private static Dictionary<object, IBundleDownloadOperation> requestedBundleList = new Dictionary<object, IBundleDownloadOperation>();

        // 에셋번들의 어드레서블 키를 용도에 따라 (리모트, 로컬) 미리 구분 해 놓은 리스트
        private static Dictionary<string, List<object>> remoteBundleKeyList = new Dictionary<string, List<object>>();

        private static bool waitForStorageAvailableSpace = false;

        public static bool RemoteMode { get; private set; } = false;
        public static async UniTask Initialize()
        {
            if (true == initialized)
                return;
            initialized = true;
            RemoteMode = false;

#if !UNITY_EDITOR
                PlayerPrefs.DeleteKey(Addressables.kAddressablesRuntimeDataPath);
#endif
            Addressables.ResourceManager.InternalIdTransformFunc = TransformResourceIdFunc;

            AsyncOperationHandle<IResourceLocator> handle = Addressables.InitializeAsync();
            await handle.Task;

            SpriteAtlasManager.atlasRequested += OnSpriteAtlasRequested;

            //  ResourceManager.ExceptionHandler = ExceptionHandler;

            bool reloadCatalog = true;

            if (!IsRemoteCatalogService())
                reloadCatalog = false;

            if (reloadCatalog)
            {
                string localCatalogPath = Addressables.RuntimePath + "/catalog.json";
                Log.Trace($"Local Catalog Path {localCatalogPath}");
                Addressables.ClearResourceLocators();
                var LocalCatalogHandle = Addressables.LoadContentCatalogAsync(localCatalogPath);
                await LocalCatalogHandle.Task;
            }

            //var checkHandle = Addressables.CheckForCatalogUpdates();
            //if (checkHandle.IsValid())
            //{
            //    await checkHandle.Task;

            //    if (checkHandle.IsValid() && checkHandle.Result.Count > 0)
            //    {
            //        var updateHandle = Addressables.UpdateCatalogs( checkHandle.Result );
            //        await updateHandle.Task;
            //    }
            //}

#if UNITY_EDITOR
            // fast mode 에서 AssetDatabaseProvider 로 로딩할때 강제로 입력되어 있는 0.2초의 딜레이 제거
            if (true == AddressablesUtil.IsPlayModeFast())
            {
                var adbProvider = Addressables.ResourceManager.ResourceProviders
                    .FirstOrDefault(rp => rp.GetType() == typeof(AssetDatabaseProvider)) as AssetDatabaseProvider;
                System.Type adbType = typeof(AssetDatabaseProvider);
                FieldInfo fi = adbType.GetField("m_LoadDelay", BindingFlags.NonPublic | BindingFlags.Instance);
                fi.SetValue(adbProvider, -1f);
            }
#endif



            // 카달로그 업데이트
            // 이 기능은 런타임에 이미 다운받아 놓은 카달로그와 서버의 카달로그가 달라졌을때 사용하는 기능 같다.
            // 증분 빌드를 해서 카달로그가 변경되더라도 이 시점에서 이 함수로 응답이 오는 것은 없다.
            //             AsyncOperationHandle updateCatalogHandle = Addressables.UpdateCatalogs(null, false);
            //             await updateCatalogHandle.Task;
            //             if (updateCatalogHandle.Status == AsyncOperationStatus.Succeeded && null != updateCatalogHandle.Result)
            //             {
            //                 var updatedList = updateCatalogHandle.Result as List<IResourceLocator>;
            //                 foreach (var updatedLoc in updatedList)
            //                     Log.Trace("asset updated : " + updatedLoc.LocatorId);
            //             }
            //             Addressables.Release(updateCatalogHandle);
        }

        private static string TransformResourceIdFunc(IResourceLocation location)
        {
            // if (location.InternalId.Contains(Const.EmptyBundleUrl))
            // {
            //     var temp = location.InternalId.Replace(Const.EmptyBundleUrl, Const.BundleUrl);
            //     temp = temp.Replace(Const.buildConfig.Version, Const.BundleVersion);

            //     return temp;
            // }
            // else
            // {
            //     return location.InternalId;
            // }

            return location.InternalId;
        }

        public static async UniTask UpdateCadalog()
        {
            // if (IsRemoteCatalogService())
            // {
            //     bool customLocation = false;
            //     var customLocationConfig = Resources.Load<AddressableCustomLocationConfig>("AddressablesCustomLocationConfig");
            //     if (customLocationConfig != null)
            //     {
            //         if (true == customLocationConfig.useCustomData && (true == Application.isEditor || true == customLocationConfig.useInBuildMode))
            //             customLocation = true;
            //     }

            //     if (!customLocation)
            //     {
            //         Addressables.ClearResourceLocators();

            //         //var buildTarget = setting.profileSettings.GetValueByName(setting.activeProfileId, "BuildTarget");

            //         var newCatalogPath = $"{Const.AssetBundleUrl}/catalog_helen.json";
            //         Log.Trace($"Reload Catalog, Path : {newCatalogPath}");

            //         var catalogHandle = Addressables.LoadContentCatalogAsync(newCatalogPath);
            //         await catalogHandle.Task;
            //     }
            // }
            // RemoteMode = true;
        }

        public static bool IsRemoteCatalogService()
        {
#if UNITY_EDITOR
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            if (setting.ActivePlayModeDataBuilderIndex == 0)
                return false;
#endif
            // if (Const.buildConfig.builtIn)
            //     return false;

            return true;
        }

        public static bool CollectionBundleInfosComplete { get; private set; } = false;

        public static void CollectionBundleInfos()
        {
            // if (!IsRemoteCatalogService())
            //     return;

            // CollectionBundleInfosComplete = false;
            // float time = Time.realtimeSinceStartup;
            // Log.Trace($"<color=red>BundleInfo Collection Start { time }</color>");
            // remoteBundleKeyList.Clear();
            // // 다운로드 번들 리스트를 미리 수집
            // // 리모트 번들들을 다운로드 시점 (RemoteDownloadPosition)에 따라 구분해서 수집해 놓는다
            // string remoteBundleLabel = AddressablesUtil.AssetBundlePosition.Remote.ToString();
            // foreach (IResourceLocator locator in Addressables.ResourceLocators)
            // {
            //     if (!locator.LocatorId.Contains("catalog_helen"))
            //         continue;

            //     ResourceLocationMap locMap = locator as ResourceLocationMap;

            //     HasDefaultGroup = locMap.Locate(DefaultLocalGroupName, null, out var defaultGroup);
            //     var remotePositionTypeList = Enum.GetValues(typeof(AddressablesUtil.RemoteDownloadPosition));
            //     foreach (var remotePos in remotePositionTypeList)
            //     {
            //         if (false == locMap.Locate(remotePos.ToString(), null, out var locations))
            //             continue;

            //         List<object> locationKeyList = new List<object>();
            //         remoteBundleKeyList.Add(remotePos.ToString(), locationKeyList);

            //         foreach (var loc in locations)
            //         {
            //             if (null == loc.Dependencies)
            //                 continue;
            //             foreach (var dep in loc.Dependencies)
            //             {
            //                 var path = dep.InternalId;
            //                 path = Path.GetFileNameWithoutExtension(path);
            //                 AddressablesUtil.RemoteDownloadPosition pos = (AddressablesUtil.RemoteDownloadPosition)remotePos;

            //                 if (pos == AddressablesUtil.RemoteDownloadPosition.DownOnPatchScene && path.IndexOf("r_") == 0)
            //                 {
            //                     //#if DEV_TOOLS
            //                     //                                Log.Trace($"{remotePos} Def Collect Bundle {path} from {loc.InternalId}");
            //                     //#endif
            //                     continue;
            //                 }

            //                 bool isExist = locationKeyList.Exists(x => x.ToString() == dep.PrimaryKey);
            //                 string transPath = dep.InternalId.Replace(Const.EmptyBundleUrl, Const.BundleUrl);
            //                 bool isPathRemote = ResourceManagerConfig.IsPathRemote(transPath);

            //                 if (!isExist && isPathRemote)
            //                 {
            //                     locationKeyList.Add(dep.PrimaryKey);
            //                 }
            //             }
            //         }
            //     }
            //     break;
            // }
            // CollectionBundleInfosComplete = true;
            // Log.Trace($"<color=red>BundleInfo Collection end { Time.realtimeSinceStartup }, Collection Time : { Time.realtimeSinceStartup - time }</color>");
        }

        // 초기화 후에 카탈로그를 교체하는 예제 코드.
        public static async UniTask<string> GetBuiltInRemotePath()
        {
            // 카탈로그 정보를 얻기위해 세팅 파일을 로딩한다
            var playerSettingsLocation = Addressables.RuntimePath + "/settings.json";
            var runtimeDataLocation = new ResourceLocationBase("RuntimeData", playerSettingsLocation, typeof(JsonAssetProvider).FullName, typeof(ResourceManagerRuntimeData));
            var rtdHandle = Addressables.ResourceManager.ProvideResource<ResourceManagerRuntimeData>(runtimeDataLocation);
            await rtdHandle.Task;

            string catalogFileName = string.Empty;
            string builtInRemotePath = string.Empty;
            string remoteCatalogHashKeyName = ResourceManagerRuntimeData.kCatalogAddress + "RemoteHash";
            bool hasRemoteCatalog = false;
            foreach (var catalogLoc in rtdHandle.Result.CatalogLocations)
            {
                foreach (var key in catalogLoc.Keys)
                {
                    if (key == remoteCatalogHashKeyName)
                    {
                        hasRemoteCatalog = true;
                        // 빌드 했을 때 지정된 리모트 카탈로그 이름을 얻어낸다
                        catalogFileName = System.IO.Path.GetFileName(catalogLoc.InternalId);
                        break;
                    }
                    else if (key == ResourceManagerRuntimeData.kCatalogAddress)
                    {
                        // 카탈로그를 읽는다
                        // 원격주소 하나만 얻어낸다 (번들이 아닌 기본 json 카탈로그)
                        if (true == catalogLoc.InternalId.Contains(".json"))
                        {
                            string builtInCatalogPath = Addressables.ResolveInternalId(catalogLoc.InternalId);
                            IResourceLocation location = new ResourceLocationBase(builtInCatalogPath, builtInCatalogPath, typeof(JsonAssetProvider).FullName, typeof(ContentCatalogData));
                            var catalogHandle = Addressables.ResourceManager.ProvideResource<ContentCatalogData>(location);
                            await catalogHandle.Task;
                            if (null == catalogHandle.Result)
                            {
                                Log.Warning("Failed to load catalog.json");
                            }
                            else
                            {
                                string remotePath = catalogHandle.Result.InternalIds.FirstOrDefault(
                                    x => true == ResourceManagerConfig.ShouldPathUseWebRequest(x));
                                if (false == string.IsNullOrEmpty(remotePath))
                                {
                                    int last = remotePath.LastIndexOf("/");
                                    builtInRemotePath = remotePath.Substring(0, last + 1);
                                }
                            }
                            Addressables.Release(catalogHandle);
                        }
                        // 원격주소 하나만 얻어낸다 (압축된 카탈로그 Compress Local Catalog)
                        else if (true == catalogLoc.InternalId.Contains(".bundle"))
                        {
                            string builtInCatalogPath = Addressables.ResolveInternalId(catalogLoc.InternalId);
                            var bundleRequest = AssetBundle.LoadFromFileAsync(builtInCatalogPath);
                            await bundleRequest;
                            if (false == bundleRequest is AssetBundleCreateRequest || null == bundleRequest.assetBundle)
                            {
                                Log.Warning("Failed to load catalog.bundle");
                                continue;
                            }

                            var textAssetReq = bundleRequest.assetBundle.LoadAllAssetsAsync<TextAsset>();
                            await textAssetReq;

                            TextAsset textAsset = textAssetReq.asset as TextAsset;
                            if (null == textAsset || null == textAsset.text)
                                continue;
                            ContentCatalogData catalogData = JsonUtility.FromJson<ContentCatalogData>(textAsset.text);
                            string remotePath = catalogData.InternalIds.First(x => true == ResourceManagerConfig.ShouldPathUseWebRequest(x));
                            if (false == string.IsNullOrEmpty(remotePath))
                            {
                                int last = remotePath.LastIndexOf("/");
                                builtInRemotePath = remotePath.Substring(0, last + 1);
                            }
                        }
                    }
                }
            }

            if (true == hasRemoteCatalog)
            {
                // 서버로부터 받은 경로와 가탈로그 파일명을 붙인다.
                // 일단 테스트를 위해 서버의 주소를 사용하지 않음
                string remoteCatalogLocation = builtInRemotePath + catalogFileName;
                Log.Trace("catalog path : " + remoteCatalogLocation);
                //if (string.IsNullOrEmpty(Const.BundleVersion))
                //    remoteCatalogLocation = PathHelper.Combine(oldRemotePath, catalogFileName);
                //else
                //    remoteCatalogLocation = Const.AssetBundleUrl + catalogFileName;

                //AddressablesRuntimeProperties.SetPropertyValue("CatalogLoadPath", Const.AssetBundleUrl);
            }
            return builtInRemotePath;
        }

        private static void ExceptionHandler(AsyncOperationHandle handle, Exception exp)
        {
            if (exp.Message.Contains("Content update not available."))
                return;
            Log.ErrorFormat("Helen Addressables Exception : {0}", exp.Message);
        }

        private static void OnSpriteAtlasRequested(string atlasName, System.Action<SpriteAtlas> callback)
        {
            // if (false == initialized)
            // {
            //     Log.Warning("Sprite atlas requested before Addressables.InitializeAsync : " + atlasName);
            //     return;
            // }

            // Log.Trace($"OnSpriteAtlasRequested : { Const.BundleAtlasDirPath + atlasName}.spriteatlas");
            // LoadAssetAsync(Const.BundleAtlasDirPath + atlasName + ".spriteatlas", callback);
        }

        private static void CheckInitialized()
        {
            if (false == initialized)
                Log.Warning("You must call AssetService.Initialize first");
        }

        private static int GenerateKey()
        {
            return operationKeyCache++;
        }



        public static async UniTask<long> CheckDownloadSize(AddressablesUtil.RemoteDownloadPosition downType)
        {
            string assetKey = downType.ToString();
            if (true == remoteBundleKeyList.TryGetValue(assetKey, out var locationKeys))
            {
                if (true == requestedBundleList.TryGetValue(assetKey, out var operation))
                    return false == operation.IsDone ? operation.SizeKBytes : 0;
                else
                    return await CheckDownloadSize(locationKeys);
            }
            return default;
        }

        private static async UniTask<long> CheckDownloadSize(IEnumerable<object> groupKeys)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle =
                Addressables.LoadResourceLocationsAsync(groupKeys, Addressables.MergeMode.Union);
            await locationHandle.Task;

            long size = 0;
            foreach (IResourceLocation location in locationHandle.Result)
            {
                var sizeData = location.Data as ILocationSizeData;
                if (sizeData != null)
                    size += sizeData.ComputeSize(location, Addressables.ResourceManager) / 1024;
            }
            return size;
        }

        public static async UniTask<bool> WaitForStorageAvailableSpace(long downloadSizeBytes)
        {
            //             if (downloadSizeBytes <= 0)
            //                 return true;

            //             if (waitForStorageAvailableSpace)
            //                 await UniTask.WaitUntil(() => waitForStorageAvailableSpace == false);

            //             try
            //             {
            //                 while (true)
            //                 {
            //                     if (DiskSpaceHelper.CheckAvailableFreeSpaceBytes(downloadSizeBytes))
            //                         break;

            //                     waitForStorageAvailableSpace = true;
            //                     var retryDownload = await MessageService.ConfirmMessage(LocalizationService.Get("popup_notice_not_enough_space_title"),
            //                         LocalizationService.Get("popup_notice_not_enough_space_context"),
            //                         LocalizationService.Get("download_retry"),
            //                         LocalizationService.Get("popup_exit_game_button"));

            //                     if (retryDownload == false)
            //                     {
            // #if UNITY_EDITOR
            //                         UnityEditor.EditorApplication.isPlaying = false;
            // #endif
            //                         return false;
            //                     }

            //                     await UniTask.Yield();
            //                 }
            //             }
            //             finally
            //             {
            //                 waitForStorageAvailableSpace = false;
            //             }

            return true;
        }

        public static AssetLoadHandle DownloadBundlesAsync(AddressablesUtil.RemoteDownloadPosition downType, Action<IAssetBundleResource> onComplete = null)
        {
            CheckInitialized();
            string assetKey = downType.ToString();
            if (true == requestedBundleList.TryGetValue(assetKey, out var op))
            {
                if (true == op.IsDone)
                {
                    onComplete?.Invoke((IAssetBundleResource)op.MainAddressableHandle.Result);
                }
                else
                {
                    if (null != onComplete)
                        ((BundleDownloadTypeOperation<IAssetBundleResource>)op).AddCompleted(onComplete);
                }
                return op.Handle;
            }
            else
            {
                if (false == remoteBundleKeyList.TryGetValue(assetKey, out var locationKeys))
                    return default;
                BundleDownloadOperation<IAssetBundleResource> operation = new BundleDownloadOperation<IAssetBundleResource>(GenerateKey(), true);
                requestedBundleList.Add(assetKey, operation);
                UniTask task = DownloadBundlesAsync(locationKeys, operation, null);
                return operation.CreateExternalHandle(task);
            }
        }

        private static async UniTask DownloadBundlesAsync(IEnumerable<object> groupKeys,
            BundleDownloadOperation<IAssetBundleResource> opSingle, Action<IAssetBundleResource> onComplete)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(groupKeys, Addressables.MergeMode.Union);
            await locationHandle.Task;

            await opSingle.LoadAsync(locationHandle.Result.ToList(), onComplete, false);

            if (false == opSingle.IsSucceeded)
            {
                Log.Error("DownloadBundlesAsync failed to load");
            }

            opSingle.Completed();
        }

        public static AssetLoadHandle LoadAssetAsync<TObject>(object assetKey, Action<TObject> onComplete = null, bool applicationBundle = false)
            where TObject : UnityEngine.Object
        {
            CheckInitialized();
            IBundleDownloadOperation op;
            if (true == requestedBundleList.TryGetValue(assetKey, out op))
            {
                if (true == op.IsDone)
                {
                    onComplete?.Invoke((TObject)op.MainAddressableHandle.Result);
                }
                else
                {
                    if (null != onComplete)
                        ((BundleDownloadTypeOperation<TObject>)op).AddCompleted(onComplete);
                }
                return op.Handle;
            }
            else
            {
                GroupedBundleDownloadOperation<TObject> operation = new GroupedBundleDownloadOperation<TObject>(GenerateKey());
                if (applicationBundle)
                    operation.ApplicationBundle = true;

                requestedBundleList.Add(assetKey, operation);
                UniTask task = LoadAssetAsync(assetKey, operation, onComplete);
                return operation.CreateExternalHandle(task);
            }
        }

        public static AssetLoadHandle LoadAssetAsync<TObject>(AssetReference assetRef, Action<TObject> onComplete = null)
            where TObject : UnityEngine.Object
        {
            return LoadAssetAsync<TObject>(assetRef.AssetGUID, onComplete);
        }

        public static async UniTask<TObject> LoadInstantiateAsync<TObject>(object assetKey, Action<TObject> onComplete = null)
            where TObject : UnityEngine.Object
        {
            AssetLoadHandle handle = LoadAssetAsync<TObject>(assetKey, onComplete);
            await handle.Task;
            if (false == handle.IsSucceeded)
            {
                Log.Warning("LoadInstantiateAsync - Cannot load " + assetKey);
                return default;
            }
            return Instantiate(handle.Result as TObject);
        }

        public static async UniTask<TObject> LoadInstantiateAsync<TObject>(AssetReference assetRef, Action<TObject> onComplete = null)
           where TObject : UnityEngine.Object
        {
            return await LoadInstantiateAsync<TObject>(assetRef.AssetGUID, onComplete);
        }

        private static async UniTask LoadAssetAsync<TObject>(object assetKey, GroupedBundleDownloadOperation<TObject> opGroup, Action<TObject> onComplete)
            where TObject : UnityEngine.Object
        {
            DateTime prevTime = DateTime.Now;

            //Log.Trace($"LoadAssetAsync Start {assetKey}, Frame : {Time.frameCount}");

            // 필요한 번들 목록 수집
            // Addressables.InitializeAsync() 나 Addressables.UpdateCatalog() 수행 중이 아니라면 task가 곧바로 complete되긴 한다..
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(assetKey, typeof(TObject));
            if (!locationHandle.IsDone)
                await locationHandle.Task;

            // 번들로 등록되어 있지 않은 리소스는 카운트가 0인 것으로 확인
            if (0 == locationHandle.Result.Count)
            {
                Log.Warning("LoadAssetAsync - Not registered asset " + assetKey);
                requestedBundleList.Remove(assetKey);
                onComplete?.Invoke(null);
                return;
            }

            // 의존성 번들 목록 수집
            List<IResourceLocation> dependencies = new List<IResourceLocation>();
            foreach (IResourceLocation loc in locationHandle.Result)
            {
                if (null != loc.Dependencies)
                    dependencies.AddRange(loc.Dependencies);
            }

#if DEV_TOOLS && UNITY_EDITOR
            Log.Trace($"LoadAssetAsync {assetKey}");
#endif

            // 의존성 번들 다운과 메인 리소스 다운 두가지를 요청한다
            BundleDownloadOperation<IAssetBundleResource> depOperation = new BundleDownloadOperation<IAssetBundleResource>(opGroup.Key, false);
            BundleDownloadOperation<TObject> mainAssetOperation = new BundleDownloadOperation<TObject>(opGroup.Key, true);
            opGroup.operations.Add(depOperation);
            opGroup.operations.Add(mainAssetOperation);

            // 의존성 번들 다운로드
            await depOperation.LoadAsync(dependencies);

            // 메인 리소스 다운로드
            List<IResourceLocation> d = locationHandle.Result.ToList();
            await mainAssetOperation.LoadAsync(d);

            //Log.Trace($"LoadAssetAsync complete {assetKey}, Frame : {Time.frameCount}");

            opGroup.AddCompleted(onComplete);
            AsyncOperationHandle mainHandle = mainAssetOperation.MainAddressableHandle;
            if (mainHandle.Result == null || mainHandle.Status != AsyncOperationStatus.Succeeded)
            {
                var message = "LoadAssetAsync has null result " + mainHandle.DebugName;
                if (mainHandle.OperationException != null)
                    message += " Exception: " + mainHandle.OperationException;
                Log.Error(message);
            }
            else
            {
                // if (typeof(TObject) == typeof(UIServiceData))
                // {
                //     var uiServiceData = mainHandle.Result as UIServiceData;
                //     if (uiServiceData.Manage == UIServiceData.ManageType.Unmanaged)
                //     {
                //         opGroup.ApplicationBundle = true;
                //     }
                // }

                opGroup.Completed();
            }

            DateTime nextTime = DateTime.Now;
            TimeSpan pastTime = nextTime - prevTime;

            //#if UNITY_EDITOR
            //            Log.Trace($"[AssetService] LoadAssetAsync({assetKey}) dependencies count:{dependencies.Count}, seconds :{pastTime.TotalSeconds}");
            //#endif
        }

        public static AssetLoadHandle LoadSceneAsync(AssetReference assetRef, LoadSceneMode mode, Action<Scene> onComplete = null)
        {
            return LoadSceneAsync(assetRef.AssetGUID, mode, onComplete);
        }

        public static AssetLoadHandle LoadSceneAsync(string assetKey, LoadSceneMode mode, Action<Scene> onComplete = null)
        {
            CheckInitialized();
            // UnloadSceneAsync 와 쌍으로 쓰려면 아래 코드가 필요하다
            //IBundleDownloadOperation operation;
            //if (true == requestedBundleList.TryGetValue(assetKey, out operation))
            //{
            //    return operation.Handle;
            //}
            //else
            //{
            //    operation = new SceneDownloadOperation(GenerateKey());
            //}
            SceneDownloadOperation operation = new SceneDownloadOperation(GenerateKey());
            UniTask task = LoadSceneAsync(assetKey, mode, (SceneDownloadOperation)operation, onComplete);
            return operation.CreateExternalHandle(task);
        }

        private static async UniTask LoadSceneAsync(string assetKey, LoadSceneMode mode, SceneDownloadOperation operation, Action<Scene> onComplete)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(assetKey, typeof(SceneInstance));
            if (!locationHandle.IsDone)
                await locationHandle.Task;

            if (0 == locationHandle.Result.Count)
            {
                Log.Warning("LoadSceneAsync - Not registered scene " + assetKey);
                onComplete?.Invoke(default(Scene));
                return;
            }

            operation.AddCompleted(onComplete);

            List<IResourceLocation> list = locationHandle.Result.ToList();
            if (!operation.IsDone)
                await operation.LoadAsync(list, mode);

            operation.Completed();
        }

        public static UniTask UnloadSceneAsync(string assetKey, Action onComplete = null)
        {
            CheckInitialized();
            IBundleDownloadOperation operation;
            if (false == requestedBundleList.TryGetValue(assetKey, out operation))
            {
                return UnLoadBuiltInSceneAsync(assetKey, onComplete);
            }

            SceneDownloadOperation sceneOp = operation as SceneDownloadOperation;
            return UnLoadSceneAsync(assetKey, sceneOp, onComplete);
        }

        private static async UniTask UnLoadSceneAsync(string assetKey, SceneDownloadOperation operation, Action onComplete)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync((SceneInstance)operation.AddressableHandles[0].Result);
            await handle.Task;

            requestedBundleList.Remove(assetKey);
            onComplete?.Invoke();
        }

        public static async UniTask LoadBuiltInSceneAsync(string scenePath, LoadSceneMode mode)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
#if UNITY_EDITOR
            await EditorSceneManager.LoadSceneAsyncInPlayMode(sceneName, new LoadSceneParameters(mode));
#else
            await SceneManager.LoadSceneAsync(sceneName, mode);
#endif
        }

        public static async UniTask UnLoadBuiltInSceneAsync(string scenePath, Action onComplete)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
#if UNITY_EDITOR
            Scene scene = EditorSceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
                await EditorSceneManager.UnloadSceneAsync(scene);
#else
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
                await SceneManager.UnloadSceneAsync(scene);
#endif
            onComplete?.Invoke();
        }

        public static void ReleaseAll(bool keepApplicationBundle = true)
        {
            Log.Trace("Asset Bundle Release");

            List<KeyValuePair<object, IBundleDownloadOperation>> applicationBundles = new List<KeyValuePair<object, IBundleDownloadOperation>>();
            CheckInitialized();
            foreach (var operation in requestedBundleList)
            {
                if (keepApplicationBundle && operation.Value.ApplicationBundle)
                    applicationBundles.Add(operation);
                else
                    operation.Value.ReleaseAddressableHandle();
            }
            requestedBundleList.Clear();

            foreach (var applicationBundleInfo in applicationBundles)
            {
                requestedBundleList.Add(applicationBundleInfo.Key, applicationBundleInfo.Value);
            }
        }

        public static void AssetServiceRelease()
        {

        }
    }
}