using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class QueueProcesser<T>
{
    private Queue<T> processQueue;
    private CancellationTokenSource cancelProcessQueue;

    public bool Running
    {
        get => processQueue != null;
    }

    public bool Start(Func<T, CancellationToken, UniTask> processMethod)
    {
        if (Running)
            return false;

        processQueue = new Queue<T>();
        cancelProcessQueue = new CancellationTokenSource();

        ProcessQueue(cancelProcessQueue, processMethod, processQueue).Forget();

        return true;
    }

    public void Enqueue(T item)
    {
        if (processQueue != null)
            processQueue.Enqueue(item);
    }

    public void Stop()
    {
        processQueue = null;

        if (cancelProcessQueue != null)
        {
            cancelProcessQueue.Cancel();
            cancelProcessQueue = null;
        }
    }

    public void Clear()
    {
        if (processQueue != null)
            processQueue.Clear();
    }

    private static async UniTask ProcessQueue(
        CancellationTokenSource cancellationToken, Func<T, CancellationToken, UniTask> processMethod, Queue<T> processQueue)
    {
        while (true)
        {
            if (processQueue == null || processQueue.Count <= 0)
            {
                await UniTask.Yield();
                continue;
            }

            cancellationToken.Token.ThrowIfCancellationRequested();

            var item = processQueue.Dequeue();
            await processMethod(item, cancellationToken.Token);
            continue;
        }
    }
}