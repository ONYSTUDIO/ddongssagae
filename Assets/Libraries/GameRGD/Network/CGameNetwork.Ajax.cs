using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DoubleuGames.GameRGD
{
    public partial class CGameNetwork : CActionMonoBehaviour
    {
        private string m_AccessToken = default;

        private string m_RefreshToken = default;

        public async UniTask<RegisterResponse> register()
        {
            var uri = CPreferences.NetworkApiRegisterServiceUrl;

            var parameter = new RegisterParameter
            {
                deviceId = CPlatformManager.Instance.GetDeviceId(),
                deviceType = CPlatformManager.Instance.GetDeviceCategory(),
            };
            var json = JsonConvert.SerializeObject(parameter);
            CLogger.Log($"register params: {json}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var result = null as RegisterResponse;

            using (var www = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST))
            {
                var uH = new UploadHandlerRaw(bytes);
                var dH = new DownloadHandlerBuffer();

                www.uploadHandler = uH;
                www.downloadHandler = dH;
                www.SetRequestHeader("Content-Type", "application/json");
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.ProtocolError && www.result != UnityWebRequest.Result.ConnectionError)
                {
                    result = JsonConvert.DeserializeObject<RegisterResponse>(www.downloadHandler.text);
                    CLogger.Log(www.downloadHandler.text);
                }
            }

            return result;
        }

        public async UniTask<AjaxResponseResult> Login(IProtocolParameter param, Type responseType)
        {
            var uri = CPreferences.NetworkApiLoginServiceUrl;

            var json = JsonConvert.SerializeObject(param);
            CLogger.Log($"login url: {uri} / params: {json}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var result = null as AjaxResponseResult;

            using (var www = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST))
            {
                var uH = new UploadHandlerRaw(bytes);
                var dH = new DownloadHandlerBuffer();

                www.uploadHandler = uH;
                www.downloadHandler = dH;
                www.SetRequestHeader("Content-Type", "application/json");
                try
                {
                    await www.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    CLogger.LogError(e.Message);
                }

                if (www.result != UnityWebRequest.Result.ProtocolError && www.result != UnityWebRequest.Result.ConnectionError)
                {
                    m_AccessToken = www.GetResponseHeader("X-DUG-Authentication");
                    m_RefreshToken = www.GetResponseHeader("X-DUG-Refresh");

                    var jso = JObject.Parse(www.downloadHandler.text);
                    var response = JsonConvert.DeserializeObject(jso["res"].ToString(), responseType) as IProtocolResponse;
                    CLogger.Log(jso["res"].ToString());
                    result = new AjaxResponseResult
                    {
                        err = jso.ContainsKey("err") ? (int)jso["err"] : 0,
                        msg = jso.ContainsKey("msg") ? (string)jso["msg"] : null,
                        res = response
                    };
                }
            }

            return result;
        }

        public async UniTask<AjaxResponseResult> Command(int cmd, IProtocolParameter param, Type responseType)
        {
            // 로그인 프로토콜의 경우 별도로 처리
            if (cmd == CNetworkConstants.CMD_LOGIN) return await Login(param, responseType);

            var uri = CPreferences.NetworkApiCommandServiceUrl;

            var parameter = new AjaxRequestParameter()
            {
                cmd = cmd,
                data = param
            };
            var json = JsonConvert.SerializeObject(parameter);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var result = null as AjaxResponseResult;

            CLogger.Log($"params: {json}");

            using (var www = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST))
            {
                var uH = new UploadHandlerRaw(bytes);
                var dH = new DownloadHandlerBuffer();

                www.uploadHandler = uH;
                www.downloadHandler = dH;
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {m_AccessToken}");
                try
                {
                    await www.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    if (e.ResponseCode == 410)
                    {
                        // 액세스 토큰 갱신
                        return await RefreshToken(cmd, param, responseType);
                    }
                    else
                    {
                        CLogger.LogError(e.Message);
                    }
                }

                if (www.result != UnityWebRequest.Result.ProtocolError && www.result != UnityWebRequest.Result.ConnectionError)
                {
                    CLogger.Log($"response: {www.downloadHandler.text}");
                    var jso = JObject.Parse(www.downloadHandler.text);
                    var response = jso.ContainsKey("res") ? JsonConvert.DeserializeObject(jso["res"].ToString(), responseType) as IProtocolResponse : null;
                    result = new AjaxResponseResult
                    {
                        err = jso.ContainsKey("err") ? (int)jso["err"] : 0,
                        msg = jso.ContainsKey("msg") ? (string)jso["msg"] : null,
                        res = response
                    };
                }
            }

            if (result.err != 0) CLogger.Log(result.msg);

            return result;
        }

        public async UniTask<AjaxResponseResult> RefreshToken(int cmd, IProtocolParameter param, Type responseType)
        {
            var uri = CPreferences.NetworkApiRefreshServiceUrl;

            using (var www = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbGET))
            {
                www.SetRequestHeader("Refresh", $"{m_RefreshToken}");
                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.ProtocolError && www.result != UnityWebRequest.Result.ConnectionError)
                {
                    m_AccessToken = www.GetResponseHeader("X-DUG-Authentication");
                    CLogger.Log($"update access token: {m_AccessToken}");
                }
            }

            return await Command(cmd, param, responseType);
        }
    }

    public class RegisterParameter
    {
        public string deviceId;
        public int deviceType;
    }

    public class RegisterResponse
    {
        public long useridx;
        public string userid;
        public string password;
    }

    public class AjaxRequestParameter
    {
        public int cmd;
        public IProtocolParameter data;
    }

    public class AjaxResponseResult
    {
        public int cmd;
        public int err;
        public string msg;
        public IProtocolResponse res;
    }
}
