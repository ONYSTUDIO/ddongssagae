using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace DoubleuGames.GameRGD
{
    public class ApplicationProperties : CSingleton<ApplicationProperties>
    {
        private bool IsInit = false;

        private Dictionary<string, object> Properties;

        public static void Initialize()
        {
            var tmp = ApplicationProperties.Instance;
            tmp.LoadProperties();
        }

        private void LoadProperties()
        {
            if (IsInit) return;
            IsInit = true;

            Properties = new Dictionary<string, object>();

            var resource = Resources.Load<TextAsset>("Application");
            var yaml = new YamlStream();
            yaml.Load(new StringReader(resource.text));

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            TraversalMappingNode(string.Empty, mapping);

            InjectValues();
        }

        private void InjectValues()
        {
            var typelist = Array.FindAll(Assembly.GetExecutingAssembly().GetTypes(), t => String.Equals(t.Namespace, "DoubleuGames.GameRGD", StringComparison.Ordinal));
            foreach (var type in typelist)
            {
                var attr = Attribute.GetCustomAttribute(type, typeof(PropertiesHolder));
                if (attr == null) continue;

                CLogger.Log($"type = {type}");
                InjectValues(type);
            }
        }

        private void InjectValues(Type type)
        {
            foreach (var pi in type.GetProperties(
                BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.DeclaredOnly))
            {
                var attr = pi.GetCustomAttribute<Value>(false);
                if (attr == null) continue;

                Properties.TryGetValue(attr.Key, out var value);
                if (value == null) continue;

                if (pi.CanWrite) pi.SetValue(null, value);
            }

            foreach (var pi in type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.DeclaredOnly))
            {
                var attr = pi.GetCustomAttribute<Value>(false);
                if (attr == null) continue;

                Properties.TryGetValue(attr.Key, out var value);
                if (value == null) continue;

                pi.SetValue(null, value);
            }
        }

        private void TraversalMappingNode(string breadcrumb, YamlMappingNode node)
        {
            foreach (var entry in node.Children)
            {
                var key = ((YamlScalarNode)entry.Key).Value;
                var value = entry.Value;

                if (breadcrumb.Length > 0) key = $"{breadcrumb}.{key}";

                if (value is YamlMappingNode)
                {
                    TraversalMappingNode(key, value as YamlMappingNode);
                }
                else if (value is YamlScalarNode)
                {
                    Properties.Add(key, ((value as YamlScalarNode).Value));
                    // CLogger.Log($"{key} = {(value as YamlScalarNode).Value}");
                }
            }
        }

        public string GetString(string key, string defaultValue = default)
        {
            Properties.TryGetValue(key, out var value);
            if (value != null) return value.ToString();
            return defaultValue;
        }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PropertiesHolder : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class Value : Attribute
    {
        public string Key
        {
            get; private set;
        }

        public Value(string key)
        {
            Key = key;

        }
    }
}
