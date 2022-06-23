namespace DoubleuGames.GameRGD
{
    public class CGameConstants
    {
        public const string FILE_LOG_PATH = "/log";
        public const bool USE_FILE_LOG = false;

        public const int UNKNOWN_ID = -1;
        public const long UNKNOWN_IDX = -1L;
        public const long UNKNOWN_TYPE = -1L;

        public const int CHECK_PAUSE_TIME = 1 * 60 * 5 * 10000; // 5 MINUTE.
        public const int CHECK_DISCONNECT_TIME = 1 * 30 * 1000; // 30 second.

        // For UI
        public const string UI_EVENT_APPLICATION_PAUSE = "UI_EVENT_APPLICATION_PAUSE";
        public const string UI_EVENT_APPLICATION_RESUME = "UI_EVENT_APPLICATION_RESUME";

        // For Server
        public const string CHAT_CONNECT_PORT = "CHAT_CONNECT_PORT";
        public const string CHAT_SOCKET_CONNECT = "CHAT_SOCKET_CONNECT";
        public const string CHAT_SOCKET_RECONNECT = "CHAT_SOCKET_RECONNECT";
        public const string CHAT_SOCKET_CLOSE = "CHAT_SOCKET_CLOSE";
        public const string CHAT_SOCKET_IO_ERROR = "CHAT_SOCKET_IO_ERROR";
        public const string CHAT_SOCKET_DATA_READY = "CHAT_SOCKET_DATA_READY";
        public const string CHAT_SOCKET_DATA_PROCESSED = "CHAT_SOCKET_DATA_PROCESSED";

        // For UI
        public const string UI_EVENT_CHAT_SOCKET_DATA_READY = "UI_EVENT_CHAT_SOCKET_DATA_READY";
        public const string UI_EVENT_CHAT_SOCKET_DATA_PROCCESSED = "UI_EVENT_CHAT_SOCKET_DATA_PROCCESSED";
        public const string UI_EVENT_CHAT_SOCKET_CONNECT = "UI_EVENT_CHAT_SOCKET_CONNECT";
        public const string UI_EVENT_CHAT_SOCKET_IO_ERROR = "UI_EVENT_CHAT_SOCKET_IO_ERROR";
        public const string UI_EVENT_CHAT_SOCKET_RECONNECT_FAILED = "UI_EVENT_CHAT_SOCKET_RECONNECT_FAILED";
        public const string UI_EVENT_CHAT_SOCKET_DISCONNECT = "UI_EVENT_CHAT_SOCKET_DISCONNECT";

        // For Server
        public const string SOCKET_PORT = "SOCKET_PORT";
        public const string SOCKET_CONNECT = "SOCKET_CONNECT";
        public const string SOCKET_RECONNECT = "SOCKET_RECONNECT";
        public const string SOCKET_CLOSE = "SOCKET_CLOSE";
        public const string SOCKET_IO_ERROR = "SOCKET_IO_ERROR";
        public const string SOCKET_DATA_READY = "SOCKET_DATA_READY";
        public const string SOCKET_DATA_PROCESSED = "SOCKET_DATA_PROCESSED";

        public const string UI_EVENT_SOCKET_DATA_READY = "UI_EVENT_SOCKET_DATA_READY";
        public const string UI_EVENT_SOCKET_DATA_PROCCESSED = "UI_EVENT_SOCKET_DATA_PROCCESSED";
        public const string UI_EVENT_SOCKET_CONNECT = "UI_EVENT_SOCKET_CONNECT";
        public const string UI_EVENT_SOCKET_RECONNECT = "UI_EVENT_SOCKET_RECONNECT";
        public const string UI_EVENT_SOCKET_IO_ERROR = "UI_EVENT_SOCKET_IO_ERROR";
        public const string UI_EVENT_SOCKET_RECONNECT_FAILED = "UI_EVENT_SOCKET_RECONNECT_FAILED";
        public const string UI_EVENT_SOCKET_DISCONNECT = "UI_EVENT_SOCKET_DISCONNECT";
        public const string UI_EVENT_RETRY_COMMAND = "UI_EVENT_RETRY_COMMAND";
        public const string UI_EVENT_RETRY_COMMAND_COMPLETE = "UI_EVENT_RETRY_COMMAND_COMPLETE";

        public const string UI_EVENT_MAINTENANCE_NOTICE = "UI_EVENT_MAINTENANCE_NOTICE";
        public const string UI_EVENT_LOGIN_COMPLETE = "UI_EVENT_LOGIN_COMPLETE";
        public const string UI_EVENT_BLOCK_USER = "UI_EVENT_BLOCK_USER";

        public const string UI_EVENT_CHECK_MAIL_BOX_NOTICE = "UI_EVENT_CHECK_MAIL_BOX_NOTICE";
        public const string UI_EVENT_MAIL_DETAIL_BUTTON_SHOW = "UI_EVENT_MAIL_DETAIL_BUTTON_SHOW";
        public const string UI_EVENT_MAIL_DETAIL_BUTTON_HIDE = "UI_EVENT_MAIL_DETAIL_BUTTON_HIDE";

        public const string UI_EVENT_DUPLICATED_LOGIN = "UI_EVENT_DUPLICATED_LOGIN";

        public const string UI_EVENT_RELOGIN = "UI_EVENT_RELOGIN";
        public const string UI_EVENT_RELOGIN_COMPLETED = "UI_EVENT_RELOGIN_COMPLETED";

        // for purchase
        public const string ON_PURCHASE_RESULT = "ON_PURCHASE_RESULT";

    }
}