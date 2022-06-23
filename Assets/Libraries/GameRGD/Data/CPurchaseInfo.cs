using Newtonsoft.Json;

namespace DoubleuGames.GameRGD
{
    public class CPurchaseInfo
    {
        public int error = -1;
        public string product_id = "";
        public string order_id = "";
        public string receipt = "";
        public string signature = "";
        public string message = "";

        public CPurchaseInfo()
        {

        }

        public override string ToString ()
        {
            return string.Format ($"[CPurchaseInfo: Error={error}, ProductId={product_id}, OrderId={order_id}, Receipt={receipt}, Signature={signature}, Message={message}]");
        }

        public static CPurchaseInfo ConvertFrom(string jsonString)
        {
            if (jsonString == null || jsonString.Length <= 0)
            {
                return null;
            }
            var _data = JsonConvert.DeserializeObject<CPurchaseInfo>(jsonString);
            return _data;
        }
    }
}