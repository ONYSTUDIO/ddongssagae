using System;
using System.Net;
using System.Text;

namespace DoubleuGames.GameRGD
{
    public class CPacketData
    {
        private enum DataTypeSize
        {
            TinyInt = 1,
            ShortInt = 2,
            Int = 4,
            Long = 8,
        };

        private byte[] sendData;
        public byte[] SendData
        {
            get
            {
                return sendData;
            }
        }

        private int sendDataPosition;
        public int Length
        {
            get
            {
                return sendDataPosition;
            }
        }

        public CPacketData(int _cmd, long _userIdx)
        {
            sendData = new byte[CSocketBase.BUFFER_SIZE];
            sendDataPosition = 0;

            WriteHeader(_cmd, _userIdx);
        }

        private void WriteHeader(int _cmd, long _userIdx)
        {
            WriteTinyInt(255);		// 0
            WriteTinyInt(255);		// 1
            WriteTinyInt(1);		// 2
            WriteInt(0);			// 3 ~ 6
            WriteTinyInt(1);		// 7
            WriteShortInt(_cmd);	// 8 ~ 9
            WriteTinyInt(0);		// 10
            WriteInt(0);			// 11 ~ 14
            WriteBigInt(_userIdx);	// 15 ~ 22
        }
        
        private void copyToSendData(byte[] values, int index = -1)
        {
            bool useDataPosition = index == -1;
            if (useDataPosition)
            {
                index = sendDataPosition;
                sendDataPosition += values.Length;
            }

            foreach(byte b in values)
            {
                sendData[index] = b;
                index++;
            }
        }

        public void WriteBigInt(long num, int index = -1)
        {
            if (index == -1)
            {
                if (!CheckDataBuffer(sizeof(long)))
                {
                    throw new CSocketBase.PacketDataWriteException("Buffer is full");
                }
            }
            
            byte[] value;
            value = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(num));
            
            copyToSendData(value, index);
        }
        
        public void WriteInt(int num, int index = -1)
        {
            if (index == -1)
            {
                if (!CheckDataBuffer(sizeof(int)))
                {
                    CLogger.Log("WriteInt() / buffer is full.");
                    throw new CSocketBase.PacketDataWriteException("Buffer is full");
                }
            }
            
            byte[] value;
            value = BitConverter.GetBytes( IPAddress.HostToNetworkOrder((int)num));
            
            copyToSendData(value, index);
        }
        
        public void WriteShortInt(int num, int index = -1)
        {
            if (index == -1)
            {
                if (!CheckDataBuffer(sizeof(short)))
                {
                    throw new CSocketBase.PacketDataWriteException("Buffer is full");
                }
            }
            
            byte[] value;
            value = BitConverter.GetBytes( IPAddress.HostToNetworkOrder((short)num));
            
            copyToSendData(value, index);
        }
        
        public void WriteTinyInt(int num, int index = -1)
        {
            if (index == -1)
            {
                if (!CheckDataBuffer(sizeof(byte)))
                {
                    throw new CSocketBase.PacketDataWriteException("Buffer is full");
                }
            }
            
            byte[] value = new byte[1];
            value[0] = (byte) num;
            
            copyToSendData(value, index);
        }
        
        public void WriteString(string str)
        {
            if (str == null)
                str = "";
            
            byte[] data = Encoding.UTF8.GetBytes(str);
            int length = Encoding.UTF8.GetByteCount (str);
            
            WriteShortInt (length + 1);
            
            if (!CheckDataBuffer(length))
            {
                throw new CSocketBase.PacketDataWriteException("Buffer is full");
            }
            
            if (length == 0)
            {
                WriteTinyInt(0);
            }
            else
            {
                foreach(char c in data)
                {
                    WriteTinyInt(c);
                }
                
                WriteTinyInt(0);
            }
        }
        
        private bool CheckDataBuffer(int size)
        {
            if (sendDataPosition + size > CSocketBase.BUFFER_SIZE)
            {
                return false;
            }
            
            return true;
        }
    }
}

