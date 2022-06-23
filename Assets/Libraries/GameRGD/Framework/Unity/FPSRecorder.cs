using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Helen
{
    public class FPSRecorder : MonoBehaviour
    {
        private struct DataInfo
        {
            public int SortOrder;
            public string DataName;
            public Func<float> DataFunc;
            public List<float> DataList;
        }

        public float RecordingInterval = 0.5f;
        public int DataCount = 5;
        private List<DataInfo> datas = new List<DataInfo>();

        private float timeline = 0;
        private int sampleCount = 0;
        private const string TimeLine = "TimeLine";
        private const string GCAlloc = "GCTotal";

        private CancellationTokenSource cancel;

        public void OnUpdate()
        {
            for (int i = 0; i < datas.Count; i++)
            {
                datas[i].DataList.Add(datas[i].DataFunc());
            }
            timeline += RecordingInterval;
            sampleCount++;
        }

        private float GetTimeLine()
        {
            return timeline;
        }

        private float GetTotalGC()
        {
            long gc = System.GC.GetTotalMemory(false);
            float mb = gc / 1024f / 1024f;
            return mb;
        }

        public void AddDataRecord(string dataName, Func<float> func, int sortOrder = 1)
        {
            if (IsRecording())
                return;

            if (func == null)
                return;

            var newList = new List<float>();

            DataInfo newData;
            newData.SortOrder = sortOrder;
            newData.DataName = dataName;
            newData.DataFunc = func;
            newData.DataList = new List<float>();

            datas.Add(newData);
        }

        public bool IsRecording()
        {
            return cancel != null && !cancel.IsCancellationRequested;
        }

        public void Cleanup()
        {
            datas.Clear();
        }

        public void StartRecording()
        {
            if (IsRecording())
                return;

            AddDataRecord(TimeLine, GetTimeLine, 0);
            AddDataRecord(GCAlloc, GetTotalGC);

            timeline = 0;
            sampleCount = 0;

            cancel = new CancellationTokenSource();
            ProcessRecording(cancel).Forget();
        }

        private async UniTask ProcessRecording(CancellationTokenSource cancel)
        {
            int time = (int)(RecordingInterval * 1000);
            while (!cancel.IsCancellationRequested)
            {
                OnUpdate();
                await UniTask.Delay(time, true, PlayerLoopTiming.PostLateUpdate, cancel.Token);
            }
        }

        public void EndRecording()
        {
            if( cancel != null )
                cancel.Cancel();

#if DEV_TOOLS
            WriteData();
#endif
        }

        public float GetFPSAverage()
        {
            var fpsData = datas.Find(X => X.DataName == "FPS");
            if (fpsData.IsNullOrDefault())
                return 0;

            float fps = 0;
            for( int i =0;i < fpsData.DataList.Count; i++ )
            {
                fps += fpsData.DataList[i];
            }
            fps /= fpsData.DataList.Count;

            return fps;
        }

        private void OnDisable()
        {
            if( cancel != null )
                cancel.Cancel();
            Cleanup();
        }

        private void WriteData()
        {
            var path = Application.persistentDataPath + "/SampleData";
            var directory = System.IO.Directory.CreateDirectory(path);

            var files = directory.GetFiles().OrderBy(X => X.CreationTime).ToArray();
            int count = files.Count();
            for (int i = 0; i <= count - DataCount; i++)
            {
                files[i].Delete();
            }

            string filename = path + "/" + DateTime.Now.ToString("MM_dd_HH_mm_ss") + ".csv";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
            {
                file.Write(GetData());
            }
        }

        private string GetData()
        {
            datas.Sort((lhs, rhs) => lhs.SortOrder.CompareTo(rhs.SortOrder));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < datas.Count; i++)
            {
                builder.Append(datas[i].DataName);

                if (i < datas.Count - 1)
                    builder.Append(",");
            }
            builder.Append("\n");

            for (int timeIndex = 0; timeIndex < sampleCount; timeIndex++)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    builder.Append(datas[i].DataList[timeIndex]);

                    if (i < datas.Count - 1)
                        builder.Append(",");
                }
                builder.Append("\n");
            }
            return builder.ToString();
        }
    }
}