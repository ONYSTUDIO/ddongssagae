using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;

namespace Helen
{
    public class RenderTexturePreset : MonoBehaviour
    {
        [Serializable]
        public class ObjectInfo
        {
            public AssetReference prefabRef;
            public int collectionIndex;
            public int transformIndex;
        }

        [SerializeField]
        private List<ObjectInfo> objectInfoList = new List<ObjectInfo>();
        public List<ObjectInfo> ObjectInfoList { get => objectInfoList; }
        [SerializeField]
        private List<TransformDataCollection> collectionList;
        public Vector3 CameraDistance = new Vector3(0f, -1.5f, -3f);
        public Vector3 CameraLookAt;
        public Vector3 CameraRotation;
        public float CameraFov = 60f;
        public float CameraNear = 0.3f;
        public float CameraFar = 1000f;
        public bool Orthographic = true;
        public float OrthographicSize = 2f;

        private void Awake()
        {
            collectionList = new List<TransformDataCollection>(GetComponentsInChildren<TransformDataCollection>());
        }

#if UNITY_EDITOR
        public int AddFormationCollection()
        {
            GameObject newRoot = new GameObject();
            newRoot.transform.SetParent(transform, false);
            newRoot.name = "formations" + collectionList.Count;
            TransformDataCollection collection = newRoot.AddComponent<TransformDataCollection>();
            collection.isIgnoreY = false;
            var trans = collection.AddTransform();
            trans.rotation = Quaternion.Euler(0f, 180f, 0f);
            collectionList.Add(collection);
            return collectionList.Count - 1;
        }

        public void AddObjectTransformInfo(UnityEngine.Object editorAsset, int collectionIndex, int transformIndex)
        {
            if (editorAsset == null)
                return;

            TransformDataCollection collection = GetTransformCollection(collectionIndex);
            if (null == collection)
                return;

            var transform = collection.GetTransform(transformIndex);
            if (null == transform)
                return;

            ObjectInfo obj = new ObjectInfo();

            obj.prefabRef = new AssetReference();
            obj.prefabRef.SetEditorAsset(editorAsset);
            obj.collectionIndex = collectionIndex;
            obj.transformIndex = transformIndex;

            objectInfoList.Add(obj);
        }

        public void RemoveObjectTransformInfo(int collectionIndex, int transformIndex)
        {
            var target = objectInfoList.Find(x => x.collectionIndex == collectionIndex && x.transformIndex == transformIndex);
            objectInfoList.Remove(target);
        }
#endif

        public TransformDataCollection GetTransformCollection(int collectionIndex)
        {
            if (collectionList.Count <= collectionIndex)
                return null;
            return collectionList[collectionIndex];
        }
    }
}