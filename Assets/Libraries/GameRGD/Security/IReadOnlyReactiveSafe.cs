using System;

namespace DoubleuGames.GameRGD
{
    public interface IReadOnlySafe<T>
    {
        T Number { get; }
    }

    public interface IReadOnlyReactiveSafe<T> : IObservable<T>, IReadOnlySafe<T>
    {
    }
}