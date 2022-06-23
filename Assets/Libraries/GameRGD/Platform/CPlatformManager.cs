using System;
using UniRx;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public abstract class CPlatformManager : CActionMonoBehaviour
    {
        private static CPlatformManager instance;
        public static CPlatformManager Instance
        {
            get
            {
                if (instance == null)
                {
                    if (isDestory)
                    {
                        return null;
                    }
#if UNITY_ANDROID
                    instance = CAndroidManager.GetInstance();
#elif UNITY_IOS
                    instance = CiOSManager.GetInstance();
#endif
                }

                return instance;
            }
        }

        private static bool isDestory = false;

        protected Subject<string> m_OnUpdateRegistrationId = new Subject<string>();
        public IObservable<string> ObserveUpdateRegistrationId() => m_OnUpdateRegistrationId.AsObservable();

        protected override void OnDestroyAction()
        {
            base.OnDestroyAction();

            isDestory = true;
            CLogger.LogWarning("OnDestroyAction ()");
        }

        public virtual string GetFanPageEventCode()
        {
            return "";
        }

        public virtual string GetAppNotiEventCode()
        {
            return "";
        }

        public virtual void ResetAppNotiEventCode()
        {
        }

        public virtual void ResetFanPageEventCode()
        {
        }

        public virtual void CheckPurchaseInventory()
        {
        }

        public virtual int CheckRemainPurchaseInventory()
        {
            return 0;
        }

        public virtual void Purchase(string productId)
        {
            CGameNetwork.Instance.BlockLogin();
        }

        public virtual string GetRemainPurchaseInfo()
        {
            return "";
        }

        public virtual void ClearPurchaseInfo()
        {
        }

        public virtual int GetNetworkStatus()
        {
            return 1;
        }

        public virtual bool IsFacebookInstalled()
        {
            return false;
        }

        public virtual void ClearLocalPush(int type)
        {
        }

        public virtual void SetLocalPush(int type, int second)
        {
        }

        public virtual string GetAppVersion()
        {
            return "0.0.0";
        }

        public virtual string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public virtual string GetMarketingDeviceId()
        {
            return "";
        }

        public virtual string GetPackageName()
        {
            return "";
        }

        public virtual string GetDeviceUUID()
        {
            return "";
        }

        public abstract int GetDeviceCategory();

        public virtual string GetDeviceName()
        {
            return "";
        }

        public virtual string GetOsVersion()
        {
            return "";
        }

        public virtual int GetOsSDKVersion()
        {
            return 0;
        }

        public virtual long GetAvailableSpace()
        {
            // check size max is 100M.
            return 100 * 1024 * 1024;
        }

        public virtual void SendAppTrackingPurchaseEvent(int value, long orderidx)
        {
        }

        public virtual void SendMessageOfNative(string _message)
        {
        }

        public virtual bool IsTablet()
        {
            return false;
        }

        public virtual void SettingPushNotification()
        {
        }

        public virtual bool IsRootDevice()
        {
            return false;
        }

        public virtual void CleanBackupData()
        {
            // only use ios.
        }

        public virtual bool IsPushEnable()
        {
            return true;
        }

        /// <summary>
        /// Compares the version.
        /// target > clinet : 1, target == client : 0, target < client : -1
        /// </summary>
        /// <returns> </returns>
        /// <param name="_targetVersionString">target version string.</param>
        public int CompareVersion(string _targetVersionString)
        {
            string _clientVersionString = GetAppVersion();

            string[] _clientVersionCodes = _clientVersionString.Split('.');
            string[] _targetVersionCodes = _targetVersionString.Split('.');


            for (int i = 0; i < _clientVersionCodes.Length; i++)
            {
                int _clinetVersionValue = int.Parse(_clientVersionCodes[i]);
                int _targetVersionValue = int.Parse(_targetVersionCodes[i]);
                if (_targetVersionValue > _clinetVersionValue)
                {
                    return 1;
                }
                else if (_targetVersionValue < _clinetVersionValue)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}
