using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public static class HttpHelper
{
    public static void SetRequestHeaderNoCache(this UnityWebRequest webRequest)
    {
        webRequest.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
        webRequest.SetRequestHeader("Pragma", "no-cache");
    }

    public static void SetRequestDeclaredOnly(this UnityWebRequest webRequest)
    {
        webRequest.redirectLimit = 0;
        webRequest.useHttpContinue = false;
    }

    public static async UniTask SendWebRequestWithoutException(this UnityWebRequest webRequest)
    {
        // UnityWebReqeustException 처리.
        { // 예외 처리
            try { await webRequest.SendWebRequest(); }
            catch (UnityWebRequestException) { }
        }
        //{ // 직접 관리
        //    var ayncOperation = webRequest.SendWebRequest();
        //    while (!ayncOperation.isDone)
        //        await UniTask.Yield();
        //}
    }
}