#pragma warning disable 0618

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class CWebLogManager : CActionMonoBehaviour
    {
        private static CWebLogManager instance;
        public static CWebLogManager Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = GameObject.FindObjectOfType<CWebLogManager>();
                    DontDestroyOnLoad(instance);
                }
                
                return instance;
            }
        }
        
        private bool isSendFriendInfo = false;
        private string myIpAddress;
        
        protected override void OnAwakeAction ()
        {
            base.OnAwakeAction ();
            
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(instance);
            }
            else
            {
                if (instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
            
            DontDestroyOnLoad(this);
        }
        
        protected override void OnEnableAction ()
        {
            base.OnEnableAction ();
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_SOCKET_CONNECT, _=>UpdateIpAddress());
        }
        
        protected override void OnDisableAction ()
        {
            base.OnDisableAction ();
            CNotificationCenter.Instance.RemoveHandler(this);
        }
        
        public void ResetFriendInfo()
        {
            isSendFriendInfo = false;
        }
        
        public void SendFriendInfo(bool isInsert)
        {
            if (isSendFriendInfo)
            {
                return;
            }
            
            string url;
            if (isInsert)
            {
                url = CPreferences.FRIEND_INFO_INSERT_URL;
            }
            else
            {
                url = CPreferences.FRIEND_INFO_UPDATE_URL;
            }
            
            Dictionary<string,string> args = new Dictionary<string, string>();
            
            // args.Add("useridx", CUserInfo.Instance.Useridx.Number.ToString());
            // args.Add("userid", CUserInfo.Instance.UserID);
            
            StartCoroutine(HttpPost(url, args));
            isSendFriendInfo = true;
        }
        
        public void SendCommunicationLog(int cmd, string errCode, int latency)
        {
            // #FIXME JsonConvert 이용
            // StringBuilder dataString = new StringBuilder();
            // JsonWriter dataWriter = new JsonWriter(dataString);
            
            // dataWriter.WriteObjectStart();
            
            // dataWriter.WritePropertyName("data");
            
            // dataWriter.WriteArrayStart();
            // dataWriter.WriteObjectStart();
            
            // dataWriter.WritePropertyName("logtype");
            // dataWriter.Write("commlog");
            
            // dataWriter.WritePropertyName("value");
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("type");
            // dataWriter.Write(CUserInfo.Instance.OsType.Number.ToString());
            // dataWriter.WritePropertyName("ipaddress");
            // dataWriter.Write(GetIpAddress());
            // dataWriter.WritePropertyName("facebookid");
            // dataWriter.Write(CUserInfo.Instance.UserID);
            // dataWriter.WritePropertyName("cmd");
            // dataWriter.Write(cmd);
            // dataWriter.WritePropertyName("errcode");
            // dataWriter.Write(errCode);
            // dataWriter.WritePropertyName("latency");
            // dataWriter.Write(latency);
            // dataWriter.WriteObjectEnd();
            
            // dataWriter.WriteObjectEnd();
            // dataWriter.WriteArrayEnd();
            
            // dataWriter.WriteObjectEnd();
            
            // Dictionary<string,string> args = new Dictionary<string, string>();
            // args.Add("json_list", dataString.ToString());
            // SendWebLog(args);
        }
        
        public void SendDeviceActionLog(int platform, int action, int actionInfo)
        {
            // #FIXME JsonConvert 이용
            // StringBuilder dataString = new StringBuilder();
            // JsonWriter dataWriter = new JsonWriter(dataString);
            
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("data");
            
            // dataWriter.WriteArrayStart();
            // dataWriter.WriteObjectStart();
            
            // dataWriter.WritePropertyName("logtype");
            // dataWriter.Write("patternlog");
            
            // dataWriter.WritePropertyName("value");
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("ipaddress");
            // dataWriter.Write(GetIpAddress());
            // dataWriter.WritePropertyName("deviceid");
            // dataWriter.Write(CPlatformManager.Instance.GetDeviceId());
            // dataWriter.WritePropertyName("platform");
            // dataWriter.Write(platform);
            // dataWriter.WritePropertyName("action");
            // dataWriter.Write(action);
            // dataWriter.WritePropertyName("action_info");
            // dataWriter.Write(actionInfo);
            // dataWriter.WriteObjectEnd();
            
            // dataWriter.WriteObjectEnd();
            // dataWriter.WriteArrayEnd();
            // dataWriter.WriteObjectEnd();
            
            // Dictionary<string,string> args = new Dictionary<string, string>();
            // args.Add("json_list", dataString.ToString());
            
            // CLogger.Log("SendDeviceActionLog() / args : {0}", dataString.ToString());
            
            // SendWebLog(args);
        }
        
        public void SendDeviceActionLog(string category, string categoryInfo, string action, string actionInfo, int stayTime)
        {
            // #FIXME JsonConvert 이용
            // StringBuilder dataString = new StringBuilder();
            // JsonWriter dataWriter = new JsonWriter(dataString);
            
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("data");
            
            // dataWriter.WriteArrayStart();
            // dataWriter.WriteObjectStart();
            
            // dataWriter.WritePropertyName("logtype");
            // dataWriter.Write("actionpatternlog");
            
            // dataWriter.WritePropertyName("value");
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("ipaddress");
            // dataWriter.Write(GetIpAddress());
            // dataWriter.WritePropertyName("deviceid");
            // dataWriter.Write(CPlatformManager.Instance.GetDeviceId());
            // dataWriter.WritePropertyName("category");
            // dataWriter.Write(category);
            // dataWriter.WritePropertyName("category_info");
            // dataWriter.Write(categoryInfo);
            // dataWriter.WritePropertyName("action");
            // dataWriter.Write(action);
            // dataWriter.WritePropertyName("action_info");
            // dataWriter.Write(actionInfo);
            // dataWriter.WritePropertyName("stayTime");
            // dataWriter.Write(stayTime);
            // dataWriter.WriteObjectEnd();
            
            // dataWriter.WriteObjectEnd();
            // dataWriter.WriteArrayEnd();
            // dataWriter.WriteObjectEnd();
            
            // Dictionary<string,string> args = new Dictionary<string, string>();
            // args.Add("json_list", dataString.ToString());
            // SendWebLog(args);
        }
        
        public void SendActionLog(string category, string categoryInfo, string action, string actionInfo, int stayTime, int isPlayNow = 0)
        {
            // #FIXME JsonConvert 이용
            // StringBuilder dataString = new StringBuilder();
            // JsonWriter dataWriter = new JsonWriter(dataString);
            
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("data");
            
            // dataWriter.WriteArrayStart();
            // dataWriter.WriteObjectStart();
            
            // dataWriter.WritePropertyName("logtype");
            // dataWriter.Write("actionlog");
            
            // dataWriter.WritePropertyName("value");
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("type");
            // dataWriter.Write(CUserInfo.Instance.OsType.Number.ToString());
            // dataWriter.WritePropertyName("ipaddress");
            // dataWriter.Write(GetIpAddress());
            // dataWriter.WritePropertyName("facebookid");
            // dataWriter.Write(CUserInfo.Instance.UserID);
            // dataWriter.WritePropertyName("category");
            // dataWriter.Write(category);
            // dataWriter.WritePropertyName("category_info");
            // dataWriter.Write(categoryInfo);
            // dataWriter.WritePropertyName("action");
            // dataWriter.Write(action);
            // dataWriter.WritePropertyName("action_info");
            // dataWriter.Write(actionInfo);
            // dataWriter.WritePropertyName("stayTime");
            // dataWriter.Write(stayTime);
            // dataWriter.WritePropertyName("isplaynow");
            // dataWriter.Write(isPlayNow);
            // dataWriter.WritePropertyName("coin");
            // dataWriter.Write(CUserInfo.Instance.Coin.Number);
            // dataWriter.WritePropertyName("credit");
            // dataWriter.Write(CUserInfo.Instance.Credit.Number);
            // dataWriter.WriteObjectEnd();
            
            // dataWriter.WriteObjectEnd();
            // dataWriter.WriteArrayEnd();
            // dataWriter.WriteObjectEnd();
            
            // Dictionary<string,string> args = new Dictionary<string, string>();
            // args.Add("json_list", dataString.ToString());
            // SendWebLog(args);
        }
        
        public void SendExceptionLog(string errorLog)
        {
            // #FIXME JsonConvert 이용
            // if (errorLog != null && errorLog.Length > 1024)
            // {
            //     errorLog = errorLog.Substring(0, 1024);
            // }
            
            // StringBuilder dataString = new StringBuilder();
            // JsonWriter dataWriter = new JsonWriter(dataString);
            
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("data");
            
            // dataWriter.WriteArrayStart();
            // dataWriter.WriteObjectStart();
            
            // dataWriter.WritePropertyName("logtype");
            // dataWriter.Write("actionerrlog");
            
            // dataWriter.WritePropertyName("value");
            // dataWriter.WriteObjectStart();
            // dataWriter.WritePropertyName("type");
            // dataWriter.Write(CPlatformManager.Instance.GetDeviceCategory());
            // dataWriter.WritePropertyName("deviceid");
            // dataWriter.Write(CPlatformManager.Instance.GetDeviceId());
            // dataWriter.WritePropertyName("error_info");
            // dataWriter.Write(errorLog);
            // dataWriter.WriteObjectEnd();
            
            // dataWriter.WriteObjectEnd();
            // dataWriter.WriteArrayEnd();
            // dataWriter.WriteObjectEnd();
            
            // Dictionary<string,string> args = new Dictionary<string, string>();
            // args.Add("json_list", dataString.ToString());
            // SendWebLog(args);
        }
        
        private void SendWebLog(Dictionary<string, string> args)
        {
            StartCoroutine(HttpPost(CPreferences.WEB_LOG_URL, args));
        }
        
        // not use. suspension
        public void SendPurchaseInfo(CPurchaseInfo info)
        {
            Dictionary<string,string> args = new Dictionary<string, string>();
            args.Add("orderNo", info.order_id);
            args.Add("receipt", info.receipt);
            args.Add("signature", info.signature);
            
            StartCoroutine(HttpPost(CPreferences.WEB_PURCHASE_LOG_URL, args));
        }

        private IEnumerator HttpPost(string url, Dictionary<string, string> args)
        {
            yield return null;
            
            WWWForm form = new WWWForm();
            foreach(KeyValuePair<String,String> post_arg in args)
            {
                form.AddField(post_arg.Key, post_arg.Value);
            }
            WWW www = new WWW(url, form);
            yield return www;
            
            if (www.error != null)
            {
                CLogger.Log("HttpPost() / result error : {0}", www.error);
            } 
            
            yield break;
        }


        private string GetIpAddress()
        {
            if (string.IsNullOrEmpty(myIpAddress))
            {
                try
                {
                    myIpAddress =  CMiscFunc.GetLocalIPAddress();
                }
                catch(Exception e)
                {
                    CLogger.Log("GetIpAddress() / error : {0}", e.Message);
                }
            }
            
            return myIpAddress;
        }
        private void UpdateIpAddress()
        {
            myIpAddress = "";
        }
    }
}
