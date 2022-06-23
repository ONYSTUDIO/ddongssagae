using UnityEngine;

namespace DoubleuGames.GameRGD
{
    [PropertiesHolder]
    public class CPreferences
    {
        public enum SERVER
        {
            REAL,
            BETA,
            DEV
        };

        public enum ANDROID_STORE_TYPE
        {
            GOOGLE,
            AMAZON,
        }

        public enum NETWORK_TYPE
        {
            NONE,
            LOCAL,
            API_REMOTE,
            API_LOCAL,
        }

        public const string PACKAGE_NAME = "com.doubleugames.projectn";
        public static string APP_VERSION = "1.0.0";

        public static SERVER TargetServer = SERVER.DEV;
        public static string FRIEND_INFO_INSERT_URL;
        public static string FRIEND_INFO_UPDATE_URL;
        public static string WEB_LOG_URL;
        public static string WEB_PURCHASE_LOG_URL;

        public static bool IsApplicationPause = false;

        public static bool IsUseAddressable = true;

        private static string m_AppNotiEventCode = "";

        public static void RemoveAppNotiEventCode()
        {
            if (PlayerPrefs.HasKey("appNotiEventCode"))
            {
                string oldStr = PlayerPrefs.GetString("appNotiEventCode");
                oldStr = oldStr.Replace(m_AppNotiEventCode + "|", "");
                PlayerPrefs.SetString("appNotiEventCode", oldStr);
            }
            m_AppNotiEventCode = "";
        }

        private static NETWORK_TYPE m_NetworkTypeEnum;
        public static NETWORK_TYPE SetNetworkType { set => m_NetworkTypeEnum = value; }
        public static NETWORK_TYPE NetworkType => m_NetworkTypeEnum;


        [Value("doubluegames.network.api-remote.register-service-url")]
        private static string m_NetworkApiRemoteRegisterServiceUrl;

        [Value("doubluegames.network.api-remote.login-service-url")]
        private static string m_NetworkApiRemoteLoginServiceUrl;

        [Value("doubluegames.network.api-remote.command-service-url")]
        private static string m_NetworkApiRemoteCommandServiceUrl;

        [Value("doubluegames.network.api-remote.refresh-service-url")]
        private static string m_NetworkApiRemoteRefreshServiceUrl;


        [Value("doubluegames.network.api-local.register-service-url")]
        private static string m_NetworkApiLocalRegisterServiceUrl;

        [Value("doubluegames.network.api-local.login-service-url")]
        private static string m_NetworkApiLocalLoginServiceUrl;

        [Value("doubluegames.network.api-local.command-service-url")]
        private static string m_NetworkApiLocalCommandServiceUrl;

        [Value("doubluegames.network.api-local.refresh-service-url")]
        private static string m_NetworkApiLocalRefreshServiceUrl;

        public static string NetworkApiRegisterServiceUrl
        {
            get
            {
                if (m_NetworkTypeEnum == NETWORK_TYPE.API_REMOTE) return m_NetworkApiRemoteRegisterServiceUrl;
                else return m_NetworkApiLocalRegisterServiceUrl;
            }
        }

        public static string NetworkApiLoginServiceUrl
        {
            get
            {
                if (m_NetworkTypeEnum == NETWORK_TYPE.API_REMOTE) return m_NetworkApiRemoteLoginServiceUrl;
                else return m_NetworkApiLocalLoginServiceUrl;
            }
        }

        public static string NetworkApiCommandServiceUrl
        {
            get
            {
                if (m_NetworkTypeEnum == NETWORK_TYPE.API_REMOTE) return m_NetworkApiRemoteCommandServiceUrl;
                else return m_NetworkApiLocalCommandServiceUrl;
            }
        }

        public static string NetworkApiRefreshServiceUrl
        {
            get
            {
                if (m_NetworkTypeEnum == NETWORK_TYPE.API_REMOTE) return m_NetworkApiRemoteRefreshServiceUrl;
                else return m_NetworkApiLocalRefreshServiceUrl;
            }
        }
    }
}
