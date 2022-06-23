using Cysharp.Threading.Tasks;

namespace Helen
{
    // 비동기 다운로드 명령의 현재 정보 참조용 객체
    public class AssetLoadHandle
    {
        internal AssetLoadHandle(IBundleDownloadOperation operation, UniTask task)
        {
            this.operation = operation;
            this.task = task;
        }

        private IBundleDownloadOperation operation;
        private UniTask task;

        public long SizeKBytes { get { return operation?.SizeKBytes ?? 0; } }
        public long CurrentKBytes { get { return operation?.CurrentSizeKBytes ?? 0; } }
        public int Count { get { return operation?.Count ?? 0; } }
        public int CurrentCount { get { return operation?.CurrentCount ?? 0; } }
        public bool IsDone { get { return operation?.IsDone ?? true; } }
        public bool IsSucceeded { get { return operation?.IsSucceeded ?? false; } }
        public UniTask Task
        {
            get
            {
                if (task.Status.IsCompleted())
                    return UniTask.FromResult(default(object));
                return UniTask.WaitUntil(() => task.Status.IsCompleted());
            }
        }
        public object Result
        {
            get
            {
                var handle = operation?.MainAddressableHandle;
                return handle.IsNullOrDefault() ? null : handle?.Result;
            }
        }

        public UniTask.Awaiter GetAwaiter()
        {
            if (task.Status.IsCompleted())
                return UniTask.FromResult(default(object)).AsUniTask().GetAwaiter();
            return UniTask.WaitUntil(() => task.Status.IsCompleted()).GetAwaiter();
        }
    }
}