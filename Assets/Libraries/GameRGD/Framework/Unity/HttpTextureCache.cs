using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

public static class HttpTextureCache
{
    private class RequestInfo
    {
        public string key;
        public string url;
        public int version;
        public RawImage image;
        public Action<bool> callback;
        public bool canceled;
    }

    private static readonly string _DIRECTORY_PATH = string.Format("{0}/{1}", Application.persistentDataPath, typeof(HttpTextureCache));

    private static Dictionary<string, WeakReference> _cache_dic = new Dictionary<string, WeakReference>();

    private static Coroutine _scheduler;
    private static List<RequestInfo> _scheduler_queue = new List<RequestInfo>();

    private static Texture2D _black_texture;

    public static Texture2D black_texture
    {
        get
        {
            if (_black_texture == null)
            {
                _black_texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

                for (int y = 0; y < _black_texture.height; y++)
                {
                    for (int x = 0; x < _black_texture.width; x++)
                    {
                        _black_texture.SetPixel(x, y, Color.black);
                    }
                }

                _black_texture.Apply();
            }
            return _black_texture;
        }
    }

    public static void SetCacheOrDownloadTextureAsync(Type type, int id, string url, int version, RawImage image, Action<bool> callback, bool is_force_request_png = false)
    {
        if (image == null)
            return;

        string key = string.Format("{0}_{1}", type, id);

        if (false == is_force_request_png)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            url = url + HttpTextureStream.pc_ext; // dxt
#elif UNITY_ANDROID
            url = url + HttpTextureStream.aos_ext; // etc2
#elif UNITY_IOS
            url = url + HttpTextureStream.ios_ext; // pvrtc
#else
            
#endif
        }
        else
        {
            url = url + ".png";
        }

        url = url.Trim();
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            if (callback != null)
                callback.SafeInvoke(false);

            return;
        }

        int cache_version = PlayerPrefs.GetInt(key, -1);
        if (cache_version == version && _cache_dic.ContainsKey(key) && (_cache_dic[key].Target as Texture2D) != null)
        {
            image.texture = _cache_dic[key].Target as Texture2D;
            image.color = Color.white;

            if (callback != null)
                callback.SafeInvoke(true);

            return;
        }
        else
        {
            // 버전이 다르면 새로 다운받는다.
            _cache_dic.Remove(key);
        }

        image.texture = black_texture;

        _scheduler_queue.Add(new RequestInfo()
        {
            key = key,
            url = url,
            version = version,
            image = image,
            callback = callback,
            canceled = false,
        });

        if (_scheduler == null)
            _scheduler = MainThreadDispatcher.StartCoroutine(Schedule());
    }

    private static IEnumerator Schedule()
    {
        while (_scheduler_queue.Count > 0)
        {
            RequestInfo request = _scheduler_queue.FirstOrDefault();
            if (request == null || request.canceled)
            {
                _scheduler_queue.Remove(request);
                continue;
            }

            yield return LoadFromCacheOrDownloadTexture(request.key, request.url, request.version, (texture) =>
            {
                _scheduler_queue.Remove(request);

                _cache_dic[request.key] = new WeakReference(texture);

                if (!request.canceled)
                {
                    if (request.image && texture)
                    {
                        request.image.texture = texture;
                        request.image.color = Color.white;
                    }

                    if (request.callback != null)
                        request.callback.SafeInvoke(texture != null);
                }
            });
        }

        _scheduler = null;
    }

    private static IEnumerator LoadFromCacheOrDownloadTexture(string key, string url, int version, Action<Texture2D> callback)
    {
        int point_index = url.LastIndexOf('.');
        string extension = point_index <= 0 ? "" : url.Substring(point_index, url.Length - point_index);
        if (extension != HttpTextureStream.aos_ext &&
            extension != HttpTextureStream.ios_ext &&
            extension != HttpTextureStream.pc_ext &&
            extension != ".png")
        {
            if (callback != null)
                callback.SafeInvoke(black_texture);
            Log.WarningFormat("texture download error. wrong texture format. key:{0}, url:{1}", key, url);
            yield break;
        }

        string file_path = string.Format("{0}/{1}{2}", _DIRECTORY_PATH, key, extension);

        int cache_version = PlayerPrefs.GetInt(key, -1);
        Log.TraceFormat("texture Cache key:{0}, local version:{1}, server version:{2}", key, cache_version, version);
        if (cache_version == version)
        {
            UriBuilder uri_bulder = new UriBuilder(file_path)
            {
                Scheme = "file"
            };
            using (UnityWebRequest www_local = UnityWebRequest.Get(uri_bulder.Uri.AbsoluteUri))
            {
                www_local.SetRequestHeaderNoCache();
                yield return www_local.SendWebRequest();

                if (www_local.isNetworkError)
                {
                    Log.WarningFormat("local texture load error. key:{0}, url:{1} error:{2}", key, url, www_local.error);
                }
                else
                {
                    Log.TraceFormat("texture loadded in cache. key:{0}", key);

                    if (callback != null)
                    {
                        Texture2D texture = HttpTextureStream.Read(((DownloadHandlerBuffer)www_local.downloadHandler).data);
                        callback.SafeInvoke(texture);
                    }
                    yield break;
                }
            }
        }

        yield return UnityEngine.Web.HttpUtility.DownloadBufferAsync(url, (buffer) =>
        {
            Texture2D texture = null;
            if (null == buffer)
            {
                Log.WarningFormat("texture download error. key:{0}, url:{1}", key, url);
                texture = black_texture;
            }
            else
            {
                texture = HttpTextureStream.Read(buffer);

                if (!Directory.Exists(_DIRECTORY_PATH))
                    Directory.CreateDirectory(_DIRECTORY_PATH);

                if (File.Exists(file_path))
                    File.Delete(file_path);

                File.WriteAllBytes(file_path, buffer);

                PlayerPrefs.SetInt(key, version);

                Log.TraceFormat("texture downloaded key:{0}", key);
            }

            if (callback != null)
                callback.SafeInvoke(texture);
        });
    }
}