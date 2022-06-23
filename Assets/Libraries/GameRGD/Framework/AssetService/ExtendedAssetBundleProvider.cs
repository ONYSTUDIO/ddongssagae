using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;
using UnityEngine;
using AsyncOperation = UnityEngine.AsyncOperation;

namespace Helen
{
    /*
      * 어드레서블의 AssetBundleProvider 에서 sync 로딩과 crc mismatch 오류 처리를 위해 확장한 클래스
      */

    class AssetBundleResource : IAssetBundleResource
    {
        AssetBundle m_AssetBundle;
        DownloadHandlerAssetBundle m_downloadHandler;
        UnityEngine.AsyncOperation m_RequestOperation;
        Helen.WebRequestQueueOperation m_WebRequestQueueOperation;
        internal ProvideHandle m_ProvideHandle;
        internal AssetBundleRequestOptions m_Options;
        int m_Retries;
        long m_BytesToDownload;

        internal UnityWebRequest CreateWebRequest(IResourceLocation loc)
        {
            var url = m_ProvideHandle.ResourceManager.TransformInternalId(loc);
            if (m_Options == null)
                return UnityWebRequestAssetBundle.GetAssetBundle(url);

            UnityWebRequest webRequest;
            if (!string.IsNullOrEmpty(m_Options.Hash))
            {
                Hash128 currentHash = Hash128.Parse(m_Options.Hash);
                CachedAssetBundle cachedBundle = new CachedAssetBundle(m_Options.BundleName, currentHash);
#if ENABLE_CACHING
                // 이전 버전이 있다면 모두 제거
                List<Hash128> cachedHashList = new List<Hash128>();
                var bundleName = Path.GetFileNameWithoutExtension(url);
                Caching.GetCachedVersions(bundleName, cachedHashList);
                // 버전이 두개 이상 있다면 현재의 것과 이전의 것들이 있다는 의미. 현재의 것만 남기고 삭제 한다
                if (cachedHashList.Count > 1)
                {
                    Debug.LogFormat("Remove old version bundles : " + bundleName);
                    foreach (var hash in cachedHashList)
                    {
                        if (hash != currentHash)
                            Caching.ClearCachedVersion(bundleName, hash);
                    }
                }

                if (m_Options.UseCrcForCachedBundle || !Caching.IsVersionCached(cachedBundle))
                    webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, cachedBundle, m_Options.Crc);
                else
                    webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, cachedBundle);
#else
                webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, cachedBundle, m_Options.Crc);
#endif
            }
            else
                webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, m_Options.Crc);

            if (m_Options.Timeout > 0)
                webRequest.timeout = m_Options.Timeout;
            if (m_Options.RedirectLimit > 0)
                webRequest.redirectLimit = m_Options.RedirectLimit;
#if !UNITY_2019_3_OR_NEWER
            webRequest.chunkedTransfer = m_Options.ChunkedTransfer;
#endif
            if (m_ProvideHandle.ResourceManager.CertificateHandlerInstance != null)
            {
                webRequest.certificateHandler = m_ProvideHandle.ResourceManager.CertificateHandlerInstance;
                webRequest.disposeCertificateHandlerOnDispose = false;
            }
            return webRequest;
        }

        float PercentComplete() { return m_RequestOperation != null ? m_RequestOperation.progress : 0.0f; }

        DownloadStatus GetDownloadStatus()
        {
            if (m_Options == null)
                return default;
            var status = new DownloadStatus() { TotalBytes = m_BytesToDownload, IsDone = PercentComplete() >= 1f };
            if (m_BytesToDownload > 0)
            {
                if (m_WebRequestQueueOperation != null)
                    status.DownloadedBytes = (long)(m_WebRequestQueueOperation.m_WebRequest.downloadedBytes);
                else if (PercentComplete() >= 1.0f)
                    status.DownloadedBytes = status.TotalBytes;
            }
            return status;
        }

        /// <summary>
        /// Get the asset bundle object managed by this resource.  This call may force the bundle to load if not already loaded.
        /// </summary>
        /// <returns>The asset bundle.</returns>
        public AssetBundle GetAssetBundle()
        {
            if (m_AssetBundle == null && m_downloadHandler != null)
            {
                m_AssetBundle = m_downloadHandler.assetBundle;
                m_downloadHandler.Dispose();
                m_downloadHandler = null;
            }
            return m_AssetBundle;
        }

        internal void Start(ProvideHandle provideHandle)
        {
            m_Retries = 0;
            m_AssetBundle = null;
            m_downloadHandler = null;
            m_RequestOperation = null;
            m_ProvideHandle = provideHandle;
            m_Options = m_ProvideHandle.Location.Data as AssetBundleRequestOptions;
            if (m_Options != null)
                m_BytesToDownload = m_Options.ComputeSize(m_ProvideHandle.Location, m_ProvideHandle.ResourceManager);
            provideHandle.SetProgressCallback(PercentComplete);
            provideHandle.SetDownloadProgressCallbacks(GetDownloadStatus);
            BeginOperation();
        }

        private void BeginOperation()
        {
            string path = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
            if (File.Exists(path) || (Application.platform == RuntimePlatform.Android && path.StartsWith("jar:")))
            {
                m_RequestOperation = null;
                m_AssetBundle = AssetBundle.LoadFromFile(path, m_Options == null ? 0 : m_Options.Crc);
                m_ProvideHandle.Complete(this, m_AssetBundle != null, null);
            }
            else if (ResourceManagerConfig.ShouldPathUseWebRequest(path))
            {
                var req = CreateWebRequest(m_ProvideHandle.Location);
                req.disposeDownloadHandlerOnDispose = false;
                m_WebRequestQueueOperation = Helen.WebRequestQueue.QueueRequest(req);
                if (m_WebRequestQueueOperation.IsDone)
                {
                    m_RequestOperation = m_WebRequestQueueOperation.Result;
                    m_RequestOperation.completed += WebRequestOperationCompleted;
                }
                else
                {
                    m_WebRequestQueueOperation.OnComplete += asyncOp =>
                    {
                        m_RequestOperation = asyncOp;
                        m_RequestOperation.completed += WebRequestOperationCompleted;
                    };
                }
            }
            else
            {
                m_RequestOperation = null;
                m_ProvideHandle.Complete<AssetBundleResource>(null, false, new Exception(string.Format("Invalid path in ExtendedAssetBundleProvider: '{0}'.", path)));
            }
        }

        private void LocalRequestOperationCompleted(UnityEngine.AsyncOperation op)
        {
            m_AssetBundle = (op as AssetBundleCreateRequest).assetBundle;
            m_ProvideHandle.Complete(this, m_AssetBundle != null, null);
        }

        private void WebRequestOperationCompleted(UnityEngine.AsyncOperation op)
        {
            UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
            var webReq = remoteReq.webRequest;
            m_downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
#if ENABLE_CACHING
            // bundle crc mismatch 일때 핸들링. 대부분의 경우 crc 상이 오류.
            // 1. 빌드된 클라이언트의 crc과 서버의 번들 crc가 실제로 다를 때. (빌드만 하고 실수로 서버에 업로드 되지 않았을 때)
            // 2. hash 가 실제로 다른데 이전 버전의 번들을 그대로 사용할 때. hash가 실제로 같은데 다른것으로 확인 될 때가 있으므로. (추측)
            // Caching.IsVersionCached 참조.
            //if ((null == m_downloadHandler.assetBundle || (false == string.IsNullOrEmpty(webReq.error) && webReq.error.Contains("CRC Mismatch"))) && m_Retries++ < 1)
            //{
            //    Debug.LogWarningFormat("Failed to load Assetbundle(incorrect content, crc mismatch, etc). Clear cache and Retry. " + webReq.url);
            //    m_downloadHandler.Dispose();
            //    m_downloadHandler = null;
            //    string path = m_ProvideHandle.ResourceManager.TransformInternalId(m_ProvideHandle.Location);
            //    var bundleName = Path.GetFileNameWithoutExtension(path);
            //    Caching.ClearAllCachedVersions(bundleName);
            //    BeginOperation();
            //}
            //else
#endif
            {
                if (string.IsNullOrEmpty(webReq.error))
                {
                    m_ProvideHandle.Complete(this, true, null);
                }
                else
                {
                    m_downloadHandler.Dispose();
                    m_downloadHandler = null;
                    bool forcedRetry = false;
                    string message = string.Format("Web request {0} failed with error '{1}', retrying ({2}/{3})...", webReq.url, webReq.error, m_Retries, m_Options.RetryCount);
#if ENABLE_CACHING
                    if (!string.IsNullOrEmpty(m_Options.Hash))
                    {
                        CachedAssetBundle cab = new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
                        if (Caching.IsVersionCached(cab))
                        {
                            message = string.Format("Web request {0} failed to load from cache with error '{1}'. The cached AssetBundle will be cleared from the cache and re-downloaded. Retrying...", webReq.url, webReq.error);
                            Caching.ClearCachedVersion(cab.name, cab.hash);
                            if (m_Options.RetryCount == 0 && m_Retries == 0)
                            {
                                Debug.LogFormat(message);
                                BeginOperation();
                                m_Retries++; //Will prevent us from entering an infinite loop of retrying if retry count is 0
                                forcedRetry = true;
                            }
                        }
                    }
#endif
                    if (!forcedRetry)
                    {
                        if (m_Retries < m_Options.RetryCount)
                        {
                            Debug.LogFormat(message);
                            BeginOperation();
                            m_Retries++;
                        }
                        else
                        {
                            var exception = new Exception(string.Format(
                                "RemoteAssetBundleProvider unable to load from url {0}, result='{1}'.", webReq.url,
                                webReq.error));
                            m_ProvideHandle.Complete<AssetBundleResource>(null, false, exception);
                        }
                    }
                }
            }
            webReq.Dispose();
        }
        /// <summary>
        /// Unloads all resources associated with this asset bundle.
        /// </summary>
        public void Unload()
        {
            if (m_RequestOperation != null && m_RequestOperation.isDone == false)
            {
                Log.Trace("Do not unload bundle");
                return;
            }

            if (m_AssetBundle != null)
            {
                m_AssetBundle.Unload(true);
                m_AssetBundle = null;
            }
            if (m_downloadHandler != null)
            {
                m_downloadHandler.Dispose();
                m_downloadHandler = null;
            }
            m_RequestOperation = null;
        }
    }
    /// <summary>
    /// IResourceProvider for asset bundles.  Loads bundles via UnityWebRequestAssetBundle API if the internalId starts with "http".  If not, it will load the bundle via AssetBundle.LoadFromFileAsync.
    /// </summary>
    [DisplayName("AssetBundle(Sync) Provider")]
    public class ExtendedAssetBundleProvider : ResourceProviderBase
    {

        /// <inheritdoc/>
        public override void Provide(ProvideHandle providerInterface)
        {
            new AssetBundleResource().Start(providerInterface);
        }
        /// <inheritdoc/>
        public override Type GetDefaultType(IResourceLocation location)
        {
            return typeof(IAssetBundleResource);
        }

        /// <summary>
        /// Releases the asset bundle via AssetBundle.Unload(true).
        /// </summary>
        /// <param name="location">The location of the asset to release</param>
        /// <param name="asset">The asset in question</param>
        /// <returns></returns>
        public override void Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (asset == null)
            {
                Debug.LogWarningFormat("Releasing null asset bundle from location {0}.  This is an indication that the bundle failed to load.", location);
                return;
            }
            var bundle = asset as AssetBundleResource;
            if (bundle != null)
            {
                bundle.Unload();
                return;
            }
        }
    }
}
