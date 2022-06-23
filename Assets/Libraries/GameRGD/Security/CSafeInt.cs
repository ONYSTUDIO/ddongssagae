using System;
using UnityEngine;
using UniRx;

namespace DoubleuGames.GameRGD
{
    [Serializable]
    public class CReactiveSafeInt : CSafeInt, IReadOnlyReactiveSafe<int>
    {
        private readonly Subject<int> mTrigger = new Subject<int>();

        public CReactiveSafeInt() : base(0)
        {
        }

        public CReactiveSafeInt(int initValue) : base(initValue)
        {
        }

        public override int Number
        {
            set
            {
                base.Number = value;
                mTrigger.OnNext(value);
            }
        }

        public IDisposable Subscribe(IObserver<int> observer)
        {
            observer.OnNext(Number);
            return mTrigger.Subscribe(observer);
        }
    }

    [Serializable]
    public class CSafeInt : IReadOnlySafe<int>
    {
        private readonly object mLock = new object();

        private const string SECURE_STRING = "SECURE72_";

        [SerializeField]
        private int data = 0;
        [SerializeField]
        private int dataDistance = 0;
        [SerializeField]
        private string checksum = "";

        [SerializeField]
        private static bool isabuser = false;
        [SerializeField]
        private static int abusecount = 0;

        public CSafeInt(int value = 0)
        {
            dataDistance = CMiscFunc.SafeRandom() % 10000 + 8433;
            data = value + dataDistance;

            checksum = CEncryption.encode(string.Format("{0}{1}", SECURE_STRING, data.ToString()));
        }

        public override string ToString()
        {
            return string.Format("[CSafeInt: {0}]", Number);
        }

        public virtual int Number
        {
            get
            {
                lock (mLock)
                {
                    string checkvalue = CEncryption.decode(checksum);

                    if (checkvalue.Length > 9 && checkvalue.Substring(0, 9) == SECURE_STRING)
                    {
                        checkvalue = checkvalue.Substring(9);
                        int checknumber = int.Parse(checkvalue);

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
                    dataDistance = CMiscFunc.SafeRandom() % 10000 + 4354;
                    data = value + dataDistance;

                    checksum = CEncryption.encode(string.Format("{0}{1}", SECURE_STRING, data.ToString()));
                }
            }
        }
    }
}
