namespace DoubleuGames.GameRGD
{
    public interface IChangeable<T>
    {
        T Before { get; set; }
        T Current { get; set; }
    }

    public interface IReadonlyChangeable<T>
    {
        T Before { get; }
        T Current { get; }
    }

    public class CChangeProperty<T> : IChangeable<T>, IReadonlyChangeable<T> where T : new()
    {
        private T mBefore = new T();
        private T mCurrent = new T();

        public T Before
        {
            get => mBefore;
            set => mBefore = value;
        }

        public T Current
        {
            get => mCurrent;
            set
            {
                Before = mCurrent;
                mCurrent = value;
            }
        }
    }
}