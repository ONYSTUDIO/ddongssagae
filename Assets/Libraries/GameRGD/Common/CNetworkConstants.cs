namespace DoubleuGames.GameRGD
{
    public class CNetworkConstants
    {
        /// <summary>
        /// 로그인
        /// </summary>
        public const int CMD_LOGIN = 1;

        /// <summary>
        /// 재로그인
        /// </summary>
        public const int CMD_RELOGIN = 2;

        /// <summary>
        /// 하트비트
        /// </summary>
        public const int CMD_HEART_BEAT = 3;

        /// <summary>
        /// 로그아웃
        /// </summary>
        public const int CMD_LOGOUT = 4;

        /// <summary>
        /// 로그인 체크
        /// </summary>
        public const int CMD_CHECK_LOGIN = 5;

        /// <summary>
        /// 로그인 블록
        /// </summary>
        public const int CMD_BLOCK_LOGIN = 6;

        /// <summary>
        /// 로그인 블록
        /// </summary>
        public const int CMD_UNBLOCK_LOGIN = 7;

        /// <summary>
        /// 채팅 로그인
        /// </summary>
        public const int CMD_CHAT_LOGIN = 501;

        /// <summary>
        /// 채팅 인덱스 조회
        /// </summary>
        public const int CMD_CHAT_GET_INDEX = 502;

        /// <summary>
        /// 채팅 정보 조회
        /// </summary>
        public const int CMD_CHAT_GET_INFO = 504;
    }
}