#pragma warning disable 0618

using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DoubleuGames.GameRGD
{
    public partial class CMiscFunc
    {
        public static DateTime executeDateTime = System.DateTime.Now;

        public static int m_LastTick = -1;
        public static int m_LastTickOffset = 0;
        private static System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

        public static void ResetExecuteTime()
        {
            executeDateTime = System.DateTime.Now;

            m_LastTick = -1;
            m_LastTickOffset = 0;
        }

        public static string ToOrdinal(long number)
        {
            if (number < 0) return number.ToString();
            long rem = number % 100;
            if (rem >= 11 && rem <= 13) string.Format("{0}th", number);
            switch (number % 10)
            {
                case 1:
                    return string.Format("{0}st", number);
                case 2:
                    return string.Format("{0}nd", number);
                case 3:
                    return string.Format("{0}rd", number);
                default:
                    return string.Format("{0}th", number);
            }
        }

        public static string ToOrdinal(int number)
        {
            if (number < 0) return number.ToString();
            int rem = number % 100;
            if (rem >= 11 && rem <= 13) string.Format("{0}th", number);
            switch (number % 10)
            {
                case 1:
                    return string.Format("{0}st", number);
                case 2:
                    return string.Format("{0}nd", number);
                case 3:
                    return string.Format("{0}rd", number);
                default:
                    return string.Format("{0}th", number);
            }
        }

        // CExtensions 에 ReverseActive 로 대체함
        // public static void ReverseGameObjectActive(GameObject obj)
        // {
        // 	obj.SetActive(!obj.activeSelf);
        // }

        /// <summary>
        /// Converts notification object to socket.
        /// </summary>
        /// <returns>The to sockte.</returns>
        /// <param name="data">Data.</param>
        /// <param name="errorcode">Errorcode.</param>
        public static CSocketBase ConvertToSocket(object data, out int errorcode)
        {
            CNotification notification = (CNotification)data;
            errorcode = int.Parse(notification.Result);
            CSocketBase sock = (CSocketBase)notification.Data;
            return sock;
        }
        /// <summary>
        /// Changes the long to number string.
        /// </summary>
        /// <returns>The long to number string.</returns>
        /// <param name="number">Number.</param>
        public static string ChangeNumberToString(long number)
        {
            return number.ToString("#,##0");
        }
        /// <summary>
        /// Changes the long to number string.
        /// </summary>
        /// <returns>The long to number string.</returns>
        /// <param name="number">Number.</param>
        public static string ChangeNumberToString(int number)
        {
            return number.ToString("#,##0");
        }
        /// <summary>
        /// Changes the long to number string.
        /// </summary>
        /// <returns>The long to number string.</returns>
        /// <param name="number">Number.</param>
        public static string ChangeNumberToString(float number)
        {
            return number.ToString("#,##0.00");
        }

        public static long ChangeStringToLong(string value)
        {
            if (String.IsNullOrEmpty(value)) throw new ArgumentException(nameof(value));
            string _tmp = value.Replace(",", string.Empty).Trim();
            return Int64.Parse(_tmp);
        }

        public static int ChangeStringToInt(string value)
        {
            if (String.IsNullOrEmpty(value)) throw new ArgumentException(nameof(value));
            string _tmp = value.Replace(",", string.Empty).Trim();
            return Int32.Parse(_tmp);
        }

        // Text에 세팅 된 값과 새로 들어온 비교 변화량
        public static long GetChangedValue(string beforeValue, long currentValue)
        {
            if (String.IsNullOrEmpty(beforeValue)) throw new ArgumentException(nameof(beforeValue));
            long _beforeValue = ChangeStringToLong(beforeValue);
            long _changed = currentValue - _beforeValue;
            return _changed;
        }

        // Text에 세팅 된 값과 새로 들어온 값 비교 변화량
        public static int GetChangedValue(string beforeValue, int currentValue)
        {
            if (String.IsNullOrEmpty(beforeValue)) throw new ArgumentException(nameof(beforeValue));
            int _beforeValue = ChangeStringToInt(beforeValue);
            int _changed = currentValue - _beforeValue;
            return _changed;
        }

        // K = 000
        // M = 000,000
        // B = 000,000,000
        // T = 000,000,000,000
        private static long[] KMBCompareValues = { 1000, 1000000, 1000000000, 1000000000000 };
        private static string[] KMBValues = { "K", "M", "B", "T" };
        public static string ChangeNumberToString_KMB(int number)
        {
            return ChangeNumberToString_KMB((long)number);
        }

        public static string ChangeNumberToString_KMB(long number)
        {
            for (int i = KMBCompareValues.Length - 1; i >= 0; --i)
            {
                if (number >= (long)KMBCompareValues[i])
                {
                    number /= (long)KMBCompareValues[i];
                    return $"{number}{KMBValues[i]}";
                }
            }
            return number.ToString();
        }

        // K = 10K ~ 9999K
        // M = 10M ~ 9999M
        // B = 10B ~ 9999B
        // T = 10T ~ 9999T
        public static string ChangeNumberToString_KMB_v2(int number)
        {
            return ChangeNumberToString_KMB_v2((long)number);
        }

        public static string ChangeNumberToString_KMB_v2(long number)
        {
            for (int i = KMBCompareValues.Length - 1; i >= 0; --i)
            {
                if (number >= (long)KMBCompareValues[i] * 10)
                {
                    number /= (long)KMBCompareValues[i];
                    return $"{number}{KMBValues[i]}";
                }
            }
            return number.ToString();
        }

        // K = 100K ~ 999K
        // M = 100M ~ 999M
        // B = 100B ~ 999B
        // T = 100T ~ 999T
        public static string ChangeNumberToString_KMB_v3(int number)
        {
            return ChangeNumberToString_KMB_v3((long)number);
        }

        public static string ChangeNumberToString_KMB_v3(long number)
        {
            for (int i = KMBCompareValues.Length - 1; i >= 0; --i)
            {
                if (number >= (long)KMBCompareValues[i] * 100)
                {
                    number /= (long)KMBCompareValues[i];
                    return $"{number}{KMBValues[i]}";
                }
            }
            return $"{number:#,0}";
        }

        public static string GenerateRequireResources(long current, long require, string color = "#ff6292")
        {
            var enough = current >= require;
            var currentKMB = CMiscFunc.ChangeNumberToString_KMB_v2(current);
            var requireKMB = CMiscFunc.ChangeNumberToString_KMB_v2(require);
            return enough ?
                $"{currentKMB}/{requireKMB}" :
                $"<color={color}>{currentKMB}</color>/{requireKMB}";
        }

        public static int SafeRandom()
        {
            int temp1 = random.Next();
            int temp2 = random.Next();

            int result = random.Next();

            if (temp1 == temp2)
            {
                result = random.Next() + System.DateTime.Now.Millisecond;
            }
            return result % 1000000;
        }

        public static int RandomRange(int min, int max)
        {
            int result = random.Next(min, max);

            return result;
        }

        public static int[] RandomRange(int count, int min, int max)
        {
            int[] results = new int[count];

            for (int i = 0; i < count; i++) results[i] = RandomRange(min, max);

            return results;
        }

        public static IList<T> ShuffleListObject<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count * 10; i++)
            {
                var temp = list[0];
                int rand = SafeRandom() % list.Count;
                list[0] = list[rand];
                list[rand] = temp;
            }
            return list;
        }

        public static void Shuffle<T>(T[] array)
        {
            System.Random rng = new System.Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }

        public static double GetTimeSecond()
        {
            TimeSpan span = new TimeSpan(System.DateTime.Now.Ticks);
            return span.TotalSeconds;
        }

        // millisecond
        public static int SafeGetTimer()
        {
            TimeSpan span = System.DateTime.Now - executeDateTime;
            return (int)span.TotalMilliseconds;
        }

        // second
        public static int SafeGetTimerSecond()
        {
            TimeSpan span = System.DateTime.Now - executeDateTime;
            return (int)span.TotalSeconds;
        }

        public static int SafeGetTimerNetwork()
        {
            int tick = SafeGetTimer();
            int return_tick;

            if (tick == 0)
            {
                tick = 1; // 0은 없다고 가정		
            }

            if (tick <= m_LastTick)
            {
                m_LastTickOffset += m_LastTick; // 리셋 보정 로직
            }

            return_tick = tick + m_LastTickOffset;
            m_LastTick = tick;

            return return_tick;
        }

        public static int MakeSafeInt(string encode_num)
        {
            return int.Parse(encode_num.Replace("ENC_", "").Replace("_ENC", ""));
        }

        string GetFacebookPhotoURL(string userid)
        {
            //			if (CUserInfo.Instance.IsSinglePlay == true && CUserInfo.Instance.UserID.Equals(userid))
            //			{
            //				return "";
            //			}

            return string.Format("http://graph.facebook.com/{0}/picture?type=normal", userid);
        }

        public static string GetFacebookPhotoURL(long userid)
        {
            //			if (CUserInfo.Instance.IsSinglePlay == true && CUserInfo.Instance.UserID.Equals(userid))
            //			{
            //				return "";
            //			}

            return string.Format("http://graph.facebook.com/{0}/picture?type=normal", userid);
        }

        public static string GetFacebookPhotoURL_Small(string userid)
        {
            //			if (CUserInfo.Instance.IsSinglePlay == true && CUserInfo.Instance.UserID.Equals(userid))
            //			{
            //				return "";
            //			}

            return string.Format("http://graph.facebook.com/{0}/picture?type=square", userid);
        }

        public static string GetFacebookPhotoURL_Small(long userid)
        {
            //			if (CUserInfo.Instance.IsSinglePlay == true && CUserInfo.Instance.UserID.Equals(userid))
            //			{
            //				return "";
            //			}

            return string.Format("http://graph.facebook.com/{0}/picture?type=square", userid);
        }

        public static string GetFacebookPhotoURL_Large(string userid)
        {
            //			if (CUserInfo.Instance.IsSinglePlay == true && CUserInfo.Instance.UserID.Equals(userid))
            //			{
            //				return "";
            //			}

            return string.Format("http://graph.facebook.com/{0}/picture?type=large", userid);
        }

        public static string GetFacebookPhotoURL_Large(long userid)
        {
            //			if (CUserInfo.Instance.IsSinglePlay == true && CUserInfo.Instance.UserID.Equals(userid))
            //			{
            //				return "";
            //			}

            return string.Format("http://graph.facebook.com/{0}/picture?type=large", userid);
        }

        public static string ConvertTimeString(int s)
        {
            var _d = (int)(s / 86400);
            var _h = (int)(s % 86400 / 3600);
            var _m = (int)(s % 3600 / 60);
            var _s = (int)(s % 60);

            if (_d > 0) return string.Format($"{_d}D {_h}H");
            else if (_h > 0) return string.Format($"{_h}H {_m}M");
            else if (_m > 0) return string.Format($"{_m}M {_s}S");

            return $"{_s}S";
        }

        public static string SetTimeString(int ss)
        {
            int _hh = (int)(ss / 3600);
            int _mm = (int)(ss % 3600 / 60);
            int _ss = (int)(ss % 60);

            string timeStr = string.Format("{0:D2}:{1:D2}:{2:D2}", _hh, _mm, _ss);
            return timeStr;
        }

        public static string SetTimeMinuteString(int ss)
        {
            int _hh = (int)(ss / 3600);
            int _mm = (int)(ss % 3600 / 60);
            int _ss = (int)(ss % 60);

            string timeStr = string.Format("{0:D2}:{1:D2}", _mm, _ss);
            return timeStr;
        }

        public static string SetSecondToDayHour(int ss)
        {
            int _dd = (int)(ss / 86400);
            int _hh = (int)(ss / 3600) - _dd * 24;

            string timeStr = string.Format("{0:D2}:{1:D2}", _dd, _hh);
            return timeStr;
        }

        public static DateTime FromServerTime(int serverTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(serverTime);
        }

        public static string GetMonthDate(DateTime _time)
        {
            string str = "";
            switch (_time.Month)
            {
                case 1:
                    str = "Jan";
                    break;

                case 2:
                    str = "Feb";
                    break;

                case 3:
                    str = "Mar";
                    break;

                case 4:
                    str = "Apr";
                    break;

                case 5:
                    str = "May";
                    break;

                case 6:
                    str = "Jun";
                    break;

                case 7:
                    str = "Jul";
                    break;

                case 8:
                    str = "Aug";
                    break;

                case 9:
                    str = "Sep";
                    break;

                case 10:
                    str = "Oct";
                    break;

                case 11:
                    str = "Nov";
                    break;

                case 12:
                    str = "Dec";
                    break;
            }

            return str;
        }

        public static string GetConvertCurrency(int value)
        {
            return string.Format("{0:#,0}", value);
        }

        public static string GetConvertCurrency(float value)
        {
            return string.Format("{0:0.##}", value);
        }

        public static string GetConvertCurrency(long value)
        {
            return string.Format("{0:#,0}", value);
        }

        public static void SetLayer(Transform obj, int layer)
        {
            obj.gameObject.layer = layer;
            foreach (Transform child in obj)
            {
                SetLayer(child, layer);
            }
        }

        public static void SetLayer(Transform obj, int layer, int originalLayer)
        {
            if (obj.gameObject.layer == originalLayer)
            {
                obj.gameObject.layer = layer;
            }

            foreach (Transform child in obj)
            {
                SetLayer(child, layer, originalLayer);
            }
        }

        public static string GetRankText(int rank)
        {
            if (rank > 3 && rank < 21)
            {
                return string.Format("{0}th", rank);
            }
            else
            {
                int value = rank % 10;

                if (value == 1)
                {
                    return string.Format("{0}st", rank);
                }
                else if (value == 2)
                {
                    return string.Format("{0}nd", rank);
                }
                else if (value == 3)
                {
                    return string.Format("{0}rd", rank);
                }
                else
                {
                    return string.Format("{0}th", rank);
                }
            }
        }

        public static string GetRankText(long rank)
        {
            if (rank > 3 && rank < 21)
            {
                return string.Format("{0}th", rank);
            }
            else
            {
                long value = rank % 10;

                if (value == 1)
                {
                    return string.Format("{0}st", rank);
                }
                else if (value == 2)
                {
                    return string.Format("{0}nd", rank);
                }
                else if (value == 3)
                {
                    return string.Format("{0}rd", rank);
                }
                else
                {
                    return string.Format("{0}th", rank);
                }
            }
        }

        private static string GetProperDisplayNameForce(string firstname, string lastname, string nickname, int maxLength)
        {
            int firstLength = 0;
            if (firstname.Length < (maxLength - 2))
            {
                firstLength = firstname.Length;
            }
            else
            {
                firstLength = maxLength - 2;
            }

            return string.Format("{0}.{1}", firstname.Substring(0, firstLength), lastname.Substring(0, 1).ToUpper());
        }

        public static string GetProperDisplayNameByNickname(string nickname, int maxLength = 11)
        {
            if (string.IsNullOrEmpty(nickname))
            {
                return "";
            }

            if (nickname.Length < maxLength)
            {
                maxLength = nickname.Length;
            }

            string result = "";
            string[] nameArray = nickname.Split(' ');

            if (nameArray.Length > 1)
            {
                result = GetProperDisplayNameForce(nameArray[0], nameArray[nameArray.Length - 1], nickname, maxLength);
            }
            else
            {
                string[] nameArray2 = nickname.Split('.');
                string first_tmp = "";

                if (nameArray2.Length > 1)
                {
                    first_tmp = nameArray2[0].ToLower();
                    if (first_tmp.Length > 1)
                    {
                        string middleName = "";
                        if (first_tmp.Length > (maxLength - 3))
                        {
                            middleName = first_tmp.Substring(1, (maxLength - 3));
                        }
                        else
                        {
                            middleName = first_tmp.Substring(1, (first_tmp.Length - 1));
                        }

                        result = string.Format("{0}{1}.{2}", first_tmp.Substring(0, 1).ToUpper(), middleName, nameArray2[1].Substring(0, 1).ToUpper());
                    }
                    else
                    {
                        result = nickname.Substring(0, maxLength);
                    }
                }
                else
                {
                    result = nickname.Substring(0, maxLength);
                }
            }

            return result;
        }

        /// <summary>
        /// 카메라의 비율. 16:10 => 1.6
        /// </summary>
        /// <returns>The screen aspect.</returns>
        /// <param name="camera">Camera.</param>
        public static float GetScreenAspect(Camera camera)
        {
            Vector3 v = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));

            return v.x;
        }

        /// <summary>
        /// 카메라 코너의 트랜스폼 XY 포지션.
        /// </summary>
        /// <returns>The camera corner.</returns>
        /// <param name="camera">Camera.</param>
        public static Vector2 GetCameraCorner(Camera camera)
        {
            //			Vector3 v = camera.ViewportToWorldPoint(new Vector3(1,1,camera.nearClipPlane));
            float screenAspect = GetScreenAspect(camera);
            float standardAspect = 1.6f;

            //1.6 : 1280 = v.x : result.x
            Vector2 size = new Vector2(640, 400);

            // 5 : 6 = 400 : 480
            // 1.3 : 1280  = 1.0 : result.y
            if (screenAspect > standardAspect)
            {
                size.x = size.x * screenAspect * 0.625f;	// x / 1.6 = x * 0.625
            }
            else if (screenAspect < standardAspect)
            {
                size.y = size.x / screenAspect;
            }
            //너비는 가로 비율이 1.6보다 작거나 같으면 640 고정.
            //높이는 가로 비율이 1.6보다 크거나 같으면 400 고정.
            return size;

        }

        public static List<int> StrToListInt(string str, int offset = 0)
        {
            List<int> _result = new List<int>();
            foreach (string _itemStr in str.Split(','))
            {
                _result.Add(int.Parse(_itemStr) + offset);
            }
            return _result;
        }



        public static byte[] StrToByteArray(string readString)
        {
            string[] stringArray = readString.Split(',');
            byte[] intArray = new byte[stringArray.Length];

            for (int i = 0; i < stringArray.Length; i++)
            {
                intArray[i] = byte.Parse(stringArray[i]);
            }

            return intArray;
        }

        public static int[] StrToArrayInt(string readString)
        {
            string[] stringArray = readString.Split(',');
            int[] intArray = new int[stringArray.Length];

            for (int i = 0; i < stringArray.Length; i++)
            {
                intArray[i] = int.Parse(stringArray[i]);
            }

            return intArray;
        }

        public static long GetDirectorySize(string _path)
        {
            long _size = 0;
            DirectoryInfo _dirInfo = new DirectoryInfo(_path);

            if (_dirInfo.Exists == false)
            {
                return _size;
            }

            foreach (DirectoryInfo _directoryInfo in _dirInfo.GetDirectories())
            {
                _size += GetDirectorySize(_directoryInfo.FullName);
            }

            //C:\abc 폴더의 용량을 검사
            foreach (FileInfo _fileInfo in _dirInfo.GetFiles())
            {
                _size += (long)_fileInfo.Length;
            }

            return _size;
        }

        public static long GetFileSize(string _path)
        {
            long _size = 0;
            FileInfo _fileInfo = new FileInfo(_path);

            if (_fileInfo.Exists == false)
            {
                return _size;
            }

            _size = _fileInfo.Length;

            return _size;
        }

        public static void RemoveDirecotry(string _path)
        {
            DirectoryInfo _dirInfo = new DirectoryInfo(_path);

            if (_dirInfo.Exists == false)
            {
                return;
            }

            Directory.Delete(_path, true);
        }

        /// <summary>
        /// Removes the sub direcotry And File.
        /// </summary>
        /// <param name="_path">_path.</param>
        /// <param name="excludePattern">Exclude pattern. | 로 구분, File은 확장자까지, 폴더는 이름만.</param>
        public static void RemoveSubDirecotry(string _path, string excludePattern = "")
        {
            DirectoryInfo _dirInfo = new DirectoryInfo(_path);

            if (_dirInfo.Exists == false)
            {
                return;
            }
            List<string> patterns = new List<string>(excludePattern.Split('|'));

            foreach (DirectoryInfo _subDirectoryInfo in _dirInfo.GetDirectories())
            {
                if (patterns.Contains(_subDirectoryInfo.Name) == false)
                {
                    Directory.Delete(_subDirectoryInfo.FullName, true);
                }
            }

            foreach (FileInfo _fileInfo in _dirInfo.GetFiles())
            {
                if (!patterns.Contains(_fileInfo.Name))
                {
                    File.Delete(_fileInfo.FullName);
                }
            }
        }

        public static void RemoveFile(string _path)
        {
            FileInfo _fileInfo = new FileInfo(_path);

            if (_fileInfo.Exists == false)
            {
                return;
            }

            File.Delete(_path);
        }

        public static bool BetweenInt(int _target, int _min, int _max, bool _includeMin = true, bool _includeMax = true)
        {
            _min = _includeMin ? _min : _min + 1;
            _max = _includeMax ? _max : _max - 1;
            return _target >= _min && _target <= _max;
        }

        public static void BeginDownload(string _savePath, string _url, Action<int> percentageAction)
        {
            long _tmpFileSize;
            long _startPoint;
            const int LENGTH = 1024 * 1024;
            const int OFFSET = 0;
            int _rcvd = 0;
            int newPercentage = 0;
            FileInfo _gFI = new FileInfo(_savePath);

            if (!_gFI.Exists) // 파일이 없을 경우
            {
                CLogger.Log("BeginDownload() / New Download.");
                Stream st = File.Create(_savePath); //일단 파일 생성해 놓쿠.
                st.Close();
                _startPoint = 0; // 새로 만들었으니 스타트 포인트는 0으로.
                _tmpFileSize = 0;
            }
            else // 파일이 있을경우 이어받기를 위해.
            {
                _tmpFileSize = _gFI.Length; // 받은 파일 싸이즈 구해서	.
                _startPoint = _tmpFileSize; // 받을 파일 싸이즈 다음(+1)으로 스타트 포인트 설정.
                CLogger.Log("BeginDownload() / Continue Download. / FileSIze {0} / STartPoint {1}", _tmpFileSize, _startPoint);
            }

            HttpWebResponse _response = null;
            Stream _responseStream = null;
            FileStream _fileStream = null;

            try
            {
                HttpWebRequest tempReq = (HttpWebRequest)WebRequest.Create(_url); // Request 할 경로 설정.
                HttpWebResponse temp = (HttpWebResponse)tempReq.GetResponse(); // Request보내고.

                long totalSize = temp.ContentLength;
                temp.Close();

                if (_tmpFileSize == totalSize)
                {
                    CLogger.Log("BeginDownload() / Don't need Download. so return / _tmpFileSize {0} / totalSize {1}", _tmpFileSize, totalSize);
                    newPercentage = 100;
                    percentageAction.Invoke(newPercentage);
                    return;
                }

                HttpWebRequest _req = (HttpWebRequest)WebRequest.Create(_url); // Request 할 경로 설정.

                _req.AddRange((int)_startPoint); // 서버에서 날려줄 파일의 시작지점 설정.

                _response = (HttpWebResponse)_req.GetResponse(); // Request보내고.

                CLogger.Log("BeginDownload() / GetResponse Complete {0}", _response.ContentLength);

                long _totalSize = _response.ContentLength + _tmpFileSize;

                _responseStream = _response.GetResponseStream(); // Response 받아서 스트림에.
                _fileStream = new FileStream(_savePath, FileMode.Open); // 저장될 파일 스트림으로.

                _fileStream.Seek(_startPoint, SeekOrigin.Begin); // 저장될 파일의 시작지점 설정.
                while (_fileStream.Length <= _totalSize)
                {
                    _rcvd = 0;
                    byte[] buff = new byte[LENGTH];
                    _rcvd = _responseStream.Read(buff, OFFSET, LENGTH); // 날라온거 읽어서.

                    if (_rcvd > 0) //날라온게 있으면.
                    {
                        _fileStream.Write(buff, 0, _rcvd); // 파일에 쓰고.

                        newPercentage = (int)((float)_fileStream.Length / (float)_totalSize * 100);
                        percentageAction.Invoke(newPercentage);
                    }
                    else
                    {
                        CLogger.Log("BeginDownload() / Wait for network");
                    }

                    if (_fileStream.Length == totalSize)
                    {
                        break;
                    }
                }

                newPercentage = 100;
                percentageAction.Invoke(newPercentage);
                CLogger.Log("BeginDownload() / End Download. file Size : {0}", _fileStream.Length);
            }
            catch (Exception e)
            {
                CLogger.LogError("BeginDownload() / message : {0}", e.Message);
            }
            finally
            {
                if (_fileStream != null)
                {
                    _fileStream.Close(); // 없으면 파일 닫고.
                    _fileStream = null;
                }

                if (_responseStream != null)
                {
                    _responseStream.Close();
                    _responseStream = null;
                }

                if (_response != null)
                {
                    _response.Close();
                    _response = null;
                }
            }
        }

        public static Color ColorParser(string colorString)
        {
            float[] colorValues = new float[3];
            int i = 0;
            foreach (string colorValueString in colorString.Split(','))
            {
                colorValues[i] = (float.Parse(colorValueString)) / 255f;
                i++;
            }
            return new Color(colorValues[0], colorValues[1], colorValues[2]);
        }

        public static string ColorToBBCCodeColorString(Color color)
        {
            string result = string.Empty;

            int r = (int)(color.r * 255);
            int g = (int)(color.g * 255);
            int b = (int)(color.b * 255);
            result = string.Format("[{0:X2}{1:X2}{2:X2}]", r, g, b);
            return result;
        }

        // public static void MultipleWidgetSize(UIWidget widget, float value)
        // {
        // 	widget.width = (int)(widget.width * value);
        // 	widget.height = (int)(widget.height * value);
        // }

        public static void ChangeChildWidgetGray(GameObject parent, bool isGray)
        {
            //ChangeChildSpirteGray(parent,isGray);
        }

        // public static void ChangeChildLabelGray(GameObject parent, bool isGray)
        // {
        // 	UILabel[] labels = parent.GetComponentsInChildren<UILabel>();

        // 	for (int i = 0;i < labels.Length;i++)
        // 	{
        // 		labels[i].color = Color.gray;
        // 	}
        // }

        // public static void ChangeChildLabelColor(GameObject parent, bool isGray)
        // {
        // 	UILabel[] labels = parent.GetComponentsInChildren<UILabel>();
        // 	string labelColorPrefixString = "[c][5e5e5e]"; //gray
        // 	string labelColorPostfixString = "[/c]";

        // 	for (int i = 0;i < labels.Length;i++)
        // 	{
        // 		if (isGray)
        // 		{
        // 			if (!labels[i].text.Contains(labelColorPrefixString))
        // 				labels[i].text = string.Format("{0}{1}{2}",labelColorPrefixString,labels[i].text,labelColorPostfixString);
        // 		}
        // 		else
        // 		{
        // 			labels[i].text = labels[i].text.Replace(labelColorPrefixString,string.Empty);
        // 			labels[i].text = labels[i].text.Replace(labelColorPostfixString,string.Empty);

        // 		}
        // 	}
        // }

        public static string IEnumerableToString(IEnumerable enumerable, char seperator = ',')
        {
            string result = string.Empty;
            foreach (var item in enumerable)
            {
                if (result.Equals(string.Empty))
                {
                    result = item.ToString();
                }
                else
                {
                    result = string.Format("{0}{1}{2}", result, seperator, item.ToString());
                }
            }
            return result;
        }

        public static void LogList(IEnumerable enumerable)
        {
            string result = string.Empty;
            foreach (var item in enumerable)
            {
                result = string.Format("{0}, {1}", result, item);
            }
            Debug.LogError(result);
        }

        public static string GetMD5String(string data)
        {
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(data);

            // encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);

            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }

        public static int GetRandomIntUnder(int n)
        {
            return UnityEngine.Random.Range(0, n);
        }

        public static string GetRemainTimeFormat(long TimeValue, bool isTrim = false, bool isSmartCheck = false, int strBaseHour = 24)
        {
            int hour = (int)TimeValue / (60 * 60);
            int min = (int)TimeValue % 3600 / 60;
            int sec = (int)TimeValue % 60;

            if (hour > strBaseHour)
            {
                int days = (int)(hour / 24);
                string ret = " Days";
                if (days == 1)
                {
                    ret = " Day";
                }
                return days + ret;
            }
            else if (isTrim && hour == 0)
            {
                return string.Format("{0:D2}:{1:D2}", min, sec);
            }
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
        }

        public static bool CheckInSymbolList(int id, List<CSafeInt> symbolList)
        {
            for (int i = 0; i < symbolList.Count; i++)
            {
                if (id == symbolList[i].Number)
                {
                    return true;
                }
            }
            return false;
        }


        public static int? CheckInSymbolList(int id, List<int> symbolList)
        {
            for (int i = 0; i < symbolList.Count; i++)
            {
                if (id == symbolList[i])
                {
                    return i;
                }
            }
            return null;
        }

        public static string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }

        public static async UniTask<Texture> LoadImageTextureAsync(string url)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            var oper = www.SendWebRequest();
            await UniTask.WaitUntil(() => oper.isDone);

            if (www.isNetworkError || www.isHttpError)
            {
                CLogger.LogWarning($"{www.error}: {url}");
                return null;
            }
            return ((DownloadHandlerTexture)www.downloadHandler).texture;
        }

        public static bool IsNullableType<T>(T value)
        {
            var _type = typeof(T);
            return Nullable.GetUnderlyingType(_type) != null;
        }

        public static string GetAppVersion()
        {
            return Application.version;
        }

        public static string ClipboardValue
        {
            get => GUIUtility.systemCopyBuffer;
            set => GUIUtility.systemCopyBuffer = value;
        }
    }
}