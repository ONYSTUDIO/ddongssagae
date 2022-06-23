using System;

namespace DoubleuGames.GameRGD
{
    public class CEncryption {
        public const string KEY = "eVBHOulunx8A6spikeRQ9UEgyaXINTyzpn3SJ7FSzmwSlewTWI3";
        
        private static string xor(string source)
        {
            string key = KEY;
            string result = "";
            
            for (int i = 0; i < source.Length; i++) 
            {
                if (i > (key.Length - 1)) {
                    key += key;
                }
                
                result += string.Format("{0}", (char)(source[i] ^ key[i]));
            }
            
            return result;
        }
        
        public static string encode(string source)
        {
            return CBase64.Encode (CEncryption.xor (source));
        }

        public static byte[] encodeBytes(byte[] source)
        {
            byte[] bytearray = new byte[source.Length];
            Array.Copy (source, 0, bytearray, 0, source.Length);

            for (int i=0; i<bytearray.Length; i++)
            {
                bytearray[i] = (byte)((bytearray[i] + i) % 256);
            }

            return bytearray;
        }
        
        public static string decode(string source)
        {
            return CEncryption.xor(CBase64.Decode (source));
        }

        public static byte[] decodeBytes(byte[] source)
        {
            byte[] bytearray = new byte[source.Length];
            Array.Copy (source, 0, bytearray, 0, source.Length);
            
            for (int i=0; i<bytearray.Length; i++)
            {
                bytearray[i] = (byte)((bytearray[i] - i + 256) % 256);
            }
            
            return bytearray;
        }
    }
}
