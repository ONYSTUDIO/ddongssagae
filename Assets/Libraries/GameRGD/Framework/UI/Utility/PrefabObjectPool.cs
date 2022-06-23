using System.Collections.Generic;
using UnityEngine;

namespace Helen
{
    public class PrefabObjectPool : MonoBehaviour
    {
        public string prefabName { get { return prefab != null ? prefab.name : string.Empty; } }

        public GameObject prefab;

        private GameObject m_inactiveObjectNode
        {
            get
            {
                if (s_InactivePoolObjects == null)
                {
                    s_InactivePoolObjects = new GameObject("InactivePoolObjects");
                    s_InactivePoolObjects.SetActive(false);

                    DontDestroyOnLoad(s_InactivePoolObjects);
                }

                if (m_InternalInactiveObjectNode == null)
                {
                    m_InternalInactiveObjectNode = new GameObject(prefabName);
                    m_InternalInactiveObjectNode.transform.SetParent(s_InactivePoolObjects.transform, false);
                    m_InternalInactiveObjectNode.SetActive(false);
                }
                return m_InternalInactiveObjectNode;
            }
        }
        private GameObject m_InternalInactiveObjectNode;

        private Queue<GameObject> m_Objects
        {
            get { return CreatePool(prefab); }
        }

        private static GameObject s_InactivePoolObjects;
        private static Dictionary<GameObject, Queue<GameObject>> s_Objects =
            new Dictionary<GameObject, Queue<GameObject>>();

        public static void DestroyImmediate()
        {
            DestroyImmediate(s_InactivePoolObjects);
            s_InactivePoolObjects = null;
        }

        private void OnDestroy()
        {
            Terminate();
        }

        public void Initialize()
        {
            Terminate();
        }

        private static Queue<GameObject> CreatePool(GameObject prefab)
        {
            Queue<GameObject> objects = null;
            if (prefab != null && !s_Objects.TryGetValue(prefab, out objects))
            {
                objects = new Queue<GameObject>();
                s_Objects.Add(prefab, objects);
            }
            return objects;
        }

        private static void DestroyPool(GameObject prefab)
        {
            Queue<GameObject> objects = null;
            if (prefab != null && s_Objects.TryGetValue(prefab, out objects))
                s_Objects.Remove(prefab);

            if (objects != null)
            {
                foreach (GameObject go in objects)
                {
                    if (go != null)
                        Destroy(go);
                }
                objects.Clear();
            }
        }

        public void Terminate()
        {
            DestroyPool(prefab);

            if (m_InternalInactiveObjectNode != null)
            {
                Destroy(m_InternalInactiveObjectNode);
                m_InternalInactiveObjectNode = null;
            }
        }

        public GameObject Dequeue(Transform parent = null, bool worldPositionStays = false)
        {
            Queue<GameObject> objects = m_Objects;
            if (objects == null)
                return Instantiate();

            GameObject go = null;
            if (objects.Count == 0)
                go = Instantiate();
            else
                go = objects.Dequeue();

            if (go != null)
            {
                go.transform.SetParent(parent, worldPositionStays);
                go.transform.localPosition = prefab.transform.localPosition;
                go.transform.localRotation = prefab.transform.localRotation;
                go.transform.localScale = prefab.transform.localScale;
                go.SetActive(true);
            }
            return go;
        }

        public void Enqueue(GameObject go)
        {
            Queue<GameObject> objects = m_Objects;
            if (objects == null)
            {
                Destroy(go);
                return;
            }

            if (go != null)
            {
                go.SetActive(false);
                go.transform.SetParent(m_inactiveObjectNode.transform, false);
                go.transform.localPosition = prefab.transform.localPosition;
                go.transform.localRotation = prefab.transform.localRotation;
                go.transform.localScale = prefab.transform.localScale;
            }
            objects.Enqueue(go);
        }

        private GameObject Instantiate()
        {
            return Instantiate(prefab);
        }
    }
}
