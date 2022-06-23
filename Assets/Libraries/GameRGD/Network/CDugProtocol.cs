using System;

namespace DoubleuGames.GameRGD
{
    public class CDugProtocol : System.Attribute
    {
        private int m_Cmd;
        public int Cmd { get => m_Cmd; }
        private Type m_ResponseType;
        public Type ResponseType { get => m_ResponseType; }

        public CDugProtocol(int cmd, Type responseType = default)
        {
            this.m_Cmd = cmd;
            this.m_ResponseType = responseType;
        }
    }
}