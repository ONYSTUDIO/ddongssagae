using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class CMonoSingleton<T> : MonoBase where T : MonoBase
    {
        // Check to see if we're about to be destroyed.
        private static bool m_ShuttingDown = false;
        private static object m_Lock = new object();
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (m_ShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed. Returning null.");
                    return null;
                }

                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = (T)FindObjectOfType(typeof(T));

                        if (m_Instance == null)
                        {
                            var singletonObject = new GameObject();
                            m_Instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name.ToString()}(Singleton)";
                        }
                        DontDestroyOnLoad(m_Instance);
                    }

                    return m_Instance;
                }
            }
        }

        private void OnApplicationQuit()
        {
            m_ShuttingDown = true;
        }

        protected override void OnDestroy()
        {
            m_ShuttingDown = true;

            base.OnDestroy();
        }
    }
}