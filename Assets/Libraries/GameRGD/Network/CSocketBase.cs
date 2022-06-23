#pragma warning disable 0168

using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DoubleuGames.GameRGD
{
    public class CSocketBase 
    {
        private enum DataTypeSize
        {
            TinyInt = 1,
            ShortInt = 2,
            Int = 4,
            Long = 8,
        };

        public const int HEADER_LENGTH = 11;
        public const int BUFFER_SIZE = 1024 * 10;
        public const int RECEIVE_BUFFER_SIZE = 1024 * 10 * 4;
        public const int CONNECT_TIMEOUT = 30 * 1000;
        private Socket socket = null;
        public bool m_isDataReady;
        public int m_state;
        public int m_redundancy = 0;
        public int m_lastSockDataFetchTime = 0;
        public int m_latency = 0;
        public int m_lastLatencyCheckTick;
        public int m_lastReceiveCheckTick;
        public int m_lastSendTick = 0;
        public int m_receiveStartTick;
        public int m_cmd = 0;
        public int m_errorcode = 0;
        public int m_packet_len;
        public string m_host;
        public int m_port;
        public int m_lastSendCheckTick = 0;
        
        protected bool isFirstPacket = true;

        protected byte[] byteData;
        public int byteDataPosition = 0;

        protected int totalBytes;
        protected int called;

        private byte[] sendBytes;
        private int sendBytesPosition = 0;

        protected int packet_remains = 0;

        protected int m_SendCmd = 0;		
        protected int m_LastPacketLength = 0;
        
        public ArrayList m_Queue;
        public CSafeInt m_Key = new CSafeInt(128);
        
        protected bool m_bHeaderRead = false;
        protected int m_RemainingBytes = 0;
        protected int m_PacketNumber = 9;
        
        public int m_SocketLiveTick = 0;

        private byte[] currentDummyData;
        private byte[] sendReadyByte;

        public System.Object thisLock = new System.Object();

        public class PacketDataWriteException : Exception
        {
            public enum ErrorType
            {
                BufferIsFull,   // 버퍼가 꽉참
                DataTypeError   // 데이터 타입을 잘못 고름
            };
            
            public PacketDataWriteException(string message):base(message)
            {
                
            }
        }

        public bool Connected
        {
            get
            {
                if (socket != null)
                {
                    return socket.Connected;
                }

                return false;
            }
        }
        
        public CSocketBase() 
        {
            m_SocketLiveTick = CMiscFunc.SafeGetTimer ();
            
            called = 0;
            isFirstPacket = true;
            m_isDataReady = false;
            m_state = -2;
            
            sendBytes = new byte[BUFFER_SIZE];

            m_Queue = new ArrayList();
            
            ResetBuffer();
        }

        public void Connect(string host, int port)
        {
            try
            {
                m_host = host;
                m_port = port;

                CLogger.Log("Connect() / host : {0} / port : {1}", host, port);

#if UNITY_EDITOR
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
#elif UNITY_IOS || UNITY_ANDROID
                CLogger.Log("Local IPv4: " + getIPAddress("", IPTYPE.LOCAL_IP, ADDRESSFAM.IPv4)); 
                CLogger.Log("Local IPv6: " + getIPAddress("", IPTYPE.LOCAL_IP, ADDRESSFAM.IPv6)); 
                CLogger.Log("Public IPv4: " + getIPAddress(host, IPTYPE.PUBLIC_IP, ADDRESSFAM.IPv4)); 
                CLogger.Log("Public IPv6: " + getIPAddress(host, IPTYPE.PUBLIC_IP, ADDRESSFAM.IPv6));

                if (Socket.OSSupportsIPv6)
                {
                    CLogger.Log("OSSupportsIPv6 true");
                    string IPv6String = getIPAddress(host, IPTYPE.PUBLIC_IP, ADDRESSFAM.IPv6);
                    
                    if (!string.IsNullOrEmpty(IPv6String))
                    {
                        CLogger.Log("Connect InterNetworkV6");
                        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    }
                    else
                    {
                        CLogger.Log("Connect InterNetwork");
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                }
                else
                {
                    CLogger.Log("Connect InterNetwork");
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
#else
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
#endif
                var addresses = System.Net.Dns.GetHostAddresses(host);

                IPEndPoint ipep = new IPEndPoint(addresses[0], port);
                
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.RemoteEndPoint = ipep;
               
                IAsyncResult result = socket.BeginConnect(ipep, OnConnected, socket);
            }
            catch (Exception e)
            {
                CLogger.LogError("Connect() / error : {0}", e.Message);

                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_IO_ERROR, null, false);
            }
        }

        public void Close()
        {
            try
            {
                if (socket != null)
                {
                    socket.Close ();
                }
            }
            catch (Exception e)
            {
                //CLogger.LogError("Close() / error : {0}", e.Message);
            }

            socket = null;
        }

        private void OnConnected(System.IAsyncResult iar)
        {
//			//CLogger.Log("OnConnected()");
            Socket client = (Socket)iar.AsyncState;
            
            try
            {
                client.EndConnect(iar);

                m_SocketLiveTick = CMiscFunc.SafeGetTimer();
                
                m_state = 1;
                m_lastSendTick = 0;

                Thread receiver = new Thread(new ThreadStart(ReadResponse));
                receiver.Start();
                CLogger.Log("OnConnected() / receive thread start.");

                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_CONNECT, null, false);
            }
            catch(SocketException e)
            {
                // TODO : Error
                CLogger.Log("OnConnected() / error : {0}", e.Message);
                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_IO_ERROR, null, false);
            }
        }

        public bool PushRequestToQueue(CPacketData _packetData, int _priority = 0)
        {
            if (m_state < 0) 
            {
                CLogger.Log("PushRequestToQueue() / state invalid. so, return. / state : {0}", m_state);
                return false;
            }

            lock (thisLock)
            {
                byte[] bytes = new byte[_packetData.Length];
                Array.Copy (_packetData.SendData, 0, bytes, 0, _packetData.Length);
                
                int cmd = ReadShortInt (bytes, 8);
                if (cmd == 0)
                {
                    CLogger.LogError("PushRequestToQueue() / Protocol Command is zero.");
                    throw new System.NotSupportedException("Protocol Command is zero.");
                }
                
                m_Queue.Add(new CPriorityByteArray(CEncryption.encodeBytes(bytes), _priority));
                
                Array.Clear (bytes, 0, bytes.Length);
                bytes = null;
                _packetData = null;
            }
            return true;
        }
        
        // 기존에 요청할 request 추가.
        public void PushMultipleRequestToQueue(ArrayList requests)
        {
            if (m_state < 0) return;
            if (requests.Count == 0) return;

            lock (thisLock)
            {
                Dictionary<int, int> checkDuplicatedCmd = new Dictionary<int, int>();
                for (int i=0; i<requests.Count; i++)
                {
                    byte[] temp = CEncryption.decodeBytes((byte[])requests[i]);
                    int cmd = ReadShortInt (temp, 8);
                    
                    //CLogger.Log("PushMultipleRequestToQueue() / add packet cmd : {0}", cmd);
                    
                    Array.Clear(temp, 0, temp.Length);
                    temp = null;
                    
                    if (checkDuplicatedCmd.ContainsKey(cmd) == false &&
                        CheckQueuedRequest(cmd) == false)
                    {
                        checkDuplicatedCmd.Add(cmd, 1);
                        m_Queue.Add(new CPriorityByteArray((byte[])requests[i], 0));
                    }
                }
            }
        }
        
        // 소켓 끊겼을 때 다시 전송해야할 Request 나열.
        public ArrayList GetRequestQueue()
        {
            m_Queue.Sort (new CPriorityByteArrayCompareAsce ());
            
            ArrayList queue = new ArrayList();
            
            for (int i=0; i<m_Queue.Count; i++)
            {
                byte[] bytes = CEncryption.decodeBytes((m_Queue[i] as CPriorityByteArray).ByteArray);
                int cmd = ReadShortInt (bytes, 8);
                
                // 로그인과 HeartBeat는 제외하고 요청.
                if (cmd != 1 && cmd != 3)
                {
                    queue.Add((m_Queue[i] as CPriorityByteArray).ByteArray);
                }
            }
            
            return queue;
        }
        
        public int GetQueueSize(int cmd=-1)
        {
            int count = 0;
            
            if (cmd >= 1)
            {
                for (int i=m_Queue.Count-1; i>=0; i--)
                {
                    byte[] bytes = CEncryption.decodeBytes((m_Queue[i] as CPriorityByteArray).ByteArray);
                    int child_cmd = ReadShortInt(bytes, 8);
                    
                    if (child_cmd == cmd)
                    {
                        count++;
                    }
                }
            }
            else
            {
                count = m_Queue.Count;
            }
            
            return count;
        }
        
        public bool CheckQueuedRequest(int cmd)
        {
            for (int i=m_Queue.Count-1; i>=0; i--)
            {
                byte[] bytes = CEncryption.decodeBytes((m_Queue[i] as CPriorityByteArray).ByteArray);
                int child_cmd = ReadShortInt(bytes, 8);
                
                if (child_cmd == cmd)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void CancelQueuedRequest(int cmd)
        {
            for (int i=m_Queue.Count-1; i>=0; i--)
            {
                byte[] bytes = CEncryption.decodeBytes((m_Queue[i] as CPriorityByteArray).ByteArray);
                int child_cmd = ReadShortInt(bytes, 8);
                
                if (child_cmd == cmd || cmd == -1)
                {
                    m_Queue[i] = null;
                    m_Queue.RemoveAt(i);
                }
            }
        }
        
        public byte[] GetQueueRequestBytes(int cmd)
        {
            byte[] bytearr = null;
            
            for (int i=m_Queue.Count-1; i>=0; i--)
            {
                byte[] bytes = CEncryption.decodeBytes((m_Queue[i] as CPriorityByteArray).ByteArray);
                int child_cmd = ReadShortInt(bytes, 8);

                if (child_cmd == cmd)
                {
                    // 복사.
                    bytearr = new byte[bytes.Length];
                    Array.Copy(bytes, 0, bytearr, 0, bytes.Length);
                    
                    // 삭제.
                    m_Queue.RemoveAt(i);
                    break;
                }
            }
            
            return bytearr;
        }
        
        public int PeekFirstQueuedRequest()
        {
            if (m_state < 0) 
            {
                return -1;
            }
            
            if (m_Queue.Count == 0) 
            {
                return -1;
            }

            m_Queue.Sort (new CPriorityByteArrayCompareAsce ());
            
            byte[] bytes = CEncryption.decodeBytes((m_Queue[0] as CPriorityByteArray).ByteArray);
            int cmd = ReadShortInt (bytes, 8);
            
            return cmd;
        }

        public int PreSendQueuedRequest()
        {
            if (m_state < 0) 
            {
                CLogger.Log("SendQueuedRequest() / state invalid. so, return");
                return -1;
            }
            
            if (m_Queue.Count == 0) 
            {
                return -1;
            }

            m_Queue.Sort (new CPriorityByteArrayCompareAsce ());
            CPriorityByteArray request = (CPriorityByteArray)m_Queue[0];
            m_Queue.RemoveAt (0);

            sendReadyByte = CEncryption.decodeBytes(request.ByteArray);
            int cmd = ReadShortInt (sendReadyByte, 8);

            request = null;
            return cmd;
        }
        
        public void SendQueuedRequest()
        {
            Array.Clear (sendBytes, 0, sendBytes.Length);
            sendBytesPosition = 0;
            
            WriteBytes(sendReadyByte);
            SendRequest ();
            
            sendReadyByte = null;
        }
        
        public void ResetBuffer()
        {	
            if (byteData != null) 
            {
                Array.Clear(byteData, 0, byteData.Length);
            }

            byteDataPosition = 0;
            totalBytes = 0;
            isFirstPacket = true;			
            called = 0;
        }

        public void GetCurrentCmd(out byte[] _outData)
        {
            _outData = null;

            if (currentDummyData != null)
            {
                int cmd = ReadShortInt (currentDummyData, 8);

                if (cmd != 1 && cmd != 3 && cmd != 4)
                {
                    _outData = CEncryption.encodeBytes(currentDummyData);

                    Array.Clear (currentDummyData, 0, currentDummyData.Length);
                    currentDummyData = null;
                }
            }
        }

        public bool IsExistRetryCommand()
        {
            if (currentDummyData != null)
            {
                return true;
            }

            return false;
        }

        public void RetryCurrentCmd()
        {
            CLogger.Log("RetryCurrentCmd()");
            lock(thisLock)
            {
                //CLogger.Log("RetryCurrentCmd()");
                Array.Clear (sendBytes, 0, sendBytes.Length);
                sendBytesPosition = 0;
                
                WriteBytes(currentDummyData);

                CLogger.Log("RetryCurrentCmd() / send request.");
                SendRequest ();
            }
        }

        public void WriteBytes(byte[] bytes)
        {
            if (!CheckSendBytes(bytes.Length))
            {
                //CLogger.Log("WriteBytes() / buffer is full. - send byte position : {0}", sendBytesPosition);
                throw new PacketDataWriteException("WriteBytes() / Buffer is full");
            }
            
            CopyToSendBytes (bytes);
        }
        
        public void WriteInt(int num, int index = -1)
        {
            if (index == -1)
            {
                if (!CheckSendBytes(sizeof(int)))
                {
                    //CLogger.Log("WriteRealSendDataInt() / buffer is full. - send byte position : {0}", sendBytesPosition);
                    throw new PacketDataWriteException("WriteInt() / Buffer is full");
                }
            }
            
            byte[] value;
            value = BitConverter.GetBytes( CIpAddress.HostToNetworkOrder((int)num));
            
            CopyToSendBytes(value, index);
        }
        
        public void WriteTinyInt(int num, int index = -1)
        {
            if (index == -1)
            {
                if (!CheckSendBytes(sizeof(byte)))
                {
                    //CLogger.Log("WriteTinyInt() / buffer is full. - send byte position : {0}", sendBytesPosition);
                    throw new PacketDataWriteException("WriteTinyInt() / Buffer is full");
                }
            }
            
            byte[] value = new byte[1];
            value[0] = (byte) num;
            
            CopyToSendBytes(value, index);
        }
        
        private void CopyToSendBytes(byte[] values, int index = -1)
        {
            bool useDataPosition = index == -1;
            if (useDataPosition)
            {
                index = sendBytesPosition;
                sendBytesPosition += values.Length;
            }
            
            foreach(byte b in values)
            {
                sendBytes[index] = b;
                index ++ ;
            }
        }
        
        private bool CheckSendBytes(int size)
        {
            if ( sendBytesPosition + size > BUFFER_SIZE)
            {
                return false;
            }
            
            return true;
        }
        
        private void SendRequest()
        {
            try
            {
                if (m_state < 0) 
                {
                    CLogger.Log("SendRequest() / state invalid. so, return. / state : {0}", m_state);
                    return;
                }
                if (CGameNetwork.Instance.IsAbused.Number == true) 
                {
                    CLogger.Log("SendRequest() / check abuser. so, return.");
                    return;
                }
                
                if (currentDummyData != null)
                {
                    Array.Clear (currentDummyData, 0, currentDummyData.Length);
                    currentDummyData = null;
                }
                
                currentDummyData = new byte[sendBytesPosition];
                Array.Copy (sendBytes, 0, currentDummyData, 0, sendBytesPosition);
                
                // 패킷 길이 업데이트.
                WriteInt(sendBytesPosition, 3);
                
                // 패킷 번호 업데이트 (packet repeating 방지 용도)
                m_PacketNumber++;
                
                if (m_PacketNumber > 250)
                {
                    m_PacketNumber = 10;
                }
                
                WriteTinyInt(m_PacketNumber, 7);
                
                int cmd = ReadShortInt (sendBytes, 8);
                m_SendCmd = cmd;
                
                WriteInt(CMiscFunc.SafeGetTimerNetwork(), 11);
                
                EncodeRealSendBytes();
                
                m_lastLatencyCheckTick = CMiscFunc.SafeGetTimer();
                m_lastReceiveCheckTick = CMiscFunc.SafeGetTimer();
                
                if (m_lastSendTick == 0) 
                {
                    m_lastSendTick = CMiscFunc.SafeGetTimer();
                }
                m_lastSendCheckTick = CMiscFunc.SafeGetTimer();
                
                m_state = 2;
                m_isDataReady = false;
                m_bHeaderRead = false;
                
                CLogger.Log("SendRequest() / send / cmd : {0} ({1})", m_SendCmd, sendBytesPosition);

                int result = socket.Send(sendBytes, 0, sendBytesPosition, SocketFlags.None);
                m_SocketLiveTick = CMiscFunc.SafeGetTimer();
            }
            catch (Exception e)
            {
                CLogger.LogWarning("SendRequest() / exception : {0}", e.Message);
                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_IO_ERROR, null, false);
            }
        }

        protected void EncodeRealSendBytes()
        {
            int key = m_Key.Number;
            int orgkey = m_Key.Number;

            for (int i=0; i<sendBytesPosition; i++)
            {
                if (i<= 14) 
                {
                    sendBytes[i] +=  (byte)(key + orgkey);
                }
                else  
                {
                    sendBytes[i] += (byte)key;
                }
                
                key = sendBytes[i];
            }
        }
        
        protected void ReadResponse()
        {	
//			if (CApplicationManager.Instance.TestUse)
//			{
//				return;
//			}
            byte[] receiveData = new byte[4096];

            while(true)
            {
                try
                {
                    if (byteData == null)
                    {
                        byteData = new byte[RECEIVE_BUFFER_SIZE];
                    }

                    if (receiveData == null)
                    {
                        receiveData = new byte[4096];
                    }

                    Array.Clear(receiveData, 0, receiveData.Length);

                    if (socket == null)
                    {
                        throw new Exception("socket is null.");
                    }

                    CLogger.Log("ReadResponse() / receive wait ...");
                    int nLength = socket.Receive(receiveData);

                    CLogger.Log("ReadResponse() / length : {0}", nLength);

                    if (isFirstPacket && nLength < HEADER_LENGTH) 
                    {
                        CLogger.LogError("ReadResponse() / error 1");
                        throw new Exception("data packet invalid.");
                    }
                    
                    if (m_state < 0) 
                    {
                        CLogger.LogError("ReadResponse() / error 2");
                        throw new Exception("socket state is invalid.");
                    }
                    
                    if (packet_remains < 0) 
                    {
                        CLogger.LogError("ReadResponse() / error 3");
                        throw new Exception("data packet invalid (ext)");
                    }
                    
                    if (packet_remains != 0 && nLength > packet_remains)
                    {
                        nLength = packet_remains;
                    }
                    
                    Array.Copy(receiveData, 0, byteData, byteDataPosition, nLength);
                    totalBytes += nLength;
                    
                    int keycode = m_Key.Number;
                    int orgkeycode = m_Key.Number;

                    CLogger.Log("ReadResponse() / key code : {0}", keycode);

                    int orgcode = 0;
                    int i;

                    m_lastReceiveCheckTick = CMiscFunc.SafeGetTimer();
                    
                    if (isFirstPacket && totalBytes >= 7)	// 첫번째 패킷인 경우 헤더분석.
                    {
                        m_bHeaderRead = true;
                        
                        m_receiveStartTick = CMiscFunc.SafeGetTimer();				
                        isFirstPacket = false;
                        int savepos = byteDataPosition;
                        
                        // 헤더만 임시로 디코딩.
                        byte[] header_bytes = new byte[7];
                        Array.Copy(byteData, 0, header_bytes, 0, 7);
                        
                        for (i=0; i<7; i++)
                        {
                            orgcode = header_bytes[i];
                            header_bytes[i] -= (byte)(keycode + orgkeycode);
                            keycode = orgcode;
                        }
                        
                        byteDataPosition = savepos;
                        m_packet_len = ReadInt(header_bytes, 3);
                        
                        packet_remains = m_packet_len;
                        
                        if (header_bytes[0] != 0xFF || header_bytes[1] != 0xFF ||  m_packet_len < 11)
                        {
                            CLogger.Log("ReadResponse() / header error / ({0}, {1})", header_bytes[0], header_bytes[1]);
                            m_errorcode = -1;
                            setLastDataFetchTime();
                            m_latency = CMiscFunc.SafeGetTimer() - m_lastLatencyCheckTick;
                            m_state = 3;
                            m_isDataReady = true;
                            packet_remains = 0;
                            
                            CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_DATA_READY, null, false);
                            m_LastPacketLength = m_packet_len;
                            
                            header_bytes = null;
                            
                            ResetBuffer();

                            m_lastSendTick = 0;
                            CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_DATA_PROCESSED, null, false);

                            if (currentDummyData != null)
                            {
                                Array.Clear (currentDummyData, 0, currentDummyData.Length);
                                currentDummyData = null;
                            }
                            
                            m_SocketLiveTick = CMiscFunc.SafeGetTimer();
                            continue;					
                        }
                        
                        header_bytes = null;
                    }
                    
                    packet_remains -= nLength;
                    byteDataPosition = totalBytes;
                    
                    m_RemainingBytes = packet_remains;
                    
                    if (packet_remains == 0)
                    {
                        m_bHeaderRead = false;
                        
                        if (m_state == 2 || m_packet_len != m_LastPacketLength) // 여러번 들어오는 경우 처음 한번만 처리.
                        {
                            // 전체 패킷 디코딩.
                            int j = 0;
                            for (j=0; j<byteData.Length; j++)
                            {
                                orgcode = byteData[j];
                                byteData[j] -= (byte)(keycode + orgkeycode);
                                keycode = orgcode;
                            }
                            
                            m_cmd = ReadShortInt(8);

                            if (m_cmd != m_SendCmd)
                            {
                                CLogger.LogError("ReadResponse() / packet order error. / cmd : {0} / send cmd : {1}", m_cmd, m_SendCmd);
                            }

                            m_errorcode = ReadTinyInt(10);
                            
                            setLastDataFetchTime();
                            m_latency = CMiscFunc.SafeGetTimer() - m_lastLatencyCheckTick;
                            
                            CLogger.Log("ReadResponse() / cmd : {0} / error code : {1} / latency : {2}", m_cmd, m_errorcode, m_latency);
                            
                            m_state = 3;
                            m_isDataReady = true;
                            packet_remains = 0;
                            //						Array.Clear(sendBytes, 0, sendBytes.Length);
                            CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_DATA_READY, null, false);
                            
                            m_LastPacketLength = m_packet_len;
                            
                            ResetBuffer();
                            
                            m_lastSendTick = 0;
                            CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_DATA_PROCESSED, null, false);

                            if (currentDummyData != null)
                            {
                                Array.Clear (currentDummyData, 0, currentDummyData.Length);
                                currentDummyData = null;
                            }
                            
                            m_SocketLiveTick = CMiscFunc.SafeGetTimer();
                        }
                    }
                }
                catch (Exception e)
                {
                    CLogger.LogWarning("ReadResponse() / exception : {0}", e.Message);
                    CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_IO_ERROR, null, false);
                    return;
                }
            }
        }

        public CSocketBase SetDummyData(int length, byte[] data)
        {
            byteDataPosition = length;
            byteData = new byte[RECEIVE_BUFFER_SIZE];
            Array.Copy(data, byteData, data.Length);
            packet_remains = 0;
            totalBytes = data.Length;
            m_state = 2;

            return this;
        }
        
        public void SetReadPosition(int pos)
        {
            byteDataPosition = pos;
        }
        
        public string ReadString(int pos = -1)
        {
            if (m_state < 0) 
            {
                return "";
            }
            
            if (pos > 0) 
            {
                byteDataPosition = pos;
            }

            if (byteData.Length < byteDataPosition + 2) 
            {
                CLogger.Log("ReadString() / 1 / state = -1");
                m_state = -1;
                try
                {
                    Close();
                }
                catch (Exception e) 
                {
                    CLogger.Log("ReadString() / Exception Occur! / message : " + e.Message);
                }

                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_CLOSE, null, false);
                return "";
            }
            
            int length = ReadShortInt();
            string result = "";

            if (length != 0)
            {
                if (byteData.Length < byteDataPosition + length)
                {
                    CLogger.Log("ReadString() / 2 / state = -1");
                    m_state = -1;
                    try
                    {
                        Close();
                    }
                    catch(Exception e)
                    {
                        CLogger.Log("ReadString() / Exception Occur! / message : " + e.Message);
                    }

                    CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_CLOSE, null, false);
                    return "";
                }

                // 서버에서 string 처리 시 마지막에 널 문자 처리되어 마지막 문자를 읽지 않도록 처리.

                result = Encoding.UTF8.GetString(byteData, byteDataPosition, length - 1);
            }
            
            byteDataPosition += length;

            return result;
        }

        public static long ReadBigInt(byte[] value, int index = 0)
        {
            var result = BitConverter.ToInt64(value, index);
            result = CIpAddress.NetworkToHostOrder(result);
            
            return result;
        }

        public long ReadBigInt(int pos = -1)
        {	
            if (m_state < 0) 
            {
                return 0;
            }
            
            if (pos > 0) 
            {
                byteDataPosition = pos;
            }
            
            if (byteData.Length < (byteDataPosition + 8)) 
            {
                CLogger.Log("ReadBigInt() / state = -1");
                m_state = -1;
                try
                {
                    Close();
                }
                catch (Exception e) 
                {
                    CLogger.Log("ReadBigInt() / Exception Occur! / message : " + e.Message);
                }
                
                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_CLOSE, null, false);
                return 0;
            }
            
            var result = BitConverter.ToInt64(byteData, byteDataPosition);
            result = CIpAddress.NetworkToHostOrder(result);
            byteDataPosition += 8;
            
            return result;
        }

        public static int ReadInt(byte[] value, int index = 0)
        {
            var result = BitConverter.ToInt32(value, index);
            result = CIpAddress.NetworkToHostOrder(result);
            
            return result;
        }
        
        public int ReadInt(int pos = -1)
        {	
            if (m_state < 0) 
            {
                return 0;
            }
            
            if (pos > 0) 
            {
                byteDataPosition = pos;
            }
            
            if (byteData.Length < byteDataPosition + 4) 
            {
                CLogger.Log("ReadInt() / state = -1");
                m_state = -1;
                try
                {
                    Close();
                }
                catch (Exception e) 
                {
                    CLogger.Log("ReadInt() / Exception Occur! / message : " + e.Message);
                }
                
                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_CLOSE, null, false);
                return 0;
            }
            
            var result = BitConverter.ToInt32(byteData, byteDataPosition);
            result = CIpAddress.NetworkToHostOrder(result);
            byteDataPosition += 4;
            
            return result;
        }

        public static short ReadShortInt(byte[] value, int index = 0)
        {
            var result = BitConverter.ToInt16(value, index);
            result = CIpAddress.NetworkToHostOrder(result);
            
            return result;
        }
        
        public short ReadShortInt(int pos = -1)
        {
            if (m_state < 0) 
            {
                return 0;
            }
            
            if (pos > 0) 
            {
                byteDataPosition = pos;
            }

            if (byteData.Length < byteDataPosition + 2) 
            {
                CLogger.Log("ReadShortInt() / state = -1");
                m_state = -1;
                try
                {
                    Close();
                }
                catch (Exception e) 
                {
                    CLogger.Log("ReadShortInt() / Exception Occur! / message : " + e.Message);
                }

                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_CLOSE, null, false);
                return 0;
            }

            var result = BitConverter.ToInt16(byteData, byteDataPosition);
            result = CIpAddress.NetworkToHostOrder(result);
            byteDataPosition += 2;

            return result;
        }

        public static byte ReadTinyInt(byte[] value, int index = 0)
        {
            var result = value[index];
            return result;
        }

        public byte ReadTinyInt(int pos = -1)
        {	
            if (m_state < 0) 
            {
                return 0;
            }
            
            if (pos > 0) 
            {
                byteDataPosition = pos;
            }
            
            if (byteData.Length < byteDataPosition + 1) 
            {
                CLogger.Log("ReadTinyInt() / state = -1");
                m_state = -1;
                try
                {
                    Close();
                }
                catch (Exception e) 
                {
                    CLogger.Log("ReadTinyInt() / Exception Occur! / message : " + e.Message);
                }
                
                CNotificationCenter.Instance.PostNotification(CGameConstants.SOCKET_CLOSE, null, false);
                return 0;
            }
            
            var result = byteData[byteDataPosition];
            byteDataPosition += 1;
            
            return result;
        }

        public int setLastDataFetchTime()
        {
            return m_lastSockDataFetchTime = CMiscFunc.SafeGetTimer();
        }
        
        public int getLastDataFetchTime()
        {
            return m_lastSockDataFetchTime;
        }
        
        public int setReceiveRedundancy()
        {
            return (this.m_redundancy = CMiscFunc.SafeGetTimer() - this.m_receiveStartTick);
        }
        
        public int getReceiveRedundancy()
        {
            return this.m_redundancy;
        }
        
        public int setLatency()
        {
            return (this.m_latency = CMiscFunc.SafeGetTimer() - this.m_lastLatencyCheckTick);
        }
        
        public int getLatency()
        {
            return this.m_latency;
        }

        private void PrintPacket(byte[] data)
        {
            StringBuilder stringBuilder = new StringBuilder ();

            foreach(byte item in data)
            {
                stringBuilder.Append(item);
                stringBuilder.Append(" ");
            }

            //CLogger.Log("PrintPacket() / data : " + stringBuilder.ToString());
        }


        // Check IPv4, IPv6 - start (http://yuluer.com/page/dhggccff-resolve-ipv6-address-from-hostname.shtml)
        enum IPTYPE 
        { 
            LOCAL_IP, 
            PUBLIC_IP 
        } 
        enum ADDRESSFAM 
        { 
            IPv4, 
            IPv6 
        }
        private string getIPAddress(string hostName, IPTYPE ipType, ADDRESSFAM Addfam) 
        { 
            //Return null if ADDRESSFAM is Ipv6 but Os does not support it 
            if (Addfam == ADDRESSFAM.IPv6 && !System.Net.Sockets.Socket.OSSupportsIPv6) 
            { 
                return null; 
            } 
            //////////////HANDLE LOCAL IP(IPv4 and IPv6)////////////// 
            if (ipType == IPTYPE.LOCAL_IP) 
            { 
                System.Net.IPHostEntry host; 
                string localIP = ""; 
                host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()); 
                foreach (System.Net.IPAddress ip in host.AddressList) 
                { 
                    //IPv4 
                    if (Addfam == ADDRESSFAM.IPv4) 
                    { 
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) 
                        { 
                            localIP = ip.ToString(); 
                        } 
                    } 
                    //IPv6 
                    else if (Addfam == ADDRESSFAM.IPv6) 
                    { 
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) 
                        { 
                            localIP = ip.ToString(); 
                        } 
                    } 
                } 
                return localIP; 
            } 
            //////////////HANDLE PUBLIC IP(IPv4 and IPv6)////////////// 
            if (ipType == IPTYPE.PUBLIC_IP) 
            { 
                //Return if hostName String is null 
                if (string.IsNullOrEmpty(hostName)) 
                { 
                    return null; 
                } 
                System.Net.IPHostEntry host; 
                string localIP = ""; 
                host = System.Net.Dns.GetHostEntry(hostName); 
                foreach (System.Net.IPAddress ip in host.AddressList) 
                { 
                    //IPv4 
                    if (Addfam == ADDRESSFAM.IPv4) 
                    { 
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) 
                        { 
                            localIP = ip.ToString(); 
                        } 
                    } 
                    //IPv6 
                    else if (Addfam == ADDRESSFAM.IPv6) 
                    { 
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) 
                        { 
                            localIP = ip.ToString(); 
                        } 
                    } 
                } 
                return localIP; 
            } 
            return null; 
        } 
        // Check IPv4, IPv6 - end
    }
}
