using UnityEngine;
using System.Runtime.InteropServices;

namespace DoubleuGames.GameRGD
{
#if UNITY_IOS
    public class CiOSManager : CPlatformManager
    {
        private static string IOS_DEVICE_TOKEN = $"{CPreferences.PACKAGE_NAME}.IOS_DEVICE_TOKEN";
        private static string USE_PUSH_SETTING = $"{CPreferences.PACKAGE_NAME}.IOS_DEVICE_TOKEN";

        private static CiOSManager sInstance = null;

        private bool isUsePushSetting = false;
        private bool isSetDeviceToken = false;
        private string deviceToken = "";

        [DllImport("__Internal")]
        private static extern void iOSInAppInit();
        [DllImport("__Internal")]
        private static extern void iOSBuyItem(string strProductId);
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetRemainPurchaseInfo();
        [DllImport("__Internal")]
        private static extern void iOSClearPurchaseInfo();
        [DllImport("__Internal")]
        private static extern void iOSRestoreCompletedTransactions();
        [DllImport("__Internal")]
        private static extern void iOSSendMessage(string strMessage);
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetDeviceUUID();
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetDeviceID();
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetMarketingDeviceId();
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetAppVersion();
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetDeviceName();
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetOsVersion();
        [DllImport("__Internal")]
        private static extern long iOSGetAvailableSpace();
        [DllImport("__Internal")]
        private static extern bool iOSIsRootDevice();
        [DllImport("__Internal")]
        private static extern void iOSSendAppTrackingPurchaseEvent(string md5UserIdx, string orderidx, string value);
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetFanPageEventCode();
        [DllImport("__Internal")]
        private static extern void iOSResetFanPageEventCode();
        [DllImport("__Internal")]
        private static extern System.IntPtr iOSGetAppNotiEventCode();
        [DllImport("__Internal")]
        private static extern void iOSResetAppNotiEventCode();
        [DllImport("__Internal")]
        private static extern void iOSSetLocalPush(int type, int second);
        [DllImport("__Internal")]
        private static extern void iOSClearLocalPush(int type);
        [DllImport("__Internal")]
        private static extern bool iOSIsPushEnable();
        [DllImport("__Internal")]
        private static extern void iOSCleanBackupData();

        public static CiOSManager GetInstance()
        {
            if (sInstance == null)
            {
                sInstance = FindObjectOfType(typeof(CiOSManager)) as CiOSManager;
                if (sInstance == null)
                {
                    sInstance = new GameObject("CiOSManager").AddComponent<CiOSManager>();
                }

                DontDestroyOnLoad(sInstance);
            }

            return sInstance;
        }

        private string deviceId = null;
        private string marketDeviceId = null;

        protected override void OnAwakeAction()
        {
            base.OnAwakeAction();

            if (sInstance == null)
            {
                sInstance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                if (this != sInstance)
                {
                    Destroy(this.gameObject);
                }
            }

            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            iOSInAppInit();

            deviceToken = PlayerPrefs.GetString(IOS_DEVICE_TOKEN, "");

            int usePushSetting = PlayerPrefs.GetInt(USE_PUSH_SETTING, 0);
            if (usePushSetting == 1)
            {
                SettingPushNotification();
            }
        }

        protected override void OnUpdateAction()
        {
#if UNITY_IOS
            base.OnUpdateAction();

            if (isUsePushSetting == true)
            {
                if (isSetDeviceToken == false)
                {
                    byte[] _token = UnityEngine.iOS.NotificationServices.deviceToken;

                    if (_token != null)
                    {
                        deviceToken = System.BitConverter.ToString(_token).Replace("-", "");
                        CLogger.Log("OnUpdateAction () / device token : {0}", deviceToken);

                        PlayerPrefs.SetString(IOS_DEVICE_TOKEN, deviceToken);
                        m_OnUpdateRegistrationId.OnNext(deviceToken);
                        isSetDeviceToken = true;
                    }
                    else
                    {
                        CLogger.Log("OnUpdateAction () / token is null.");
                    }
                }
            }
#endif
        }

        public void TestOnPurchaseResut(string productId)
        {
            CPurchaseInfo purchaseInfo = new CPurchaseInfo();

            purchaseInfo.error = 0;
            purchaseInfo.product_id = productId;
            purchaseInfo.order_id = string.Format("1000000176{0}", System.DateTime.Now.Ticks % 1000000);
            purchaseInfo.receipt = "12999763169054705758.1385661264739593";
            purchaseInfo.signature = "khwrAqWkNPWuQ8voeX2yE2H6eIykei8gMVo6kdYMHCRDlkOZLfMSGYHQibkkW/0Uwhp1CVG8fh1ZeGHj2OHkMZupc9z+I4By2jp2RXIixuwLPgGK0mrwPtRZLmqZgIk0l7/3GgAhGNE+PFoMtavFWX25VsPSgpfu9nwLTflyyFaCUKxpRWBkVx+sPVKeg9HrrAIzuTpYTzRSPBC2y0aq9HRqaPlld8X+hAjNzc3nM2MWyIPFyIow6YCifho9qy7cxri5U+4Nqe54kEsLC4SvEiKJyKWxAL12rf7qP3neAaBQHcifQlWnRQJ7t0XofGLX2GcScSP39bCXEGa9W6SWIg==";

            CLogger.Log("TestOnPurchaseResut() / purchase info : {0}", purchaseInfo);
            CNotificationCenter.Instance.PostNotification(CGameConstants.ON_PURCHASE_RESULT, purchaseInfo);
        }

        public override void Purchase(string productId)
        {
            base.Purchase(productId);

            EnqueAction(() => DelayPurchase(productId), 0.5f);
        }

        private void DelayPurchase(string productId)
        {
#if UNITY_IOS
            CLogger.Log("DelayPurchase() / product id : {0}", productId);
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                CLogger.Log("DelayPurchase() / platform not supported. platform : {0}", Application.platform);
                EnqueAction(() => TestOnPurchaseResut(productId));
                return;
            }

            iOSBuyItem(productId);
#endif
        }

        public override string GetRemainPurchaseInfo()
        {
            string purchaseInfo = "";

#if UNITY_IOS
            CLogger.Log("GetRemainPurchaseInfo()");
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return purchaseInfo;
            }

            purchaseInfo = Marshal.PtrToStringAnsi(iOSGetRemainPurchaseInfo());
#endif
            return purchaseInfo;
        }

        public override void ClearPurchaseInfo()
        {
            base.ClearPurchaseInfo();
#if UNITY_IOS
            CLogger.Log("ClearPurchaseInfo()");
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            iOSClearPurchaseInfo();
#endif
        }

        public override int GetNetworkStatus()
        {
            int status = 1;
            CLogger.Log("GetNetworkStatus() / status : {0}", status);

            return status;
        }

        public override int GetDeviceCategory()
        {
            return 1;
        }

        public override string GetDeviceId()
        {
            if (string.IsNullOrEmpty(deviceId) == false)
            {
                CLogger.Log("GetDeviceId () / exist / device id : {0}", deviceId);
                return deviceId;
            }

#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                deviceId = SystemInfo.deviceUniqueIdentifier;
                return deviceId;
            }

            deviceId = Marshal.PtrToStringAnsi(iOSGetDeviceID());
            CLogger.Log("GetDeviceId () / device id : {0}", deviceId);
#endif
            return deviceId;
        }

        public override string GetMarketingDeviceId()
        {
            if (string.IsNullOrEmpty(marketDeviceId) == false)
            {
                CLogger.Log("GetMarketingDeviceId () / exist / device id : {0}", marketDeviceId);
                return marketDeviceId;
            }

#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                marketDeviceId = SystemInfo.deviceUniqueIdentifier;
                return "unity_editor";
            }

            marketDeviceId = Marshal.PtrToStringAnsi(iOSGetMarketingDeviceId());
            CLogger.Log("GetMarketingDeviceId () / device id : {0}", marketDeviceId);
#endif
            return marketDeviceId;
        }

        public override string GetDeviceUUID()
        {
            return deviceToken;
        }

        public override string GetAppVersion()
        {
            string version = "1.5.18";
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return CPreferences.APP_VERSION;
            }

            version = Marshal.PtrToStringAnsi(iOSGetAppVersion());
            CLogger.Log("GetAppVersion() / version : {0}", version);
#endif
            return version;
        }

        public override string GetDeviceName()
        {
            string deviceName = "";
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return deviceName;
            }

            deviceName = Marshal.PtrToStringAnsi(iOSGetDeviceName());
            CLogger.Log("GetDeviceName() / deviceName : {0}", deviceName);
#endif
            return deviceName;
        }

        public override string GetOsVersion()
        {
            string osVersion = "";
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return osVersion;
            }

            osVersion = Marshal.PtrToStringAnsi(iOSGetOsVersion());
            CLogger.Log("GetOsVersion() / osVersion : {0}", osVersion);
#endif
            return osVersion;
        }

        public override long GetAvailableSpace()
        {
            long _availableSpace = 100 * 1024 * 1024; // 100M
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _availableSpace;
            }

            _availableSpace = iOSGetAvailableSpace();
#endif
            CLogger.Log("GetAvailableSpace() / size : {0}", _availableSpace);
            return _availableSpace;
        }

        public override string GetFanPageEventCode()
        {
            string _eventCode = "";
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _eventCode;
            }

            _eventCode = Marshal.PtrToStringAnsi(iOSGetFanPageEventCode());
            CLogger.Log("GetFanPageEventCode() / event code : {0}", _eventCode);
#endif
            return _eventCode;
        }

        public override void ResetFanPageEventCode()
        {
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            CLogger.Log("ResetFanPageEventCode()");
            iOSResetFanPageEventCode();
#endif
        }

        public override string GetAppNotiEventCode()
        {
            string _eventCode = "";
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _eventCode;
            }

            _eventCode = Marshal.PtrToStringAnsi(iOSGetAppNotiEventCode());
            CLogger.Log("GetAppNotiEventCode() / event code : {0}", _eventCode);
#endif
            return _eventCode;
        }

        public override void ResetAppNotiEventCode()
        {
            CPreferences.RemoveAppNotiEventCode();
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            CLogger.Log("ResetAppNotiEventCode()");
            iOSResetAppNotiEventCode();
#endif
        }

        public override void SendMessageOfNative(string _message)
        {
            base.SendMessage(_message);

#if UNITY_IOS
            iOSSendMessage(_message);
#endif
        }
        public override bool IsTablet()
        {
            bool result = false;

#if UNITY_IOS
            if (UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPad1Gen)
                    || UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPad2Gen)
                    || UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPad4Gen)
                    || UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPad5Gen)
                    || UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPadAir2)
                    || UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPadMini1Gen)
                    || UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPadMini2Gen)
                    || UnityEngine.iOS.Device.generation.Equals(UnityEngine.iOS.DeviceGeneration.iPadMini3Gen))
            {
                result = true;
            }
#endif	
            return result;
        }

        public override bool IsRootDevice()
        {
            bool _isRooted = false;

#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _isRooted;
            }

            _isRooted = iOSIsRootDevice();
#endif

            CLogger.Log("IsRootDevice () / value : {0}", _isRooted);

            return _isRooted;
        }

        public override void SendAppTrackingPurchaseEvent(int value, long orderidx)
        {
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            CLogger.Log("SendAppTrackingPurchaseEvent() / value : {0} / orderidx : {1}", value, orderidx);
            //			string md5UserIdx = CMiscFunc.GetMD5String(CUserInfo.Instance.Useridx.Number.ToString());
            //
            //			iOSSendAppTrackingPurchaseEvent(md5UserIdx, orderidx.ToString(), value.ToString());
#endif
        }

        public override void SettingPushNotification()
        {
#if UNITY_IOS
            CLogger.Log("SettingPushNotification()() / use push setting value : {0}", isUsePushSetting);
            if (isUsePushSetting == false)
            {
                isUsePushSetting = true;
                UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert |
                                                                        UnityEngine.iOS.NotificationType.Badge |
                                                                        UnityEngine.iOS.NotificationType.Sound);

                PlayerPrefs.SetInt(USE_PUSH_SETTING, 1);
            }
#endif
        }

        public override bool IsPushEnable()
        {
            bool isEnable = true;

            int isSetting = PlayerPrefs.GetInt(USE_PUSH_SETTING, 0);
            if (isSetting != 1)
            {
                return true;
            }

#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return true;
            }

            isEnable = iOSIsPushEnable();
            CLogger.Log("IsPushEnable() / enable : {0}", isEnable);
#endif
            return isEnable;
        }

        public override void SetLocalPush(int type, int second)
        {
            base.SetLocalPush(type, second);

#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            iOSSetLocalPush(type, second);
            CLogger.Log("SetLocalPush() / type {0}, time : {1}", type, second);
#endif
        }

        public override void ClearLocalPush(int type)
        {
            base.ClearLocalPush(type);

#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            iOSClearLocalPush(type);
            CLogger.Log("ClearLocalPush() / type : {0}", type);
#endif
        }

        public override void CleanBackupData()
        {
            base.CleanBackupData();

#if UNITY_IOS
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            iOSCleanBackupData();
            CLogger.Log("CleanBackupData ()");
#endif
        }

    }
#endif
}
