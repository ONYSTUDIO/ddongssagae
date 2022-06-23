using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    public static partial class CExtension
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }

        public static T CopyComponent<T>(this Component original, GameObject target) where T : Component
        {
            var _type = original.GetType();
            var _copy = target.AddComponent(_type);
            var _fields = _type.GetFields();

            foreach (var _field in _fields)
            {
                _field.SetValue(_copy, _field.GetValue(original));
            }
            return _copy as T;
        }

        public static void InitTransform(this Component component)
        {
            component.gameObject.InitTransform();
        }

        public static void InitTransform(this GameObject gameObject)
        {
            var transform = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static void InitPosition(this RectTransform rectTransform)
        {
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void SetActiveByActiveSelf(this GameObject gameObject, bool active)
        {
            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        public static void SetActiveByActiveSelf(this Component target, bool active)
        {
            if (target != null)
            {
                target.gameObject?.SetActiveByActiveSelf(active);
            }
        }

        public static void SetActiveReverse(this GameObject gameObject)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public static void ForAll<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (T item in sequence)
            {
                action(item);
            }
        }

        public static void SetParentSame(this GameObject _gameobject, GameObject target)
        {
            if (target.transform.parent != _gameobject.transform.parent)
            {
                _gameobject.transform.SetParent(target.transform.parent);
            }
        }

        public static GameObject AddChild(this GameObject _parent, GameObject _child)
        {
            _child.transform.SetParent(_parent.transform);
            return _child;
        }

        public static void ClearChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public static GameObject GetChildGameObjectByName(this Transform transform, string name)
        {
            return transform.GetComponentsInChildren<Transform>(true)
                .Where(t => t.gameObject.name == name)
                .Select(t => t.gameObject)
                .FirstOrDefault();
        }

        public static Transform GetChildTransformByName(this Transform transform, string name)
        {
            return transform.GetComponentsInChildren<Transform>(true)
                .Where(t => t.gameObject.name == name)
                .FirstOrDefault();
        }

        public static Vector3 GetForwardPosition(this Transform transform, float distance)
        {
            return transform.position + transform.rotation * (Vector3.forward * distance);
        }

        public static void ConnectWithPanel(this Toggle toggle, GameObject panel)
        {
            toggle.OnValueChangedAsObservable().Subscribe(isOn =>
            {
                if (panel != null) panel.SetActiveByActiveSelf(isOn);
            }).AddTo(toggle);
        }

        public static T To<T>(this System.Object source)
        {
            var _to = (T)Activator.CreateInstance(typeof(T));
            var _sourceFields = source.GetType().GetFields().ToList();
            var _toFields = typeof(T).GetFields().ToList();

            foreach (var _sourceField in _sourceFields)
            {
                if (_toFields.Any(x => x.Name == _sourceField.Name))
                {
                    var _packetField = _toFields.FirstOrDefault(x => x.Name == _sourceField.Name);
                    if (_packetField != null)
                    {
                        _packetField.SetValue(_to, _sourceField.GetValue(source));
                    }
                }
            }

            return _to;
        }

        public static T TryGetRamdom<T>(this List<T> list)
        {
            if (list.Count() == 0) return default;
            return list[CMiscFunc.SafeRandom() % list.Count()];
        }

        public static string ReportAllProperties<T>(this T instance, bool singleLine = false) where T : class
        {
            if (instance == null)
                return string.Empty;

            var strListType = typeof(List<string>);
            var strArrType = typeof(string[]);

            var arrayTypes = new[] { strListType, strArrType };
            var handledTypes = new[]
            {
                typeof(Int64), typeof(Int32), typeof(String), typeof(bool), typeof(DateTime), typeof(double), typeof(float), typeof(decimal),
                typeof(CSafeNumber), typeof(CSafeInt), typeof(CSafeBool),
                strListType, strArrType
            };

            var validFields = instance.GetType()
                                          .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                          .Where(f => handledTypes.Contains(f.FieldType) || f.FieldType.IsEnum)
                                          .Where(f => f.GetValue(instance) != null)
                                          .ToList();

            if (validFields.Count == 0)
                return string.Empty;

            var format = singleLine ? $"{{0}}: {{1}}" : $"{{0, -{validFields.Max(f => f.Name.Length)}}}: {{1}}";
            return string.Join(
                     singleLine ? ", " : Environment.NewLine,
                     validFields.Select(f =>
                     {
                         var val = (arrayTypes.Contains(f.FieldType) ? string.Join(", ", (IEnumerable<string>)f.GetValue(instance)) : f.GetValue(instance));
                         return string.Format(format, f.Name, val);
                     }));
        }

        public static void CopyFrom<T>(this T instance, T source) where T : class
        {
            var strListType = typeof(List<string>);
            var strArrType = typeof(string[]);

            var arrayTypes = new[] { strListType, strArrType };
            var handledTypes = new[]
            {
                typeof(Int64), typeof(Int32), typeof(String), typeof(bool), typeof(DateTime), typeof(double), typeof(decimal),
                typeof(CSafeNumber), typeof(CSafeInt), typeof(CSafeBool),
                strListType, strArrType
            };

            var validFields = instance.GetType()
                                          .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                          .Where(f => handledTypes.Contains(f.FieldType))
                                          .Where(f => f.GetValue(instance) != null)
                                          .ToList();

            foreach (var field in validFields)
            {
                if (field.FieldType == typeof(CSafeNumber))
                    (field.GetValue(instance) as CSafeNumber).Number = (field.GetValue(source) as CSafeNumber).Number;
                else if (field.FieldType == typeof(CSafeInt))
                    (field.GetValue(instance) as CSafeInt).Number = (field.GetValue(source) as CSafeInt).Number;
                else if (field.FieldType == typeof(CSafeBool))
                    (field.GetValue(instance) as CSafeBool).Number = (field.GetValue(source) as CSafeBool).Number;
                else
                    field.SetValue(instance, field.GetValue(source));
            }
        }

        public static T Populate<T>(this object source)
        {
            var _packet = (T)Activator.CreateInstance(typeof(T));
            var _dataFields = source.GetType().GetFields();
            var _packetFields = typeof(T).GetFields();

            foreach (var _dataField in _dataFields)
            {
                if (_packetFields.Any(x => x.Name == _dataField.Name))
                {
                    var _packetField = _packetFields.FirstOrDefault(x => x.Name == _dataField.Name);
                    if (_packetField != null)
                    {
                        _packetField.SetValue(_packet, _dataField.GetValue(source));
                    }
                }
            }

            return _packet;
        }

        public static string ToPascalCase(this string word)
        {
            return string.Join("", word.Split('_')
                         .Select(w => w.Trim())
                         .Where(w => w.Length > 0)
                         .Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1).ToLower()));
        }

        public static string GetFullPath(this Transform trn)
        {
            string path = "/" + trn.name;
            while (trn.transform.parent != null)
            {
                trn = trn.parent;
                path = "/" + trn.name + path;
            }
            return path;
        }

        public static T Clone<T>(this T src)
        {
            if (src is ICloneable)
            {
                return (T)(src as ICloneable).Clone();
            }

            using (var stream = new MemoryStream())
            {
                var dataContractSerializer = new DataContractSerializer(typeof(T));

                dataContractSerializer.WriteObject(stream, src);
                stream.Position = 0;
                return (T)dataContractSerializer.ReadObject(stream);
            }
        }

        public static string ToData(this Vector3 v)
        {
            CLogger.Log($"{v}");
            return $"{(Mathf.Abs(v.x) < 0.001f ? 0f : v.x)}|{(Mathf.Abs(v.y) < 0.001f ? 0f : v.y)}|{(Mathf.Abs(v.z) < 0.001f ? 0f : v.z)}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool hasState(this Animator anim, string stateName, int layer = 0)
        {
            int stateID = Animator.StringToHash(stateName);
            return anim.HasState(layer, stateID);
        }
    }
}
