namespace DoubleuGames.GameRGD
{
    public class CBase64 
    {
        public static string Encode(string data)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(data);
            return System.Convert.ToBase64String(encbuff);
        }
        
        public static string Decode(string data)
        {
            byte[] decbuff = System.Convert.FromBase64String(data);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }
    }
}
