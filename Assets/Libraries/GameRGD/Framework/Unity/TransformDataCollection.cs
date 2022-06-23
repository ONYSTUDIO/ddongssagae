using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helen
{
    // 자식으로 붙어 있는 게임오브젝트를 수집합니다
    [ExecuteInEditMode]
    public class TransformDataCollection : MonoBehaviour
    {
        public List<Transform> Transforms { get; private set; } = new List<Transform>();
#if UNITY_EDITOR
        public bool isIgnoreY = true;
#endif

        private void Awake()
        {
            CollectChild();
        }

        public void CollectChild()
        {
            Transforms.Clear();
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                Transforms.Add(child);
            }
        }

#if UNITY_EDITOR
        public Transform GetTransform(int index)
        {
            if (Transforms.Count <= index)
                return null;
            return Transforms[index];
        }

        public Transform AddTransform()
        {
            GameObject newChild = new GameObject("child" + transform.childCount);
            newChild.transform.parent = transform;
            CollectChild();
            return newChild.transform;
        }

        public void RemoveAt(int index)
        {
            if (Transforms.Count <= index)
                return;
            
            var target = Transforms[index];
            Transforms.RemoveAt(index);
            Destroy(target.gameObject);
        }

        private void OnDrawGizmos()
        {
            CollectChild(); 

            for (int i = 0; i < Transforms.Count; i++)
            {
                if (true == isIgnoreY && !(Transforms[i] is RectTransform))
                {
                    Vector3 pos = Transforms[i].position;
                    pos.y = 0f;
                    Transforms[i].position = pos;
                }

                Color color = Color.green;
                using (new Handles.DrawingScope(color))
                {
                    Handles.SphereHandleCap(i, Transforms[i].position, Transforms[i].rotation, 0.5f, EventType.Repaint);
                }

                color = Color.cyan;
                using (new Handles.DrawingScope(color))
                {
                    Handles.ArrowHandleCap(0, Transforms[i].position, Transforms[i].rotation, 1f, EventType.Repaint);
                }
            }
        }
#endif
    }
}
