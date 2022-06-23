using System;
using UnityEngine;
using UniRx;

namespace DoubleuGames.GameRGD
{
    [Serializable]
    public class CReactiveSafeBool : CSafeBool, IReadOnlyReactiveSafe<bool>
    {
        private readonly Subject<bool> mTrigger = new Subject<bool>();

        public CReactiveSafeBool() : base(false)
        {
        }

        public CReactiveSafeBool(bool initValue) : base(initValue)
        {
        }

        public override bool Number
        {
            set
            {
                base.Number = value;
                mTrigger.OnNext(value);
            }
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            observer.OnNext(Number);
            return mTrigger.Subscribe(observer);
        }
    }

    [Serializable]
    public class CSafeBool : IReadOnlySafe<bool>
    {
        private readonly object mLock = new object();
        private const string SECURE_STRING = "SECURE49_";

        [SerializeField]
        private int data = 0;
        [SerializeField]
        private int dataDistance = 0;
        [SerializeField]
        private string checksum = "";

        private static bool isabuser = false;
        private static int abusecount = 0;

        public CSafeBool(bool value = false)
        {
            dataDistance = CMiscFunc.SafeRandom() % 10000 + 3156;

            if (true == value)
            {
                data = 1;
            }
            else
            {
                data = 0;
            }

            data = data + dataDistance;
            checksum = CEncryption.encode(string.Format("{0}{1}", SECURE_STRING, data.ToString()));
        }

        public override string ToString()
        {
            return string.Format("[CSafeBool: {0}]", Number);
        }

        public virtual bool Number
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
                            CLogger.Log("Number / Abuse detected : CSafeBool");
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

                    int result = data - dataDistance;
                    if (result == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            set
            {
                lock (mLock)
                {
                    dataDistance = CMiscFunc.SafeRandom() % 10000 + 6966;
                    if (value == true)
                    {
                        data = 1;
                    }
                    else
                    {
                        data = 0;
                    }

                    data = data + dataDistance;

                    checksum = CEncryption.encode(string.Format("{0}{1}", SECURE_STRING, data.ToString()));
                }
            }
        }
    }
}
