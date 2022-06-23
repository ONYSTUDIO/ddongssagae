#pragma warning disable 0414

using System;
using System.Collections;
using System.Timers;
using UnityEngine;
using static DoubleuGames.GameRGD.CPreferences;

namespace DoubleuGames.GameRGD
{
    public class CChatNetwork : CActionMonoBehaviour
    {
        public const int TYPE_CITY = 0;
        public const int TYPE_GAME = 1;

        private const int HEART_BEAT_INTERVAL = 2500;
        public int m_HeartBeatInterval = HEART_BEAT_INTERVAL;

        public long m_RoomIdx = 0;
        public long m_ChatIdx = 0;
        public int m_Type = TYPE_GAME;

        public int m_CurrentCmd = -1;	// 현재 진행 중인 명령.

        private int m_HeartBeatTick;

        private int m_ErrorCnt = 0;
        private int m_HeartBeatErrorCnt = 0;
        private int m_TimeoutCount = 0;
        public bool m_IsConnected = false;
        
        //재접속  플래그
        public bool m_bCanReconnect = true;
        public bool m_IsReconnect = false;
        private Timer m_ReconnectTimer = null;
        
        // 소켓
        public CChatSocket m_Socket = null;
        
        public int m_SocketNullTick = 0;
        // 이벤트 리스너c
//		private var m_EventHandlerList:CEventHandlerList;
        
        // 재접속 관련
        private int m_ReconnectCount = 0;
        
        private string m_SERVERIP = "";
        private int m_SERVERPORT = 0;
        private int[] m_PORTLIST;
//		private int[] m_PORTLIST = {9330, 80, 22001, 7338};
        private ArrayList m_PendingRequestQueue = new ArrayList();
        
        private int m_LoginTryCount = 0;
        private bool m_bCoinUpdateForce = false;
        
        private Timer m_EnterFrameTimer;

        private bool isApplicationPause = false;
        private int pauseTime;

        protected static CChatNetwork instance = null;
        public static CChatNetwork Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = GameObject.FindObjectOfType<CChatNetwork>();
                    
                    DontDestroyOnLoad(instance);
                }
                
                return instance;
            }
        }

        protected override void OnAwakeAction ()
        {
            base.OnAwakeAction ();

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
                m_SERVERIP = "work.doubleubingo.com";
                m_PORTLIST = new int[]{8080, };
            }
            else
            {
                m_SERVERIP = "m.doubleubingo.com";
                m_PORTLIST = new int[]{9334, };
            }
            
            m_SERVERPORT = m_PORTLIST[0];

            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_PAUSE, OnApplicationPause);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_RESUME, OnApplicationResume);
        }
        
        private void OnApplicationPause(object param)
        {
            isApplicationPause = true;
            pauseTime = 0;
        }
        
        private void OnApplicationResume(object param)
        {
            isApplicationPause = false;
            pauseTime = 0;
        }

        protected override void OnDestroyAction ()
        {
            base.OnDestroyAction ();

            Close();
        }

        public void Init(long roomidx, int type = TYPE_GAME)
        {
            CLogger.Log("Init()");

            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_CONNECT, OnSocketConnect);
            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_CLOSE, OnSocketClose);
            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_IO_ERROR, OnSocketError);

            m_Type = type;
            m_RoomIdx = roomidx;

            if (m_PendingRequestQueue != null)
            {
                m_PendingRequestQueue.Clear();
                m_PendingRequestQueue = null;
            }
            m_PendingRequestQueue = new ArrayList();

            m_CurrentCmd = -1;
            m_ErrorCnt = 0;
            m_TimeoutCount = 0;
            m_IsConnected = false;
            m_bCanReconnect = true;
            m_IsReconnect = false;
            m_ReconnectCount = 0;
            
            UpdateIntervalTime();
            
            if (m_Socket == null) 
            {
                m_Socket = new CChatSocket();
                m_Socket.Connect (m_SERVERIP, m_SERVERPORT);
            } 
            else 
            {
                if (m_Socket.Connected)
                {
                    m_IsConnected = true;

                    if (m_EnterFrameTimer != null)
                    {
                        m_EnterFrameTimer.Stop();
                        m_EnterFrameTimer = null;
                    }

                    m_EnterFrameTimer = new Timer ();
                    m_EnterFrameTimer.Interval = 100; // 0.1 second
                    m_EnterFrameTimer.Elapsed += new ElapsedEventHandler (OnEnterFrame);
                    m_EnterFrameTimer.Start ();
                    
                    if (type == TYPE_GAME) 
                    {
                        GetGameChatIndex();
                    }
                } 
                else 
                {
                    if (m_EnterFrameTimer != null)
                    {
                        m_EnterFrameTimer.Stop();
                        m_EnterFrameTimer = null;
                    }

                    m_Socket.Close();
                    m_Socket = null;

                    m_Socket = new CChatSocket();
                    m_Socket.Connect (m_SERVERIP, m_SERVERPORT);
                }
            }
        }

        public void UpdateIntervalTime(bool isShow = true)
        {
            if (CPreferences.IsApplicationPause)
            {
                m_HeartBeatInterval = 30 * 1000;
            }
            else
            {
//				if (CUserInfo.Instance.IsSinglePlay == true)
//				{
//					m_HeartBeatInterval = 6 * 1000;
//				}
//				else
//				{
//					if (isShow)
//					{
//						if (m_Type == TYPE_CITY) 
//						{
//							m_HeartBeatInterval = 2 * 1000;
//						} 
//						else 
//						{
//							m_HeartBeatInterval = HEART_BEAT_INTERVAL;
//						}
//					}
//					else
//					{
//						m_HeartBeatInterval = 6 * 1000;
//					}
//				}
            }
        }

        public void OnExit()
        {
            CLogger.Log("OnExit()");
            Close();
        }

        public void Close()
        {
            CLogger.Log("Close()");
            CNotificationCenter.Instance.RemoveHandler(this);

            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_PAUSE, OnApplicationPause);
            CNotificationCenter.Instance.AddHandler(this, CGameConstants.UI_EVENT_APPLICATION_RESUME, OnApplicationResume);

            if (m_PendingRequestQueue != null)
            {
                m_PendingRequestQueue.Clear();
                m_PendingRequestQueue = null;
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
            m_CurrentCmd = -1;
            m_IsReconnect = true;
            m_IsConnected = false;
            m_bCanReconnect = false;
            m_TimeoutCount = 0;
        }

        public void ResumeNetwork()
        {
            if (m_IsConnected || m_IsReconnect)
            {
                CLogger.Log("ResumeConnect() / already or connecting...");
                return;
            }
            
            m_bCanReconnect = true;
            
            CLogger.Log("ResumeConnect()");
            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_CONNECT, OnSocketConnect);
            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_CLOSE, OnSocketClose);
            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_IO_ERROR, OnSocketError);
            
            m_Socket = new CChatSocket ();
            m_Socket.Connect (m_SERVERIP, m_SERVERPORT);
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
                    ArrayList requestQueue = m_Socket.GetRequestQueue();
                    for (int i=0; i<requestQueue.Count; i++)
                    {
                        m_PendingRequestQueue.Add(requestQueue[i]);
                    }

                    byte[] currentCommandData = null;
                    m_Socket.GetCurrentCmd(out currentCommandData);
                    
                    if (currentCommandData != null)
                    {
                        m_PendingRequestQueue.Add(currentCommandData);
                    }

                    m_Socket.Close();
                }
            }
            catch (Exception e) 
            {
                CLogger.Log("PauseConnect() / Exception : " + e.Message);
            }

            m_CurrentCmd = -1;
            
            m_Socket = null;
            m_IsConnected = false;
            m_IsReconnect = false;
            m_bCanReconnect = false;
        }

        public void PauseNResumeNetwork()
        {
            PauseConnect();

            EnqueAction(ResumeNetwork, 1);
        }

        // 주기적으로 이루어지는 작업 처리
        private void OnEnterFrame(object sender, ElapsedEventArgs args)
        {
            if (isApplicationPause == true)
            {
                pauseTime += 100;
                
                if (pauseTime > CGameConstants.CHECK_PAUSE_TIME)
                {
                    CLogger.LogWarning("OnEnterFrame() / long pause. so, network close.");
                    Close();
                    return;
                }
            }

//			if (CUserInfo.Instance.IsAbused.Number == true)
//			{
//				Close();
//				return;
//			}
            
            // 30초 내로 응답 오지 않으면 실패로 처리 (socket에 일부 패킷이 도착했을 때는 해당 시간 기준으로 체크).
            if (m_CurrentCmd != -1 && m_Socket.m_state == 2 && CMiscFunc.SafeGetTimer() - m_Socket.m_lastReceiveCheckTick > 30000)
            {
                CLogger.Log("OnEnterFrame() / Network Receive Timeout");
                
                m_TimeoutCount++;
                if (m_TimeoutCount >= 5)
                {
                    CLogger.Log("OnEnterFrame() / Network Receive Timeout 5 Over");
                    PauseNResumeNetwork();
                }
                else
                {
                    if (m_Socket.IsExistRetryCommand())
                    {
                        CLogger.Log("OnEnterFrame() / Retry Current Command.");
                        m_Socket.RetryCurrentCmd();
                    }
                    else
                    {
                        m_Socket.m_cmd = m_CurrentCmd;
                        m_Socket.m_errorcode = 6;
                        
                        m_Socket.m_state = 3;
                        
                        CNotificationCenter.Instance.PostNotification(CGameConstants.CHAT_SOCKET_DATA_READY);
                        
                        m_Socket.ResetBuffer();

                        m_CurrentCmd = -1;
                        m_Socket.m_state = 4;
                        m_Socket.m_lastSendTick = 0;
                    }
                }
                
                return;
            }
            
            // 현재 명령 수신 완료 되었는데 1초내로 m_CurrentCmd가 -1로 리셋되지 않으면 강제 리셋
            if (m_CurrentCmd != -1 && (m_Socket.m_state == 3 || m_Socket.m_state == 4) && CMiscFunc.SafeGetTimer() - m_Socket.m_lastSockDataFetchTime > 1000)
            {
                CLogger.Log("OnEnterFrame() / reset - " + m_CurrentCmd);

                m_Socket.ResetBuffer();
                m_CurrentCmd = -1;
            }
            
            // 현재 유휴상태이면 큐처리된 요청 보내기
            if (m_CurrentCmd == -1)
            {
                lock(m_Socket.thisLock)
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
                if (CMiscFunc.SafeGetTimer () - m_HeartBeatTick > m_HeartBeatInterval) {
                    m_HeartBeatTick = CMiscFunc.SafeGetTimer ();
                    GameChatInfo ();
                    return;
                }
            }
            else
            {
                m_HeartBeatTick = CMiscFunc.SafeGetTimer();
            }
        }
        
        public bool CheckQueuedRequest(int cmd)
        {
            for (int i=m_PendingRequestQueue.Count-1; i>=0; i--)
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
            for (int i=m_PendingRequestQueue.Count-1; i>=0; i--)
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
            
            m_HeartBeatTick = CMiscFunc.SafeGetTimer();
            
            m_EnterFrameTimer = new Timer ();
            m_EnterFrameTimer.Interval = 100; // 0.1 second
            m_EnterFrameTimer.Elapsed += new ElapsedEventHandler (OnEnterFrame);
            m_EnterFrameTimer.Start ();
            
            CNotificationCenter.Instance.RemoveHandler (this, CGameConstants.CHAT_SOCKET_CONNECT);

            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_DATA_READY, OnDataReady);
            CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_DATA_PROCESSED, OnDataProcessed);

            GameChatLogin();
//			CLogger.Log("OnSocketConnect() / request length : {0}", m_PendingRequestQueue.Count);

            // 기존에 pending 중이던 request 모두 전송하도록 처리.
            m_Socket.PushMultipleRequestToQueue(m_PendingRequestQueue);
            m_PendingRequestQueue = new ArrayList();
        }
        
        // 서버 재접속
        public void Reconnct()
        {
            CLogger.Log("Reconnct()");
            m_IsReconnect = true;
            
            int lastsendtick = 0;
            
            if (m_Socket != null)
            {
                lastsendtick = m_Socket.m_lastSendTick;
                
                // 기존에 처리중이던 Request 재전송 용도로 저장.
                ArrayList requestQueue = m_Socket.GetRequestQueue();
                for (int i=0; i<requestQueue.Count; i++)
                {
                    m_PendingRequestQueue.Add(requestQueue[i]);
                }

                byte[] currentCommandData = null;
                m_Socket.GetCurrentCmd(out currentCommandData);
                
                if (currentCommandData != null)
                {
                    m_PendingRequestQueue.Add(currentCommandData);
                }
                
                try
                {
                    m_Socket.Close();
                }
                catch (Exception e) 
                {
                    CLogger.Log("OnSocketReConnect() / Exception Occur / message : " + e.Message);
                }
                
                m_Socket = null;
            }
            
            m_CurrentCmd = -1; 
            m_IsConnected = false;

            m_Socket = new CChatSocket ();
            m_Socket.Connect (m_SERVERIP, m_SERVERPORT);
            m_Socket.m_lastSendTick = lastsendtick;
            
            if (m_EnterFrameTimer != null)
            {
                m_EnterFrameTimer.Stop();
                m_EnterFrameTimer = null;
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

            // 다음 포트로 시도.
            for (int i = 0; i<m_PORTLIST.Length; i++)
            {
                if (m_SERVERPORT == m_PORTLIST[i])
                {
                    if (i == (m_PORTLIST.Length - 1))
                    {
                        m_SERVERPORT = m_PORTLIST[0];
                    }
                    else
                    {
                        m_SERVERPORT = m_PORTLIST[i+1];
                    }
                }
            }

            m_IsConnected = false;
            if (m_bCanReconnect)
            {
                if (m_ReconnectTimer != null)
                {
                    m_ReconnectTimer.Stop();
                    m_ReconnectTimer = null;
                }

                CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_CONNECT, OnSocketConnect);

                m_ReconnectCount = 0;
                m_ReconnectTimer = new Timer();
                m_ReconnectTimer.Interval = 2000;
                m_ReconnectTimer.Elapsed += new ElapsedEventHandler(TryReconnect);
                m_ReconnectTimer.Start();		
            }
        }
        
        public void OnSocketClose(object obj)
        {
            if (m_EnterFrameTimer != null)
            {
                m_EnterFrameTimer.Stop();
                m_EnterFrameTimer = null;
            }

            m_IsConnected = false;

            CLogger.Log("OnSocketClose() / can reconnect : {0}", m_bCanReconnect);
            if (m_bCanReconnect)
            {
                if (m_ReconnectTimer != null)
                {
                    m_ReconnectTimer.Stop();
                    m_ReconnectTimer = null;
                }

                CNotificationCenter.Instance.AddHandler (this, CGameConstants.CHAT_SOCKET_CONNECT, OnSocketConnect);

                m_ReconnectCount = 0;
                m_ReconnectTimer = new Timer();
                m_ReconnectTimer.Interval = 2000;
                m_ReconnectTimer.Elapsed += new ElapsedEventHandler(TryReconnect);
                m_IsReconnect = true;
            }
        }
        
        protected void TryReconnect(object sender, ElapsedEventArgs args)
        {
            if (m_ReconnectTimer != null)
            {
                m_ReconnectTimer.Stop();
                m_ReconnectTimer = null;
            } 

            EnqueAction(Reconnct);
        }
        
        protected void OnDataProcessed(object obj)
        {
            m_CurrentCmd = -1;
            m_Socket.m_state = 4;
        }
        
        protected void OnDataReady(object obj)
        {
//			CLogger.Log("OnDataReady() / cmd : {0}", m_Socket.m_cmd);
            m_HeartBeatTick = CMiscFunc.SafeGetTimer();

            if (m_Socket == null)
            {
                return;
            }

            switch(m_Socket.m_cmd)
            {
            case CNetworkConstants.CMD_CHAT_LOGIN:
                if (m_Socket.m_errorcode == 0)
                {
                    m_Socket.m_Key.Number = m_Socket.ReadInt();
                    m_ChatIdx = m_Socket.ReadBigInt();
                    
                    // 로그인 하는 도중에 쌓인 패킷 넣어주기
                    m_Socket.PushMultipleRequestToQueue(m_PendingRequestQueue);
                    m_PendingRequestQueue = new ArrayList();
                    
                    m_IsReconnect = false;
                    
                    m_bCanReconnect = true;				
                    m_IsConnected = true;
                    m_LoginTryCount = 0;

                    CLogger.Log("OnDataReady() / game chat login success");
                }	
                else
                {
                    if (m_LoginTryCount < 3)
                    {
                        m_LoginTryCount++;
                        this.GameChatLogin();
                    }
                    else
                    {
                        Close();
                        return;
                    }
                }
                break;
                
            case CNetworkConstants.CMD_CHAT_GET_INDEX:
                if (m_Socket.m_errorcode == 0) 
                {
                    m_ChatIdx = m_Socket.ReadBigInt();
                    CLogger.Log("OnDataReady() / chat index : {0}", m_ChatIdx);
                }
                break;
            }

            // 스크립트를 통해 통신 로그 기록
            if (m_Socket.m_latency > 1000 || m_Socket.m_errorcode != 0 && (m_Socket.m_errorcode < 10 || m_Socket.m_errorcode > 29)) 
            {
                int _cmd = m_Socket.m_cmd;
                int _errorCode = m_Socket.m_errorcode;
                int _latency = m_Socket.m_latency;

                EnqueAction(()=>CWebLogManager.Instance.SendCommunicationLog(_cmd, _errorCode.ToString(), _latency));
            }
            
            if (m_Socket.m_errorcode != 0 && (m_Socket.m_errorcode < 10 || m_Socket.m_errorcode > 29) && m_Socket.m_cmd != 98) 
            {
                CLogger.Log("OnDataReady() / error - " + m_Socket.m_cmd + "," + m_Socket.m_errorcode);

                m_ErrorCnt++;		// Quick Start는 Error로 치지 않음.
                if (m_Socket.m_errorcode == -1) 
                {
                    m_ErrorCnt += 20;	// 헤더 파싱 오류는 바로 refresh.
                }
            }
            else
            {
                m_ErrorCnt = 0;
            }
            
            // HeartBeat 3회 오류 혹은 사용자가 다른 곳에서 로그인하여 online 정보 사라졌을 경우 (errorcode = 44)
            if ( m_HeartBeatErrorCnt >= 3)
            {
                CLogger.Log("OnDataReady() / HeartBeat Error 3 Over or User Login from other location. / m_HeartBeatErrorCnt : " + m_HeartBeatErrorCnt + " / error code : " + m_Socket.m_errorcode);

                Close();
            }
            else 
            {
                // 5초 이내로 패킷 수신되면 receive timeout 횟수 초기화
                if ((CMiscFunc.SafeGetTimer() - m_Socket.m_lastReceiveCheckTick) < 5000)
                {
                    m_TimeoutCount = 0;
                }
                
                CNotificationCenter.Instance.PostNotification(CGameConstants.UI_EVENT_CHAT_SOCKET_DATA_READY, new CNotification(m_Socket.m_errorcode.ToString(), m_Socket));
            }
        }

        public bool GameChatLogin()
        {
            CLogger.Log("GameChatLogin() / room index : {0}", m_RoomIdx);
            // int cmd = CProtocolConstants.CMD_CHAT_LOGIN;
            
            m_Socket.m_Key.Number = 128;
            
//			CPacketData _packetData = new CPacketData(cmd, CUserInfo.Instance.Useridx.Number);
//			_packetData.WriteBigInt(m_RoomIdx);
//			
//			m_Socket.PushRequestToQueue(_packetData, -1);	// 로그인 패킷은 모든 패킷에 우선 처리
            
            return true;
        }
        
        public bool GameChatInfo()
        {
            if (!m_IsConnected)	return false;
            
            int cmd = CNetworkConstants.CMD_CHAT_GET_INFO;
            
            if (CheckQueuedRequest(cmd) || m_CurrentCmd == cmd) return false;
            
//			CPacketData _packetData = new CPacketData(cmd, CUserInfo.Instance.Useridx.Number);
//			_packetData.WriteBigInt(m_RoomIdx);
//			_packetData.WriteBigInt(m_ChatIdx);
//			
//			m_Socket.PushRequestToQueue(_packetData);
            
            return true;
        }
        
        public bool GameChatSend(string chattext)
        {
//			CLogger.Log("GameChatSend() / message : {0}", chattext);
            if (!m_IsConnected)	return false;
            
            // int cmd = CProtocolConstants.CMD_CHAT_SEND_GAME;
            
//			CPacketData _packetData = new CPacketData(cmd, CUserInfo.Instance.Useridx.Number);
//			_packetData.WriteBigInt(m_RoomIdx);
//			_packetData.WriteString(chattext);
//			
//			m_Socket.PushRequestToQueue(_packetData);
            
            return true;
        }
        
        public bool GetGameChatIndex()
        {
            if (!m_IsConnected)	return false;
            
            // int cmd = CProtocolConstants.CMD_CHAT_GET_INDEX;
            
//				CPacketData _packetData = new CPacketData(cmd, CUserInfo.Instance.Useridx.Number);
//			_packetData.WriteBigInt(m_RoomIdx);
//			
//			m_Socket.PushRequestToQueue(_packetData);
            
            return true;
        }
        
        public bool CityChatSend(int cityidx, String chattext, int chatType = 0)
        {
            if (!m_IsConnected)	return false;
            
            // int cmd = CProtocolConstants.CMD_CHAT_SEND_CITY;
            
//			CPacketData _packetData = new CPacketData(cmd, CUserInfo.Instance.Useridx.Number);
//			_packetData.WriteShortInt(cityidx);		// 도시 인덱스.
//			_packetData.WriteTinyInt(chatType);		// 도시 채팅 타입.
//			_packetData.WriteBigInt(m_ChatIdx);	// 도시 채팅 인덱스.
//			_packetData.WriteString(chattext);			// 채팅 내용.
//			
//			m_Socket.PushRequestToQueue(_packetData);
            
            return true;
        }
    }
}
