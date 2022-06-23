using System;
using System.Collections.Generic;
using UniRx;

namespace DoubleuGames.GameRGD
{
    public interface IBase : ICollection<IDisposable>, ICancelable
    {
        IObservable<IBase> OnDisposeAsObservable();
    }
}