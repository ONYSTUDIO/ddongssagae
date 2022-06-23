using System;
using UnityEngine;
using NiceJson;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helen
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AssetPathAttribute : PropertyAttribute
    {
        public string prefix = string.Empty;
        public string postfix = string.Empty;

        public AssetPathAttribute()
        {

        }
        public string GetAssetPathFormat()
        {
            return string.Format("{0}{{0}}{1}", prefix, postfix);
        }

        public string GetAssetPath(string path)
        {
            return string.Format("{0}{1}{2}", prefix, path, postfix);
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AssetPathJsonAttribute : AssetPathAttribute
    {
        public readonly Type jsonSerializationType;
        public AssetPathJsonAttribute(Type t)
        {
            jsonSerializationType = t;
        }
        public Array GetJsonSerialize(string path)
        {
#if UNITY_EDITOR
            TextAsset table = AssetDatabase.LoadAssetAtPath<TextAsset>(GetAssetPath(path));
            if (table == null)
            {
                Log.Error("table load failed : " + path);
                return null;
            }

            return JsonHelper.GetJsonArray(table.text, jsonSerializationType);
#else
        return null;
#endif
        }
    }
}