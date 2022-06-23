using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Helen
{
    internal class InstanceFieldInjector
    {
        private class InjectFieldInfo
        {
            public static InjectFieldInfo Create(FieldInfo fieldInfo)
            {
                InjectAttribute injectAttribute = fieldInfo.GetCustomAttribute<InjectAttribute>(false);
                if (injectAttribute != null)
                    return new InjectFieldInfo(fieldInfo, injectAttribute);
                return null;
            }

            public Type Type { get { return fieldInfo.FieldType; } }
            public object Key { get; private set; }

            private readonly FieldInfo fieldInfo;

            protected InjectFieldInfo(FieldInfo fieldInfo, InjectAttribute injectAttribute)
            {
                this.fieldInfo = fieldInfo;

                if (injectAttribute != null)
                    Key = injectAttribute.Key;
            }

            public void SetValue(object instance, object value)
            {
                fieldInfo.SetValue(instance, value);
            }
        }

        private class InjectTypeInfo
        {
            public static InjectTypeInfo Create(Type type)
            {
                return new InjectTypeInfo(type);
            }

            public Type Type { get; private set; }
            public Type BaseType { get; private set; }

            protected InjectTypeInfo(Type type)
            {
                Type = type;
                if (type.BaseType != typeof(MonoBehaviour))
                    BaseType = type.BaseType;

                injectFieldInfos = type
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Select(fi => InjectFieldInfo.Create(fi))
                    .Where(fi => fi != null)
                    .ToArray();
            }

            private readonly InjectFieldInfo[] injectFieldInfos;

            public InjectFieldInfo[] GetFieldInfos()
            {
                return injectFieldInfos;
            }
        }

        public bool Inject(object instance, IContainer container)
        {
            int injectCount = 0;

            InjectTypeInfo typeInfo = GetTypeInfo(instance.GetType().UnderlyingSystemType);
            while (typeInfo != null)
            {
                var fieldInfos = typeInfo.GetFieldInfos();
                for (int i = 0; i < fieldInfos.Length; ++i)
                {
                    InjectFieldInfo fieldInfo = fieldInfos[i];
                    object value = container.Resolve(fieldInfo.Type, new ServiceKey(fieldInfo.Key));
                    fieldInfo.SetValue(instance, value);
                    injectCount += 1;
                }
                typeInfo = GetTypeInfo(typeInfo.BaseType);
            }

            return injectCount > 0;
        }

        private readonly Dictionary<Type, InjectTypeInfo> typeInfos =
            new Dictionary<Type, InjectTypeInfo>();

        private InjectTypeInfo GetTypeInfo(Type type)
        {
            if (type == null)
                return null;

            if (!typeInfos.TryGetValue(type, out InjectTypeInfo typeInfo))
                typeInfos.Add(type, typeInfo = InjectTypeInfo.Create(type));
            return typeInfo;
        }
    }
}