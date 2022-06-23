#pragma warning disable 0162

using System;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class CAndroidManager : CPlatformManager
    {
        private static CAndroidManager sInstance = null;

#if UNITY_ANDROID
        private AndroidJavaObject androidjavaObject = null;
#endif

        public static CAndroidManager GetInstance()
        {
            if (sInstance == null)
            {
                sInstance = FindObjectOfType(typeof(CAndroidManager)) as CAndroidManager;
                if (sInstance == null)
                {
                    sInstance = new GameObject("CAndroidManager").AddComponent<CAndroidManager>();
                }

                DontDestroyOnLoad(sInstance);
            }

            return sInstance;
        }

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

#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || //Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                CLogger.Log("Awake() / platform not supported. platform : {0}", Application.platform);
                return;
            }
            
            CLogger.Log("Awake()");
            // #TODO 인앱결제 설정 오류로 인해 비활성
            // AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            // if (jc != null)
            // {
            //     androidjavaObject = jc.GetStatic<AndroidJavaObject> ("currentActivity");
            //     if (androidjavaObject != null)
            //     {
            //         bool isInAppBillingSetup = androidjavaObject.Call<bool>("isInAppBillingSetup");
            //         CLogger.Log("Awake() / call funcation : {0}", isInAppBillingSetup);
            //     }
            //     else
            //     {
            //         CLogger.Log("Awake() / android object is null.");
            //     }
            // }
#endif
        }

        public override void CheckPurchaseInventory()
        {
            base.CheckPurchaseInventory();
#if UNITY_ANDROID
            CLogger.Log("CheckPurchaseInventory()");
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            androidjavaObject.Call("checkPurchaseInventory");
#endif
        }

        public override int CheckRemainPurchaseInventory()
        {
            base.CheckRemainPurchaseInventory();
            int count = 0;

#if UNITY_ANDROID
            CLogger.Log("CheckRemainPurchaseInventory()");
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return 0;
            }
            
            count = androidjavaObject.Call<int>("checkRemainPurchaseInventory");
#endif

            return count;
        }

        public void TestOnPurchaseResut(string productId)
        {
            CPurchaseInfo purchaseInfo = new CPurchaseInfo();

            purchaseInfo.error = 0;
            purchaseInfo.product_id = productId;
            purchaseInfo.order_id = string.Format("12999763169054705758.1385661264739593{0}", System.DateTime.Now.Ticks);
            purchaseInfo.receipt = "12999763169054705758.1385661264739593";
            purchaseInfo.signature = "khwrAqWkNPWuQ8voeX2yE2H6eIykei8gMVo6kdYMHCRDlkOZLfMSGYHQibkkW/0Uwhp1CVG8fh1ZeGHj2OHkMZupc9z+I4By2jp2RXIixuwLPgGK0mrwPtRZLmqZgIk0l7/3GgAhGNE+PFoMtavFWX25VsPSgpfu9nwLTflyyFaCUKxpRWBkVx+sPVKeg9HrrAIzuTpYTzRSPBC2y0aq9HRqaPlld8X+hAjNzc3nM2MWyIPFyIow6YCifho9qy7cxri5U+4Nqe54kEsLC4SvEiKJyKWxAL12rf7qP3neAaBQHcifQlWnRQJ7t0XofGLX2GcScSP39bCXEGa9W6SWIg==";

            CLogger.Log("TestOnPurchaseResut() / purchase info : {0}", purchaseInfo);
            // CNotificationCenter.Instance.PostNotification(CConstants.ON_PURCHASE_RESULT, purchaseInfo);
        }

        public override void Purchase(string productId)
        {
            base.Purchase(productId);

#if UNITY_ANDROID
            EnqueAction(()=>DelayPurchase(productId), 0.5f);
#endif
        }

        private void DelayPurchase(string productId)
        {
#if UNITY_ANDROID
            CLogger.Log("DelayPurchase() / product id : {0}", productId);
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                CLogger.Log("DelayPurchase() / platform not supported. platform : {0}", Application.platform);
                EnqueAction(()=>TestOnPurchaseResut(productId));
                return;
            }
            
            androidjavaObject.Call("purchase", productId);
#endif
        }

        public override string GetRemainPurchaseInfo()
        {
            string purchaseInfo = "";

#if UNITY_ANDROID
            CLogger.Log("GetRemainPurchaseInfo()");
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
//				purchaseInfo = "{\"error\": 0, \"product_id\": \"com.doubleugames.dub.android.0100\", \"order_id\": \"" + string.Format("12999763169054705758.1385661264739593{0}", System.DateTime.Now.Ticks) + "\",";
//				purchaseInfo += "\"receipt\": \"12999763169054705758.1385661264739593\", \"signature\": \"khwrAqWkNPWuQ8voeX2yE2H6eIykei8gMVo6kdYMHCRDlkOZLfMSGYHQibkkW/0Uwhp1CVG8fh1ZeGHj2OHkMZupc9z+I4By2jp2RXIixuwLPgGK0mrwPtRZLmqZgIk0l7/3GgAhGNE+PFoMtavFWX25VsPSgpfu9nwLTflyyFaCUKxpRWBkVx+sPVKeg9HrrAIzuTpYTzRSPBC2y0aq9HRqaPlld8X+hAjNzc3nM2MWyIPFyIow6YCifho9qy7cxri5U+4Nqe54kEsLC4SvEiKJyKWxAL12rf7qP3neAaBQHcifQlWnRQJ7t0XofGLX2GcScSP39bCXEGa9W6SWIg==\" }";

                return purchaseInfo;
            }

            purchaseInfo = androidjavaObject.Call<string>("getRemainPurchaseInfo");
#endif
            return purchaseInfo;
        }

        public override void ClearPurchaseInfo()
        {
            base.ClearPurchaseInfo();
#if UNITY_ANDROID
            CLogger.Log("ClearPurchaseInfo()");
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }
            
            androidjavaObject.Call("removeRemainPurchaseInfo");
#endif
        }

        public override int GetNetworkStatus()
        {
            int status = 1;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return 1;
            }

            status = androidjavaObject.Call<int>("getNetworkStatus");
            CLogger.Log("GetNetworkStatus() / status : {0}", status);
#endif

            return status;
        }

        public override bool IsFacebookInstalled()
        {
#if UNITY_ANDROID
            CLogger.Log("IsFacebookInstalled()");

            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return false;
            }

            string FBpackageName = "com.facebook.katana"; 

            AndroidJavaObject packageManager = androidjavaObject.Call<AndroidJavaObject>( "getPackageManager" ); 
            AndroidJavaObject appList = packageManager.Call<AndroidJavaObject>( "getInstalledPackages", 0 ); 
            for (int i = 0; i < appList.Call<int>( "size" ); ++i) 
            { 
                AndroidJavaObject appInfo = appList.Call<AndroidJavaObject>( "get", i ); 
                string appPackageName = appInfo.Get<string>( "packageName" );
                if (FBpackageName.Equals(appPackageName)) 
                { 
                    return true; 
                } 
            } 
#endif
            return false;
        }

        public override void SetLocalPush(int type, int second)
        {
            base.SetLocalPush(type, second);

#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            CLogger.Log("SetLocalPush() / type {0}, time : {1}", type, second);
            androidjavaObject.Call("setLocalPush", type, second);
#endif
        }

        public override void ClearLocalPush(int type)
        {
            base.ClearLocalPush(type);
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }
            
            androidjavaObject.Call("clearLocalPush", type);
            CLogger.Log("ClearLocalPush() / type : {0}", type);
#endif
        }

        public override string GetMarketingDeviceId()
        {
            string marketingId = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return "unity_editor";
            }
            
            marketingId = androidjavaObject.Call<string>("getMarketingDeviceId");
            CLogger.Log("GetMarketingDeviceId() / id : {0}", marketingId);
#endif
            return marketingId;
        }

        public override string GetDeviceUUID()
        {
            string uuid = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return uuid = "EM_A";
            }

            CLogger.Log("GetDeviceUUID()");
            uuid = androidjavaObject.Call<string>("getDeviceUUID");
#endif
            return uuid;
        }

        public override string GetAppVersion()
        {
            string version = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return CPreferences.APP_VERSION;
            }
            
            version = Application.version;//androidjavaObject.Call<string>("getAppVersion");
#endif
            return version;
        }

        public override string GetPackageName()
        {
            string packageName = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return $"{CPreferences.PACKAGE_NAME}";
            }
            
            androidjavaObject.Call<string>("getAppPackageName");
#endif
            return packageName;
        }

        public override int GetDeviceCategory()
        {
            int category = 2;

#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return 2;
            }
            
            category = androidjavaObject.Call<int>("getDeviceCategory");
#endif
            return category;
        }

        public override string GetDeviceName()
        {
            string deviceName = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return deviceName;
            }
            
            deviceName = androidjavaObject.Call<string>("getModelName");
#endif
            return deviceName;
        }

        public override string GetOsVersion()
        {
            string osVersion = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return osVersion;
            }

            osVersion = androidjavaObject.Call<string>("getOSVersion");
#endif
            return osVersion;
        }

        public override int GetOsSDKVersion()
        {
            int sdkVersion = 0;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return sdkVersion;
            }
            
            sdkVersion = androidjavaObject.Call<int>("getSdkVersion");
#endif
            return sdkVersion;
        }

        public override long GetAvailableSpace()
        {
            long _availableSpace = 100 * 1024 * 1024; // 100M
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _availableSpace;
            }
            
            _availableSpace = androidjavaObject.Call<long>("getAvailableSpace");
#endif
            CLogger.Log("GetAvailableSpace() / size : {0}", _availableSpace);
            return _availableSpace;
        }

        public override string GetFanPageEventCode()
        {
            string _eventCode = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _eventCode;
            }
            
            _eventCode = androidjavaObject.Call<string>("getFanPageEventCode");
            CLogger.Log("GetFanPageEventCode() / event code : {0}", _eventCode);
#endif
            return _eventCode;
        }

        public override void ResetFanPageEventCode()
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }

            CLogger.Log("ResetFanPageEventCode()");
            androidjavaObject.Call("resetFanPageEventCode");
#endif
        }

        public override string GetAppNotiEventCode()
        {
            string _eventCode = "";
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _eventCode;
            }
            
            _eventCode = androidjavaObject.Call<string>("getAppNotiEventCode");
            CLogger.Log("GetAppNotiEventCode() / event code : {0}", _eventCode);
#endif
            return _eventCode;
        }

        public override void ResetAppNotiEventCode()
        {
            CPreferences.RemoveAppNotiEventCode();
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }
        
            CLogger.Log("ResetAppNotiEventCode()");
            androidjavaObject.Call("resetAppNotiEventCode");
#endif
        }

        public override bool IsTablet()
        {
#if UNITY_ANDROID
            float _screenHeightInInch =  Screen.height / Screen.dpi;
            if (_screenHeightInInch < 3.1)
            {
                return false;
                // it's a phone
            }
            else
            {
                return true;
                // it's tablet
            }
#endif
            return false;
        }

        public override void SendAppTrackingPurchaseEvent(int value, long orderidx)
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return;
            }
            
            CLogger.Log("SendAppTrackingPurchaseEvent() / value : {0} / orderidx : {1}", value, orderidx);
            string md5UserIdx = "";//CMiscFunc.GetMD5String(CUserInfo.Instance.Useridx.Number.ToString());

            androidjavaObject.Call("sendAppTrackingPurchaseEvent", md5UserIdx, orderidx.ToString(), value.ToString());
#endif
        }

        public override bool IsRootDevice()
        {
            bool _isRooted = false;

#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _isRooted;
            }
            
            _isRooted = androidjavaObject.Call<bool>("isRooted");
#endif

            CLogger.Log("IsRootDevice () / value : {0}", _isRooted);

            return _isRooted;
        }

        public override bool IsPushEnable()
        {
            bool _isPushEnable = true;

#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                return _isPushEnable;
            }
            
            _isPushEnable = androidjavaObject.Call<bool>("isPushEnable");
#endif

            CLogger.Log("IsPushEnable () / value : {0}", _isPushEnable);

            return _isPushEnable;
        }
    }
}
