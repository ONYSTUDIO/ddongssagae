using System;
using UnityEngine;
using UniRx;

namespace DoubleuGames.GameRGD
{
    [Serializable]
    public class CReactiveSafeNumber : CSafeNumber, IReadOnlyReactiveSafe<long>
    {
        private readonly Subject<long> mTrigger = new Subject<long>();

        public CReactiveSafeNumber() : base(0L)
        {
        }

        public CReactiveSafeNumber(long initValue) : base(initValue)
        {
        }

        public override long Number
        {
            set
            {
                base.Number = value;
                mTrigger.OnNext(value);
            }
        }

        public IDisposable Subscribe(IObserver<long> observer)
        {
            observer.OnNext(Number);
            return mTrigger.Subscribe(observer);
        }
    }

    [Serializable]
    public class CSafeNumber : IReadOnlySafe<long>
    {
        private readonly object mLock = new object();
        private const string SECURE_STRING = "SECURE14_";
        [SerializeField]
        private long data = 0;
        [SerializeField]
        private long dataDistance = 0;
        [SerializeField]
        private string checksum = "";

        private static bool isabuser = false;
        private static int abusecount = 0;

        public CSafeNumber(long value = 0)
        {
            dataDistance = CMiscFunc.SafeRandom() % 10000 + 7123;
            data = value + dataDistance;

            checksum = CEncryption.encode(string.Format("{0}{1}", SECURE_STRING, data.ToString()));
        }

        public override string ToString()
        {
            return string.Format("[CSafeNumber: {0}]", Number);
        }

        public virtual long Number
        {
            get
            {
                lock (mLock)
                {
                    string checkvalue = CEncryption.decode(checksum);

                    if (checkvalue.Length > 9 && checkvalue.Substring(0, 9) == SECURE_STRING)
                    {
                        checkvalue = checkvalue.Substring(9);
                        long checknumber = long.Parse(checkvalue);

                        if (checknumber != data)
                        {
                            CLogger.Log("Abuse detected : SafeInt");
                            data = checknumber;

                            // 조작이 발견되면 자동 block 및 안내 페이지로 이동
                            if (!isabuser)
                            {
                                abusecount++;

                                // 3회 조작시 Block
                                if (abusecount > 2)
                                {
                                    isabuser = true;
                                    CNotificationCenter.Instance.PostNotification("OCCUR_ABUSER");
                                }
                            }
                        }
                    }
                    else
                    {
                        data = dataDistance;
                    }

                    return data - dataDistance;
                }
            }
            set
            {
                lock (mLock)
                {
                    dataDistance = CMiscFunc.SafeRandom() % 10000 + 873;
                    data = value + dataDistance;

                    checksum = CEncryption.encode(string.Format("{0}{1}", SECURE_STRING, data.ToString()));
                }
            }
        }
    }
}
