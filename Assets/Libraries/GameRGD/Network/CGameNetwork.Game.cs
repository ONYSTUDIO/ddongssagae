using System;
using UniRx;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public partial class CGameNetwork : CActionMonoBehaviour
    {
        public enum LOGIN_TYPE
        {
            FACEBOOK = 1,
            GUEST = 10,
        }

        // 0 : facebook / 1 : guest /
        private LOGIN_TYPE m_LoginType = LOGIN_TYPE.GUEST;
        public LOGIN_TYPE LoginType { get => m_LoginType; }

        private INetworkController m_NetworkController;
        public INetworkController SetNetworkController { set => m_NetworkController = value; }

        // 사용자 기본 정보.
        private string m_UserId = ""; // 페이스북 UserID
        public string UserId { get => m_UserId; }
        public string SetUserId { set => m_UserId = value; }

        private string m_UserKey = "";
        public string UserKey { get => m_UserKey; }
        private string m_UserEmail;
        public string UserEmail { get => m_UserEmail; }

        private CSafeNumber m_Useridx = new CSafeNumber(0);
        public CSafeNumber Useridx { get => m_Useridx; }
        private CSafeNumber m_UserCoin = new CSafeNumber(0);
        public CSafeNumber UserCoin { get => m_UserCoin; }

        private CSafeBool m_IsAbused = new CSafeBool(false);
        public CSafeBool IsAbused { get => m_IsAbused; }
        private CSafeInt m_AbuseTermHour = new CSafeInt(0);
        public CSafeInt AbuseTermHour { get => m_AbuseTermHour; }

        private CSafeInt m_RequestIdx = new CSafeInt(CMiscFunc.SafeRandom());
        public CSafeInt RequestIdx { get => m_RequestIdx; }

        private ReactiveProperty<string> m_Nickname = new ReactiveProperty<string>();
        public IObservable<string> NicknameAsOnChangeObservable() => m_Nickname.AsObservable();
        public string Nickname
        {
            get => m_Nickname.Value;
            set => m_Nickname.Value = value;
        }

        private ReactiveProperty<string> m_Photourl = new ReactiveProperty<string>();
        public IObservable<string> PhotourlAsOnChangeObservable() => m_Photourl.AsObservable();
        public string Photourl { get => m_Photourl.Value; }
        public string SetPhotourl { set => m_Photourl.Value = value; }

        private int m_Sex;
        public int Sex { get => m_Sex; }

        public bool Login()
        {
            return m_NetworkController.Login(this);
        }

        private void OnLoginReceive()
        {
            m_NetworkController.OnLoginReceive(this);
        }

        public void ActionMonoEnqueAction(Action action)
        {
            EnqueAction(action);
        }

        private readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public long GetCurrentUnixTimestampMillis()
        {
            DateTime localDateTime, univDateTime;
            localDateTime = DateTime.Now;
            univDateTime = localDateTime.ToUniversalTime();
            return (long)(univDateTime - UnixEpoch).TotalMilliseconds;
        }

        public bool SendHeartBeat()
        {
            CLogger.Log("SendHeartBeat()");
            if (m_IsReconnect)
            {
                CLogger.Log("SendHeartBeat() / reconnect. so, return");
                return false;
            }

            if (CheckQueuedRequest(CNetworkConstants.CMD_HEART_BEAT) || m_CurrentCmd == CNetworkConstants.CMD_HEART_BEAT)
            {
                CLogger.Log("SendHeartBeat() / current command 3. so, return.");
                return false;
            }

            CPacketData _packetData = new CPacketData(CNetworkConstants.CMD_HEART_BEAT, m_Useridx.Number);
            m_Socket.PushRequestToQueue(_packetData);

            return true;
        }
    }

    public interface IProtocol
    {
        Type ResponseType { set; }
        int Cmd { get; set; }

        void Send(IProtocolParameter param = null);
        IObservable<object> SendAsObservableOnce(IProtocolParameter param = null);
        IObservable<object> OnResponseAsObservable();

        void Receive(CSocketBase socket);
    }

    public interface IProtocolParameter
    {

    }

    public interface IProtocolResponse
    {

    }

    public class ProtocolParameterBase : IProtocolParameter
    {
        public long useridx;
    }

    public class ProtocolErrorResponse : IProtocolResponse
    {
        public int error;

        public ProtocolErrorResponse(int error)
        {
            this.error = error;
        }
    }

    public class ProtocolAsyncOperation : UnityEngine.CustomYieldInstruction
    {
        public bool IsDone = false;
        public int Error;
        public object Result;

        public void OnFinished(int error, object result)
        {
            IsDone = true;
            Error = error;
            Result = result;
        }

        public override bool keepWaiting
        {
            get
            {

                return !IsDone;
            }
        }
    }
}
