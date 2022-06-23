using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using static DoubleuGames.GameRGD.CPreferences;

namespace DoubleuGames.GameRGD
{
    public partial class CGameNetwork : CActionMonoBehaviour
    {
        private const int HEART_BEAT_INTERVAL = 40000;
        private const int HEART_BEAT_MANAGEMENT_INTERVAL = 60000;

        public int m_CurrentCmd = -1;	// 현재 진행 중인 명령
        private int m_HeartBeatTick;

        private int m_ErrorCnt = 0;
        private int m_HeartBeatErrorCnt = 0;
        private int m_SocketCloseCount = 0;
        private int m_TimeoutCount = 0;
        public bool m_IsConnected = false;
        public int m_RetryUserCoinInfo = 0; // 9번패킷 실패시 한번 더 시도
        public CSafeInt m_OldUserCoin = new CSafeInt(0);

        // 소켓.
        private CSocketBase m_Socket = null;
        public CSocketBase Socket
        {
            get => m_Socket;
        }

        public int m_SocketNullTick = 0;

        //재접속  플래그.
        public bool m_bCanReconnect = true;
        public bool m_IsReconnect = false;
        private Timer m_ReconnectTimer = null;

        // 재접속 관련.
        private Timer m_CheckTimer;
        private Timer m_CheckSendTimer;
        private Timer m_CheckLiveTimer;

        private string m_SERVERIP = "";
        private int m_SERVERPORT = 0;
        private int[] m_PORTLIST;
        public ArrayList m_PendingRequestQueue = new ArrayList();

        public int m_LoginTryCount = 0;
        public bool m_bCoinUpdateForce = false;

        public int m_DisconnectState = 0;

        private Timer m_EnterFrameTimer;

        private bool m_IsFirstLogin = true;
        private bool m_IsMaintenanceNotified = false;

        private bool m_IsApplicationPause = false;
        private int m_PauseTime;

        private bool m_IsCheckRetryCommand = false;

        public CSafeInt m_SlotLogInRetryCount = new CSafeInt(0);
        public int m_LoadSlotNum;

        private Dictionary<int, IProtocol> m_NetWorkProtocols;

        private static CGameNetwork instance;
        public static CGameNetwork Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = GameObject.FindObjectOfType<CGameNetwork>();

                    DontDestroyOnLoad(instance);
                }

                return instance;
            }
        }

        protected override void OnAwakeAction()
        {
            base.OnAwakeAction();

            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                if (this != instance)
                {
                    Destroy(this.gameObject);
                }
            }

            if (CPreferences.TargetServer == SERVER.DEV)
            {
                m_SERVERIP = "ng-dev.doubleugames.com";
                m_PORTLIST = new int[] { 33000 };
            }
            else
            {
                m_SERVERIP = "ng-dev.doubleugames.com";
                m_PORTLIST = new int[] { 33000 };
            }

            m_SERVERPORT = m_PORTLIST[0];

            int savePort = PlayerPrefs.GetInt(CGameConstants.SOCKET_PORT, 0);

            if (savePort > 0)
            {
                bool isPortValid = false;
                foreach (int item in m_PORTLIST)
                {
                    if (item == m_SERVERPORT)
                    {
                        isPortValid = true;
                        break;
                    }
                }

                if (isPortValid)
                {
                    m_SERVERPORT = savePort;
                }
            }

            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_PAUSE, OnApplicationPause);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_RESUME, OnApplicationResume);
        }

        public void InitNetworkProtocols(List<IProtocol> protocols)
        {
            m_NetWorkProtocols = new Dictionary<int, IProtocol>();
            foreach (var protocol in protocols)
            {
                m_NetWorkProtocols.Add(protocol.Cmd, protocol);
                CLogger.Log("InitializeProtocol: {0} - {1}", protocol.Cmd, protocol);
            }

        }

        public IProtocol GetNetworkProtocol(int cmd)
        {
            return m_NetWorkProtocols[cmd];
        }

        public void InitCloseCount()
        {
            m_SocketCloseCount = 0;
        }

        public void InitFirstLogin()
        {
            m_IsFirstLogin = true;
        }

        private void OnApplicationPause(object param)
        {
            m_IsApplicationPause = true;
            m_PauseTime = 0;
        }

        private void OnApplicationResume(object param)
        {
            m_IsApplicationPause = false;
            m_PauseTime = 0;
        }

        protected override void OnDestroyAction()
        {
            base.OnDestroyAction();

            CLogger.LogWarning("OnDestroyAction() / network close.");
            Close();
        }

        public void Close()
        {
            CLogger.Log("Close()");
            CNotificationCenter.Instance.RemoveHandler(this);

            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_PAUSE, OnApplicationPause);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_RESUME, OnApplicationResume);

            if (m_CheckTimer != null)
            {
                m_CheckTimer.Stop();
                m_CheckTimer = null;
            }

            if (m_CheckSendTimer != null)
            {
                m_CheckSendTimer.Stop();
                m_CheckSendTimer = null;
            }

            if (m_CheckLiveTimer != null)
            {
                m_CheckLiveTimer.Stop();
                m_CheckLiveTimer = null;
            }

            if (m_EnterFrameTimer != null)
            {
                m_EnterFrameTimer.Stop();
                m_EnterFrameTimer = null;
            }

            try
            {
                if (m_Socket != null)
                {
                    m_Socket.Close();
                }
            }
            catch (Exception e)
            {
                CLogger.Log("Close() / Exception : " + e.Message);
            }

            m_Socket = null;
            m_IsConnected = false;
            m_CurrentCmd = -1;

            m_bCanReconnect = true;
            m_IsReconnect = false;

            m_LoginTryCount = 0;
            m_DisconnectState = 0;

            if (m_PendingRequestQueue.Count > 0)
            {
                m_PendingRequestQueue.Clear();
            }

            m_Useridx.Number = 0;
        }

        public void Connect()
        {
            if (m_IsConnected || m_IsReconnect)
            {
                CLogger.Log("Connect() / already or connecting... / is connect : {0}, is reconnect : {1}", m_IsConnected, m_IsReconnect);
                return;
            }

            CLogger.Log("Connect()");
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_CONNECT, OnSocketConnect);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_CLOSE, OnSocketClose);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_IO_ERROR, OnSocketError);

            m_Socket = new CSocketBase();
            m_Socket.Connect(m_SERVERIP, m_SERVERPORT);

            if (m_CheckTimer != null)
            {
                m_CheckTimer.Stop();
                m_CheckTimer = null;
            }

            if (m_CheckSendTimer != null)
            {
                m_CheckSendTimer.Stop();
                m_CheckSendTimer = null;
            }

            m_CheckSendTimer = new Timer();
            m_CheckSendTimer.Interval = 3000;
            m_CheckSendTimer.Elapsed += new ElapsedEventHandler(CheckSendTick);
            m_CheckSendTimer.Start();

            if (m_CheckLiveTimer != null)
            {
                m_CheckLiveTimer.Stop();
                m_CheckLiveTimer = null;
            }

            m_CheckLiveTimer = new Timer();
            m_CheckLiveTimer.Interval = 3000;
            m_CheckLiveTimer.Elapsed += new ElapsedEventHandler(CheckLiveTick);
            m_CheckLiveTimer.Start();

            CLogger.Log("Connect() / END");
        }

        public void ResumeConnect()
        {
            if (m_IsConnected || m_IsReconnect)
            {
                CLogger.Log("ResumeConnect() / already or connecting...");
                return;
            }

            m_bCanReconnect = true;

            CLogger.Log("ResumeConnect()");
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_CONNECT, OnSocketConnect);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_CLOSE, OnSocketClose);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_IO_ERROR, OnSocketError);

            if (m_Socket == null)
            {
                m_Socket = new CSocketBase();
            }

            m_Socket.Connect(m_SERVERIP, m_SERVERPORT);

            if (m_CheckTimer != null)
            {
                m_CheckTimer.Stop();
                m_CheckTimer = null;
            }

            if (m_CheckSendTimer != null)
            {
                m_CheckSendTimer.Stop();
                m_CheckSendTimer = null;
            }

            m_CheckSendTimer = new Timer();
            m_CheckSendTimer.Interval = 300;
            m_CheckSendTimer.Elapsed += new ElapsedEventHandler(CheckSendTick);
            m_CheckSendTimer.Start();

            if (m_CheckLiveTimer != null)
            {
                m_CheckLiveTimer.Stop();
                m_CheckLiveTimer = null;
            }

            m_CheckLiveTimer = new Timer();
            m_CheckLiveTimer.Interval = 300;
            m_CheckLiveTimer.Elapsed += new ElapsedEventHandler(CheckLiveTick);
            m_CheckLiveTimer.Start();

            CLogger.Log("ResumeConnect() / END");
        }

        public void PauseConnect()
        {
            CLogger.Log("PauseConnect()");
            CNotificationCenter.Instance.RemoveHandler(this);

            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_PAUSE, OnApplicationPause);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_RESUME, OnApplicationResume);

            if (m_ReconnectTimer != null)
            {
                m_ReconnectTimer.Stop();
                m_ReconnectTimer = null;
            }

            if (m_CheckTimer != null)
            {
                m_CheckTimer.Stop();
                m_CheckTimer = null;
            }

            if (m_CheckSendTimer != null)
            {
                m_CheckSendTimer.Stop();
                m_CheckSendTimer = null;
            }

            if (m_CheckLiveTimer != null)
            {
                m_CheckLiveTimer.Stop();
                m_CheckLiveTimer = null;
            }

            if (m_EnterFrameTimer != null)
            {
                m_EnterFrameTimer.Stop();
                m_EnterFrameTimer = null;
            }

            try
            {
                if (m_Socket != null)
                {
                    // 기존에 처리중이던 Request 재전송 용도로 저장.
                    byte[] currentCommandData = null;
                    m_Socket.GetCurrentCmd(out currentCommandData);

                    if (currentCommandData != null)
                    {
                        m_PendingRequestQueue.Add(currentCommandData);
                    }

                    ArrayList requestQueue = m_Socket.GetRequestQueue();
                    for (int i = 0; i < requestQueue.Count; i++)
                    {
                        m_PendingRequestQueue.Add(requestQueue[i]);
                    }

                    m_Socket.Close();
                    m_Socket = null;
                }
            }
            catch (Exception e)
            {
                CLogger.Log("PauseConnect() / Exception : " + e.Message);
            }

            m_CurrentCmd = -1;

            m_Socket = new CSocketBase();
            m_IsConnected = false;
            m_IsReconnect = false;
            m_bCanReconnect = false;
            CLogger.Log("PauseConnect() / End");
        }

        public void PauseNResumeNetwork()
        {
            //CLoadingPopup.Show();
            PauseConnect();

            EnqueAction(ResumeConnect, 1);
        }

        // 주기적으로 이루어지는 작업 처리.
        private void OnEnterFrame(object sender, ElapsedEventArgs args)
        {
            if (m_IsApplicationPause == true)
            {
                m_PauseTime += 100;

                if (m_PauseTime > CGameConstants.CHECK_PAUSE_TIME)
                {
                    CLogger.LogWarning("OnEnterFrame() / long pause. so, network close.");
                    Close();
                    return;
                }
            }

            //			if (CUserInfo.Instance.IsAbused.Number == true)
            //			{
            //				CLogger.Log("OnEnterFrame() / check abuse");
            //				
            //				//EnqueAction(()=>CBlockUserInfoPopup.Show());
            //				Close();
            //				return;
            //			}

            // 30초 내로 응답 오지 않으면 실패로 처리 (socket에 일부 패킷이 도착했을 때는 해당 시간 기준으로 체크)	// change 15 second.
            if (m_CurrentCmd != -1 && m_Socket.m_state == 2 && CMiscFunc.SafeGetTimer() - m_Socket.m_lastReceiveCheckTick > 15000)
            {
                CLogger.Log("OnEnterFrame() / Network Receive Timeout");
                if (m_Socket != null)
                {
                    int _cmd = m_Socket.m_cmd;
                    string _host = m_Socket.m_host;
                    int _port = m_Socket.m_port;

                    EnqueAction(() => CWebLogManager.Instance.SendCommunicationLog(_cmd, string.Format("network receive timeout ({0}:{1})", _host, _port), 0));
                }

                m_TimeoutCount++;
                if (m_TimeoutCount >= 5)
                {
                    CLogger.Log("OnEnterFrame() / Network Receive Timeout 5 Over");

                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                }
                else
                {
                    if (m_Socket.IsExistRetryCommand())
                    {
                        CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_RETRY_COMMAND);
                        m_IsCheckRetryCommand = true;

                        CLogger.Log("OnEnterFrame() / Retry Current Command.");
                        m_Socket.RetryCurrentCmd();
                    }
                    else
                    {
                        CLogger.Log("OnEnterFrame() / socket dummy receive.");

                        m_Socket.m_cmd = m_CurrentCmd;
                        m_Socket.m_errorcode = 8;

                        m_Socket.m_state = 3;

                        CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_DATA_READY, null, false);

                        m_Socket.ResetBuffer();

                        m_CurrentCmd = -1;
                        m_Socket.m_state = 4;
                        m_Socket.m_lastSendTick = 0;

                        CLogger.Log("OnEnterFrame() / socket dummy receive / END");
                    }
                }

                return;
            }

            // 현재 명령 수신 완료 되었는데 1초내로 m_CurrentCmd가 -1로 리셋되지 않으면 강제 리셋.
            if (m_CurrentCmd != -1 && (m_Socket.m_state == 3 || m_Socket.m_state == 4) && CMiscFunc.SafeGetTimer() - m_Socket.m_lastSockDataFetchTime > 1000)
            {
                CLogger.Log("OnEnterFrame() / reset - " + m_CurrentCmd);
                m_Socket.ResetBuffer();
                m_CurrentCmd = -1;
            }

            // 현재 유휴상태이면 큐처리된 요청 보내기.
            if (m_CurrentCmd == -1)
            {
                lock (m_Socket.thisLock)
                {
                    m_CurrentCmd = m_Socket.PreSendQueuedRequest();
                    if (m_CurrentCmd != -1)
                    {
                        m_Socket.SendQueuedRequest();
                    }
                }
            }

            if (m_CurrentCmd != -1)
            {
                return;
            }

            // 하트비트 보내기 (로그인 성공 후에만 보냄).
            // 김가람 수정 - 다른 command를 서버가 수신하면 heartbeat 업데이트 하도록 해 두었으므로 뒤의 OR 조건  생략.
            if (m_IsConnected)
            {
                if (CMiscFunc.SafeGetTimer() - m_HeartBeatTick > HEART_BEAT_INTERVAL)
                {
                    m_HeartBeatTick = CMiscFunc.SafeGetTimer();
                    SendHeartBeat();

                    return;
                }
            }
            else
            {
                m_HeartBeatTick = CMiscFunc.SafeGetTimer();
            }
        }

        public void CheckSendTick(object sender, ElapsedEventArgs args)
        {
            if (m_Socket != null)
            {
                if (m_Socket.m_lastSendCheckTick > 0 && CMiscFunc.SafeGetTimer() - m_Socket.m_lastSendCheckTick > 180 * 1000)
                {
                    this.m_DisconnectState = 1;
                }
            }
        }

        public void CheckLiveTick(object sender, ElapsedEventArgs args)
        {
            if (m_Socket != null)
            {
                m_SocketNullTick = 0;

                if (m_Socket.m_SocketLiveTick > 0 && CMiscFunc.SafeGetTimer() - m_Socket.m_SocketLiveTick > 90 * 1000)
                {
                    CLogger.Log("CheckLiveTick() / invalid live tick.");

                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                }
            }
            else
            {
                if (m_SocketNullTick == 0)
                {
                    m_SocketNullTick = CMiscFunc.SafeGetTimer();
                }

                if (CMiscFunc.SafeGetTimer() - m_SocketNullTick > 30 * 1000)
                {
                    CLogger.Log("CheckLiveTick() / invalid socket tick.");

                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                }
            }
        }

        private void OnCheckTimer(object sender, ElapsedEventArgs args)
        {
            if (m_DisconnectState >= 2) return;

            m_DisconnectState = 0;
            if (m_Socket != null)
            {
                if (m_Socket.m_lastSendTick != 0)
                {
                    if (CMiscFunc.SafeGetTimer() - m_Socket.m_lastSendTick > 30000)
                    {
                        m_DisconnectState = 1;
                    }
                }
            }
        }

        public void OnSocketConnect(object obj)
        {
            CLogger.Log("OnSocketConnect()");
            if (m_ReconnectTimer != null)
            {
                m_ReconnectTimer.Stop();
                m_ReconnectTimer = null;
            }

            m_HeartBeatErrorCnt = 0;
            m_TimeoutCount = 0;

            CLogger.Log("OnSocketConnect() / is reconnect : {0}", this.m_IsReconnect);
            CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_CONNECT, m_SERVERPORT);

            m_HeartBeatTick = CMiscFunc.SafeGetTimer();

            m_EnterFrameTimer = new Timer();
            m_EnterFrameTimer.Interval = 100; // 0.1 second
            m_EnterFrameTimer.Elapsed += new ElapsedEventHandler(OnEnterFrame);
            m_EnterFrameTimer.Start();

            CNotificationCenter.Instance.RemoveHandler(this, CGameConstants.SOCKET_CONNECT);

            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_DATA_READY, OnDataReady);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_DATA_PROCESSED, OnDataProcessed);

            CLogger.Log("OnSocketConnect() / is first login : {0}", m_IsFirstLogin);
            if (m_IsFirstLogin == false)
            {
                EnqueAction(() =>
                {
                    // 기존에 pending 중이던 request 모두 전송하도록 처리.
                    m_Socket.PushMultipleRequestToQueue(m_PendingRequestQueue);
                    m_PendingRequestQueue = new ArrayList();
                });
            }

            if (m_CheckTimer != null)
            {
                m_CheckTimer.Stop();
                m_CheckTimer = null;
            }

            m_CheckTimer = new Timer();
            m_CheckTimer.Interval = 1000;
            m_CheckTimer.Elapsed += new ElapsedEventHandler(OnCheckTimer);
            m_CheckTimer.Start();
        }

        // 서버 재접속.
        public void Reconnect()
        {
            CLogger.Log("Reconnect()");

            if (m_EnterFrameTimer != null)
            {
                m_EnterFrameTimer.Stop();
                m_EnterFrameTimer = null;
            }

            int lastsendtick = 0;
            if (m_Socket != null)
            {
                lastsendtick = m_Socket.m_lastSendTick;

                // 기존에 처리중이던 Request 재전송 용도로 저장.
                byte[] currentCommandData = null;
                m_Socket.GetCurrentCmd(out currentCommandData);

                if (currentCommandData != null)
                {
                    m_PendingRequestQueue.Add(currentCommandData);
                }

                if (m_Socket != null)
                {
                    ArrayList requestQueue = m_Socket.GetRequestQueue();
                    for (int i = 0; i < requestQueue.Count; i++)
                    {
                        m_PendingRequestQueue.Add(requestQueue[i]);
                    }
                }

                try
                {
                    m_Socket.Close();
                }
                catch (Exception e)
                {
                    CLogger.Log("Reconnect() / Exception Occur / message : " + e.Message);
                }

                m_Socket = null;
            }

            m_CurrentCmd = -1;
            m_IsConnected = false;

            m_Socket = new CSocketBase();
            m_Socket.Connect(m_SERVERIP, m_SERVERPORT);
            m_Socket.m_lastSendTick = lastsendtick;

            if (m_DisconnectState == 0)
            {
                m_Socket.m_lastSendTick = CMiscFunc.SafeGetTimer() - 15000;
            }
        }

        protected void OnSocketError(object obj)
        {
            CLogger.Log("OnSocketError() / socket state : {0}", m_Socket.m_state);

            if (m_EnterFrameTimer != null)
            {
                m_EnterFrameTimer.Stop();
                m_EnterFrameTimer = null;
            }

            string host = m_Socket.m_host;
            string port = m_Socket.m_port.ToString();

            // 다음 포트로 시도.
            for (int i = 0; i < m_PORTLIST.Length; i++)
            {
                if (m_SERVERPORT == m_PORTLIST[i])
                {
                    if (i == (m_PORTLIST.Length - 1))
                    {
                        m_SERVERPORT = m_PORTLIST[0];
                    }
                    else
                    {
                        m_SERVERPORT = m_PORTLIST[i + 1];
                    }

                    break;
                }
            }

            CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_DISCONNECT);
            m_IsConnected = false;
            EnqueAction(() => CWebLogManager.Instance.SendCommunicationLog(m_CurrentCmd, string.Format("socket error ({0}:{1})", host, port), 0));

            m_SocketCloseCount++;
            if (m_bCanReconnect)
            {
                if (m_ReconnectTimer != null)
                {
                    m_ReconnectTimer.Stop();
                    m_ReconnectTimer = null;
                }

                // 10회 이상 socket 연결 끊길 경우 Refresh Layer 보여줌.
                if (m_SocketCloseCount > 10)
                {
                    CLogger.Log("OnSocketError() / socket close 10 over");

                    EnqueAction(Close);
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                    return;
                }

                CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_CONNECT, OnSocketConnect);

                m_ReconnectTimer = new Timer();
                m_ReconnectTimer.Interval = 3000;
                m_ReconnectTimer.Elapsed += new ElapsedEventHandler(TryReconnect);
                m_ReconnectTimer.Start();

                m_IsReconnect = true;
            }
        }

        public void OnSocketClose(object obj)
        {
            if (m_EnterFrameTimer != null)
            {
                m_EnterFrameTimer.Stop();
                m_EnterFrameTimer = null;
            }

            CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_DISCONNECT);

            m_IsConnected = false;

            if (m_Socket != null)
            {
                if (m_CurrentCmd == 1)
                {
                    EnqueAction(() => CWebLogManager.Instance.SendCommunicationLog(m_CurrentCmd, string.Format("socket closed ({0}:{1}:{2}:{3})", m_Socket.m_host, m_Socket.m_port, m_UserId, m_UserKey), 0));
                }
                else
                {
                    EnqueAction(() => CWebLogManager.Instance.SendCommunicationLog(m_CurrentCmd, string.Format("socket closed ({0}:{1})", m_Socket.m_host, m_Socket.m_port), 0));
                }
            }

            CLogger.Log("OnSocketClose() / close count : {0} / can reconnect : {1}", m_SocketCloseCount, m_bCanReconnect);
            m_SocketCloseCount++;
            if (m_bCanReconnect)
            {
                if (m_ReconnectTimer != null)
                {
                    m_ReconnectTimer.Stop();
                    m_ReconnectTimer = null;
                }

                // 10회 이상 socket 연결 끊길 경우 Refresh Layer 보여줌.
                if (m_SocketCloseCount > 10)
                {
                    CLogger.Log("OnSocketClose() / socket close 10 over");

                    EnqueAction(Close);
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                    return;
                }

                CNotificationCenter.Instance.AddHandler(this, CGameConstants.SOCKET_CONNECT, OnSocketConnect);


                m_ReconnectTimer = new Timer();
                m_ReconnectTimer.Interval = 3000;
                m_ReconnectTimer.Elapsed += new ElapsedEventHandler(TryReconnect);

                m_IsReconnect = true;

                if (m_Socket != null)
                {
                    if (m_DisconnectState == 0)
                    {
                        m_Socket.m_lastSendTick = CMiscFunc.SafeGetTimer() - 15000;
                    }
                }
            }
        }

        protected void TryReconnect(object sender, ElapsedEventArgs args)
        {
            CLogger.Log("TryReconnect");

            if (m_ReconnectTimer != null)
            {
                m_ReconnectTimer.Stop();
                m_ReconnectTimer = null;
            }

            EnqueAction(Reconnect);
        }

        public bool CheckQueuedRequest(int cmd)
        {
            for (int i = m_PendingRequestQueue.Count - 1; i >= 0; i--)
            {
                byte[] bytes = CEncryption.decodeBytes((byte[])m_PendingRequestQueue[i]);
                int child_cmd = CSocketBase.ReadShortInt(bytes, 8);

                if (child_cmd == cmd)
                {
                    return true;
                }
            }

            if (m_Socket != null)
            {
                return m_Socket.CheckQueuedRequest(cmd);
            }

            return false;
        }

        public void CancelQueuedRequest(int cmd)
        {
            for (int i = m_PendingRequestQueue.Count - 1; i >= 0; i--)
            {
                byte[] bytes = CEncryption.decodeBytes((byte[])m_PendingRequestQueue[i]);
                int child_cmd = CSocketBase.ReadShortInt(bytes, 8);

                if (child_cmd == cmd)
                {
                    m_PendingRequestQueue.RemoveAt(i);
                }
            }

            if (m_Socket != null)
            {
                m_Socket.CancelQueuedRequest(cmd);
            }
        }

        protected void OnDataProcessed(object obj)
        {
            // CLogger.Log("OnDataProcessed");
            m_CurrentCmd = -1;
            m_Socket.m_state = 4;
        }

        protected void OnDataReady(object obj)
        {
            // 어떤 패킷이라도 통신했으면 HeartBeat Skip.
            m_HeartBeatTick = CMiscFunc.SafeGetTimer();

            if (m_Socket != null && m_Socket.m_cmd != CNetworkConstants.CMD_CHECK_LOGIN)
            {
                EnqueAction(() =>
                {
                    // #TODO 의존성으로 인해 다른 방식으로 처리 요망
                    // CApplicationManager.Instance.SaveFacebookInfoLastTime();
                });
            }

            if (m_IsCheckRetryCommand)
            {
                m_IsCheckRetryCommand = false;
                CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_RETRY_COMMAND_COMPLETE);
            }

            if (m_Socket == null)
            {
                return;
            }

            // 메인터넌스 처리.
            if (m_Socket.m_errorcode >= 50)
            {
                m_Socket.m_errorcode -= 50;

                CLogger.LogWarning("OnDataReady() / maintenance.");

                if (m_IsMaintenanceNotified == false && CMiscFunc.SafeGetTimer() > 60000)
                {
                    m_IsMaintenanceNotified = true;
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_MAINTENANCE_NOTICE);
                }
            }

            // if (m_Socket.m_errorcode != 0 && m_Socket.m_cmd == CProtocolConstants.CMD_BUY_PRODUCT_ITEM)
            // {
            // 	EnqueAction(()=>UnblockLogin(), 0.5f);
            // }

            CLogger.Log("OnDataReady() / cmd : {0} / error : {1}", m_Socket.m_cmd, m_Socket.m_errorcode);

            if (m_NetWorkProtocols.ContainsKey(m_Socket.m_cmd))
            {
                IProtocol proto = m_NetWorkProtocols[m_Socket.m_cmd];
                proto.Receive(m_Socket);
            }
            else
            {
                switch (m_Socket.m_cmd)
                {
                    case CNetworkConstants.CMD_LOGIN:
                        if (m_Socket.m_errorcode == 0)
                        {
                            OnLoginReceive();
                        }
                        else
                        {
                            OnLoginFailed();
                        }
                        break;
                    case CNetworkConstants.CMD_RELOGIN:
                        if (m_Socket.m_errorcode == 0)
                        {
                            OnReloginReceive();
                        }
                        else
                        {
                            OnReloginFailed();
                        }
                        break;

                    case CNetworkConstants.CMD_HEART_BEAT:
                        if (m_Socket.m_errorcode == 0)
                        {
                            m_HeartBeatErrorCnt = 0;
                        }
                        else if (m_Socket.m_errorcode == 1)
                        {
                            m_HeartBeatErrorCnt++;
                        }
                        break;
                }
            }

            // 스크립트를 통해 통신 로그 기록
            if (m_Socket.m_latency > 1000 || m_Socket.m_errorcode != 0 && m_Socket.m_errorcode != 44 && (m_Socket.m_errorcode < 10 || m_Socket.m_errorcode > 29))
            {
                int _cmd = m_Socket.m_cmd;
                int _errcode = m_Socket.m_errorcode;
                int _latency = m_Socket.m_latency;

                EnqueAction(() =>
                {
                    CWebLogManager.Instance.SendCommunicationLog(_cmd, _errcode.ToString(), _latency);
                });
            }

            if (m_Socket.m_errorcode != 0 && (m_Socket.m_errorcode < 10 || m_Socket.m_errorcode > 29) && m_Socket.m_cmd != 98)
            {
                m_ErrorCnt++;		// Quick Start는 Error로 치지 않음
                if (m_Socket.m_errorcode == -1)
                {
                    m_ErrorCnt += 20;	// 헤더 파싱 오류는 바로 refresh
                    CLogger.Log("OnDataReady() / header parsing error.");
                }
            }
            else
            {
                m_ErrorCnt = 0;
            }

            // HeartBeat 3회 오류 혹은 사용자가 다른 곳에서 로그인하여 online 정보 사라졌을 경우 (errorcode = 44)
            if (m_HeartBeatErrorCnt >= 3 || m_Socket.m_errorcode == 44 || m_Socket.m_errorcode == 45)
            {
                //				CLogger.Log("OnDataReady() / HeartBeat Error 3 Over or User Login from other location. / m_HeartBeatErrorCnt : " + m_HeartBeatErrorCnt + " / error code : " + m_Socket.m_errorcode);
                if (m_EnterFrameTimer != null)
                {
                    m_EnterFrameTimer.Stop();
                    m_EnterFrameTimer = null;
                }

                if (m_Socket.m_errorcode != 44)
                {
                    m_IsReconnect = true;
                }
                m_IsConnected = false;
                m_Socket.Close();

                if (m_HeartBeatErrorCnt >= 3)
                {
                    CLogger.Log("OnDataReady() / heart beat error 3 over.");
                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                }
                else if (m_Socket.m_errorcode == 45)	// Messive Error
                {
                    CLogger.Log("OnDataReady() / error code 45.");

                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                }
                else if (m_Socket.m_errorcode == 44)
                {
                    CLogger.Log("OnDataReady() / error code 44.");

                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_DUPLICATED_LOGIN);
                }
                else
                {
                    CLogger.Log("OnDataReady() / connect error.");

                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                }

                return;
            }

            if (m_ErrorCnt >= 20 && m_Socket.Connected)
            {
                if (m_EnterFrameTimer != null)
                {
                    m_EnterFrameTimer.Stop();
                    m_EnterFrameTimer = null;
                }

                m_IsReconnect = true;
                m_IsConnected = false;

                CLogger.Log("OnDataReady() / error count 20 over.");
                Close();

                CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
            }
            else
            {
                // 5초 이내로 패킷 수신되면 receive timeout 횟수 초기화
                if ((CMiscFunc.SafeGetTimer() - m_Socket.m_lastReceiveCheckTick) < 5000)
                {
                    if (m_TimeoutCount > 0 || m_SocketCloseCount > 0)
                    {
                        CLogger.Log("OnDataReady() / error count init.");
                    }

                    m_TimeoutCount = 0;
                    m_SocketCloseCount = 0;
                }

                CLogger.Log("OnDataReady() / Notify CGameConstants.UI_EVENT_SOCKET_DATA_READY");
                CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_DATA_READY, new CNotification(m_Socket.m_errorcode.ToString(), m_Socket));
            }
        }

        public void OnLoginFailed()
        {
            // Abuser 처리.
            if (m_Socket.m_errorcode == 30)
            {
                CLogger.Log("OnDataReady() / user is block.");
                m_IsAbused.Number = true;
                m_AbuseTermHour.Number = m_Socket.ReadTinyInt();

                Close();

                CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_BLOCK_USER);
                return;
            }

            CLogger.Log("OnDataReady() / login error : {0}", m_Socket.m_errorcode);
            if (m_Socket.m_errorcode != 44)
            {
                if (m_LoginTryCount < 3)
                {
                    m_LoginTryCount++;
                    EnqueAction(() => Login());
                }
                else
                {
                    CLogger.Log("OnDataReady() / login fail 3 over.");

                    Close();
                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                    return;
                }
            }
        }

        public void OnReloginFailed()
        {
            CLogger.Log("OnDataReady() / relogin error : {0}", m_Socket.m_errorcode);
            if (m_Socket.m_errorcode != 44)
            {
                if (m_LoginTryCount < 3)
                {
                    m_LoginTryCount++;
                    //EnqueAction(()=>Relogin());
                }
                else
                {
                    CLogger.Log("OnDataReady() / Relogin fail 3 over.");
                    Close();

                    CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_SOCKET_RECONNECT_FAILED);
                    return;
                }
            }
        }

        public bool CheckLogin()
        {
            if (m_Socket == null)
            {
                return false;
            }

            CPacketData _packetData = new CPacketData(CNetworkConstants.CMD_CHECK_LOGIN, m_Useridx.Number);

            _packetData.WriteString(m_UserId);
            // 0 : facebook login type
            // 1 : guest login type
            _packetData.WriteTinyInt(0);

            m_Socket.PushRequestToQueue(_packetData);
            return true;
        }

        private void OnReloginReceive()
        {
            //			m_useridx.Number = m_Socket.ReadBigInt(11);
            //			
            //			CUserInfo.Instance.Useridx.Number = m_useridx.Number;
            //			CLogger.Log("OnReloginReceive() / UserIdx : {0}", m_useridx.Number);
            //			
            //			m_Socket.m_Key.Number = m_Socket.ReadInt();
            //			
            //			int updateType = m_Socket.ReadTinyInt(); // nothing : 0 / optional : 1 / force : 2
            //			
            //			long _coin = m_Socket.ReadBigInt();
            //			long _credit = m_Socket.ReadBigInt();
            //			long _key = m_Socket.ReadBigInt();
            //			long _giftPoint = m_Socket.ReadBigInt();
            //			
            //			//일단 사용 안하도록 수정.
            //			//			CUserInfo.Instance.Coin.Number = _coin;
            //			//			CUserInfo.Instance.NetworkCoin.Number = _coin;
            //			//			CUserInfo.Instance.Credit.Number = _credit;
            //			//			CUserInfo.Instance.NetworkCredit.Number = _credit;
            //			//			CUserInfo.Instance.Key.Number = _key;
            //			//			CUserInfo.Instance.NetworkKey.Number = _key;
            //			//			CUserInfo.Instance.GiftPoint.Number = _giftPoint;
            //			
            //			CLogger.Log("OnReloginReceive() / Coin : " + CUserInfo.Instance.Coin.Number + " / Credit : " + CUserInfo.Instance.Credit.Number + " / Key : " + CUserInfo.Instance.Key.Number + " / GiftPoint : " + CUserInfo.Instance.GiftPoint.Number);
            //			
            //			CUserInfo.Instance.IsSkipAddLevel.Number = false;
            //			CUserInfo.Instance.Level.Number = m_Socket.ReadInt();
            //			CUserInfo.Instance.LevelExp.Number = m_Socket.ReadInt();
            //			CUserInfo.Instance.LevelMaxExp.Number = m_Socket.ReadInt();
            //			
            //			if (CUserInfo.Instance.Level.Number > CLevelInfo.MaxLevel)
            //			{
            //				// max level => add level skip.
            //				CUserInfo.Instance.IsSkipAddLevel.Number  = true;
            //			}
            //			
            //			if (CUserInfo.Instance.Level.Number > CLevelInfo.MaxLevel)
            //			{
            //				CUserInfo.Instance.Level.Number = CLevelInfo.MaxLevel;
            //				CUserInfo.Instance.LevelExp.Number = CLevelInfo.LevelMaxExp[CLevelInfo.MaxLevel].Number;
            //				CUserInfo.Instance.LevelMaxExp.Number = CLevelInfo.LevelMaxExp[CLevelInfo.MaxLevel].Number;
            //			}
            //			
            //			long currentClientIdx = m_Socket.ReadBigInt();
            //			long oldClientIdx = m_Socket.ReadBigInt();
            //			
            //			CLogger.Log("OnReloginReceive() / my client Idx : {0} : / client idx : {1} / old client idx : {2}", CUserInfo.Instance.ClientIdx.Number, currentClientIdx, oldClientIdx);
            //			if (CUserInfo.Instance.ClientIdx.Number != currentClientIdx || currentClientIdx != oldClientIdx)
            //			{
            //				Close();
            //				CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_DUPLICATED_LOGIN);
            //				return;
            //			}
            //			
            //			// storage key.
            //			CUserInfo.Instance.SlotStorageKey.Number = m_Socket.ReadBigInt() - 1;
            //			long timeStamp = m_Socket.ReadBigInt();
            //			
            //			if (timeStamp > 0) 
            //			{
            //				CUserInfo.Instance.Timestamp.Number = timeStamp * 1000;		// ms 단위로 변경.
            //			}
            //			
            //			// 로그인 하는 도중에 쌓인 패킷 넣어주기.
            //			m_Socket.PushMultipleRequestToQueue(m_PendingRequestQueue);
            //			m_PendingRequestQueue = new ArrayList();
            //			
            //			m_IsReconnect = false;
            //			
            //			m_bCanReconnect = true;				
            //			m_IsConnected = true;
            //			m_LoginTryCount = 0;
            //			
            //			CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_RELOGIN_COMPLETED);
        }

        public bool UpdateDeviceInfo()
        {
            //			CLogger.Log("UpdateDeviceInfo()");
            //			if (m_Socket == null) 
            //			{
            //				return false;
            //			}
            //			
            //			CPacketData _packetData = new CPacketData(CProtocolConstants.CMD_UPDATE_DEVICE_INFO, m_useridx.Number);
            //			
            //			_packetData.WriteString(CUserInfo.Instance.DeviceUUID);
            //			_packetData.WriteString(CPlatformManager.Instance.GetAppVersion());
            //			_packetData.WriteTinyInt(CPlatformManager.Instance.IsPushEnable() == true ? 1 : 0);
            //			_packetData.WriteTinyInt(CUserInfo.Instance.OsType.Number);
            //			_packetData.WriteString(CPlatformManager.Instance.GetDeviceName());
            //			_packetData.WriteString(CPlatformManager.Instance.GetOsVersion());
            //			
            //			if (m_IsReconnect || m_IsConnected == false)
            //			{
            //				m_PendingRequestQueue.Add(CEncryption.encodeBytes(_packetData.SendData));
            //				return false;
            //			}
            //			
            //			m_Socket.PushRequestToQueue(_packetData);
            return true;
        }

        public bool UpdateLocalStorageKey(long useridx, long timestamp)
        {
            if (m_Socket == null)
            {
                return false;
            }

            if (CheckQueuedRequest(97))
            {
                CancelQueuedRequest(97);
            }

            CPacketData packetData = new CPacketData(97, 0);
            packetData.WriteBigInt(useridx);
            packetData.WriteBigInt(timestamp);

            if (m_IsReconnect)
            {
                m_PendingRequestQueue.Add(CEncryption.encodeBytes(m_Socket.GetQueueRequestBytes(97)));
                return false;
            }

            m_Socket.PushRequestToQueue(packetData);

            return true;
        }

        public bool BlockLogin()
        {
            if (m_Socket == null) return false;

            int cmd = CNetworkConstants.CMD_BLOCK_LOGIN;

            CPacketData _packetData = new CPacketData(cmd, m_Useridx.Number);
            //			_packetData.WriteString(CUserInfo.Instance.DeviceId);

            return PushRequestToQueue(_packetData);
        }

        public bool UnblockLogin()
        {
            if (m_Socket == null) return false;

            int cmd = CNetworkConstants.CMD_UNBLOCK_LOGIN;

            CPacketData _packetData = new CPacketData(cmd, m_Useridx.Number);

            return PushRequestToQueue(_packetData);
        }

        //
        // PushRequestToQueue
        //
        public bool PushRequestToQueue(CPacketData _packetData)
        {
            if (m_IsReconnect || m_IsConnected == false)
            {
                m_PendingRequestQueue.Add(CEncryption.encodeBytes(_packetData.SendData));
                return false;
            }

            m_Socket.PushRequestToQueue(_packetData);

            return true;
        }
    }
}
