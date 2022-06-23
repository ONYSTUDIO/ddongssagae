using System;

namespace DoubleuGames.GameRGD
{
    public class CRefCounter
    {
        public int Count { get; private set; }

        public int Increment()
        {
            return (++Count);
        }

        public int Decrement()
        {
            --Count;
            if (Count < 0)
            {
                Count = 0;
                throw new Exception(nameof(Count));
            }
            return Count;
        }
    }
}
