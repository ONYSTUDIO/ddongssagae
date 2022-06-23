using Cysharp.Threading.Tasks;

namespace Helen
{
    public interface IWaitableMediator
    {
        UniTask Wait();
    }

    public interface IWaitableMediator<T>
    {
        UniTask<T> Wait();
    }
}