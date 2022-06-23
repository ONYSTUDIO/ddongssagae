using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Helen
{
    internal class InstanceMethodInjector
    {
        private class InjectParameterInfo
        {
            public static InjectParameterInfo Create(ParameterInfo parameterInfo)
            {
                InjectAttribute injectAttribute =
                    parameterInfo.GetCustomAttribute<InjectAttribute>(false);
                return new InjectParameterInfo(parameterInfo, injectAttribute);
            }

            public Type Type { get { return parameterInfo.ParameterType; } }
            public object DefaultValue
            {
                get
                {
                    if (parameterInfo.HasDefaultValue)
                        return parameterInfo.DefaultValue;

                    if (parameterInfo.ParameterType.IsValueType)
                        return Activator.CreateInstance(parameterInfo.ParameterType);
                    return null;
                }
            }

            public object Key { get; private set; }

            private readonly ParameterInfo parameterInfo;

            protected InjectParameterInfo(ParameterInfo parameterInfo, InjectAttribute injectAttribute)
            {
                this.parameterInfo = parameterInfo;

                if (injectAttribute != null)
                    Key = injectAttribute.Key;
            }
        }

        private class InjectMethodInfo
        {
            public static InjectMethodInfo Create(MethodInfo methodInfo)
            {
                InjectAttribute injectAttribute = methodInfo.GetCustomAttribute<InjectAttribute>(false);
                if (injectAttribute != null)
                    return new InjectMethodInfo(methodInfo);
                return null;
            }

            private readonly MethodInfo methodInfo;

            protected InjectMethodInfo(MethodInfo methodInfo)
            {
                this.methodInfo = methodInfo;

                injectParameterInfos = methodInfo
                        .GetParameters()
                        .Select(pi => InjectParameterInfo.Create(pi))
                        .Where(pi => pi != null)
                        .ToArray();
            }

            private readonly InjectParameterInfo[] injectParameterInfos;

            public InjectParameterInfo[] GetParameters()
            {
                return injectParameterInfos;
            }

            public void Invoke(object obj, object[] parameters)
            {
                methodInfo.Invoke(obj, parameters);
            }
        }

        private class InjectTypeInfo
        {
            public static InjectTypeInfo Create(Type type)
            {
                InjectTypeInfo typeInfo = new InjectTypeInfo(type);
                if (typeInfo.InjectMethodInfo != null)
                    return typeInfo;
                return null;
            }

            public Type Type { get; private set; }

            public Type BaseType { get; private set; }

            protected InjectTypeInfo(Type type)
            {
                Type = type;
                if (type.BaseType != typeof(MonoBehaviour))
                    BaseType = type.BaseType;

                InjectMethodInfo = type
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Select(mi => InjectMethodInfo.Create(mi))
                    .Where(mi => mi != null)
                    .FirstOrDefault();
            }

            private readonly InjectMethodInfo InjectMethodInfo;

            public InjectMethodInfo GetMethodInfo()
            {
                return InjectMethodInfo;
            }
        }

        public bool Inject(object instance, IContainer container, params object[] customParams)
        {
            InjectTypeInfo typeInfo = GetTypeInfo(instance.GetType().UnderlyingSystemType);
            if (typeInfo != null)
            {
                InjectMethodInfo methodInfo = typeInfo.GetMethodInfo();
                InjectParameterInfo[] parameterInfos = methodInfo.GetParameters();
                object[] parameters = new object[parameterInfos.Length];

                if (customParams != null)
                {
                    for (int i = 0; i < customParams.Length; ++i)
                    {
                        object customParam = customParams[i];
                        if (customParam.IsNullOrDefault())
                            continue;

                        Type customType = customParam.GetType().UnderlyingSystemType;

                        for (int j = 0; j < parameters.Length; ++j)
                        {
                            if (!parameters[j].IsNullOrDefault())
                                continue;

                            InjectParameterInfo parameterInfo = parameterInfos[j];
                            if (!parameterInfo.Type.IsAssignableFrom(customType))
                                continue;
                            parameters[j] = customParam;
                        }
                    }
                }

                for (int i = 0; i < parameters.Length; ++i)
                {
                    if (!parameters[i].IsNullOrDefault())
                        continue;

                    InjectParameterInfo parameterInfo = parameterInfos[i];
                    parameters[i] = container.Resolve(parameterInfo.Type, new ServiceKey(parameterInfo.Key));
                    if (parameters[i].IsNullOrDefault())
                        parameters[i] = parameterInfo.DefaultValue;
                }

                methodInfo.Invoke(instance, parameters);

                return true;
            }
            return false;
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