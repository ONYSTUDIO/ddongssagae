namespace DoubleuGames.GameRGD
{
    public class CSingleton<T> where T : class, new()
    {
        private static T instance;
        public static T Instance
        {
            get { return instance ?? (instance = new T()); }
        }
    }
}