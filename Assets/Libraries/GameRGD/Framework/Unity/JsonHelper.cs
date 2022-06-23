using NiceJson;
using System;
using System.Collections;
using System.Reflection;
using UniRx;
using UnityEngine;
using Helen;

/// <summary>
/// Foundation/Tests/EditorTests/JsonHelperTests
/// </summary>
public static class JsonHelper
{
    public static string EmptyObjectJson
    {
        get => "{}";
    }

    public static bool IsEmptyOrDefault(this JsonNode jsonNode)
    {
        if (jsonNode == null)
            return true;

        if (jsonNode is JsonObject && (jsonNode as JsonObject).Count <= 0)
            return true;

        if (jsonNode is JsonArray && (jsonNode as JsonArray).Count <= 0)
            return true;

        if (jsonNode is JsonBasic && (jsonNode as JsonBasic).ValueObject.IsNullOrDefault())
            return true;

        return false;
    }

    public static JsonNode Parse(string json)
    {
        return JsonNode.ParseJsonString(json);
    }

    public static bool Parse<T>(this JsonNode jsonNode, string key, T target)
        where T : IJsonParsable
    {
        JsonNode targetNode = jsonNode.GetValue(key);
        if (targetNode != null)
        {
            target.ParseFromJson(targetNode);
            return true;
        }
        return false;
    }

    public static string ToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T ToObject<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    public static object ToObject(string json, Type type)
    {
        return JsonUtility.FromJson(json, type);
    }

    public static T ToObject<T>(JsonNode node)
    {
        return node == null ? default : (T)ToObject(node, typeof(T));
    }

    public static object ToObject(JsonNode node, Type type, string hint = null)
    {
        if (node is JsonBasic)
        {
            JsonBasic jsonBasic = node as JsonBasic;
            try
            {
                if (type.IsEnum)
                {
                    if (jsonBasic.ValueObject is string)
                        return Enum.Parse(type, (string)jsonBasic.ValueObject, true);
                    else
                        return Enum.ToObject(type, jsonBasic.ValueObject);
                }
                return Convert.ChangeType(jsonBasic.ValueObject, type);
            }
            catch (Exception exception)
            {
                Log.Error($"{exception.Message} : {hint ?? "null"} : {jsonBasic.ToJsonString()}");
                throw exception;
            }
        }
        else if (node is JsonArray)
        {
            if (type.IsArray)
            {
                JsonArray jsonArray = node as JsonArray;
                Type arrayElementType = type.GetElementType();
                Array array = Array.CreateInstance(arrayElementType, jsonArray.Count);
                for (int i = 0; i < jsonArray.Count; i++)
                    array.SetValue(ToObject(jsonArray[i], arrayElementType, hint), i);
                return array;
            }
            else if (type.IsGenericType && typeof(IList).IsAssignableFrom(type))
            {
                JsonArray jsonArray = node as JsonArray;
                Type arrayElementType = type.GetGenericArguments()[0];
                IList array = (IList)Activator.CreateInstance(type);
                for (int i = 0; i < jsonArray.Count; i++)
                    array.Add(ToObject(jsonArray[i], arrayElementType, hint));
                return array;
            }
            else
                Assert.Fail("unsupport type");
        }
        else if (node is JsonObject)
        {
            JsonObject jsonObject = node as JsonObject;
            object instance = Activator.CreateInstance(type);
            Type serializeFieldType = typeof(SerializeField);
            foreach (FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if ((fi.IsPublic || fi.IsDefined(serializeFieldType, true)) && jsonObject.ContainsKey(fi.Name))
                    fi.SetValue(instance, ToObject(jsonObject[fi.Name], fi.FieldType, fi.Name));
            }
            foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (pi.CanWrite && jsonObject.ContainsKey(pi.Name))
                    pi.SetValue(instance, ToObject(jsonObject[pi.Name], pi.PropertyType, pi.Name), null);
            }
            return instance;
        }

        Assert.Fail("unsupport type");
        return null;
    }

    public static JsonNode GetValue(this JsonNode jsonNode, string key)
    {
        if (jsonNode == null)
            Log.Error($"JsonNodeGetValueError: {key}");

        if (jsonNode.ContainsKey(key))
            return jsonNode[key];
        return null;
    }

    public static bool TryGetNode(this JsonNode jsonNode, string key, out JsonNode target)
    {
        target = default;

        if (jsonNode.ContainsKey(key))
        {
            target = jsonNode[key];
            return true;
        }
        return false;
    }

    public static bool TryGetArrayNode(this JsonNode jsonNode, string key, out JsonArray target)
    {
        target = default;

        if (jsonNode.ContainsKey(key))
        {
            target = jsonNode[key] as JsonArray;
            return target != null;
        }
        return false;
    }

    #region parse for etc

    public static bool ReadContainer(
        this JsonNode jsonNode, string key, Action<JsonNode> onDelete, Action<JsonNode> onUpsert)
    {
        Assert.IsNotNull(onUpsert);
        Assert.IsNotNull(onDelete);

        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        JsonNode deleteNode = itemNode.GetValue("delete");
        if (deleteNode != null && deleteNode is JsonArray)
        {
            JsonArray jsonArray = deleteNode as JsonArray;
            for (int i = 0; i < jsonArray.Count; ++i)
                onDelete(jsonArray[i]);
        }

        JsonNode upsertNode = itemNode.GetValue("upsert");
        if (upsertNode != null && upsertNode is JsonArray)
        {
            JsonArray jsonArray = upsertNode as JsonArray;
            for (int i = 0; i < jsonArray.Count; ++i)
                onUpsert(jsonArray[i]);
        }

        return deleteNode != null || upsertNode != null;
    }

    public static bool ReadValue(
        this JsonNode jsonNode, string key, Action<JsonNode> parser)
    {
        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        parser?.Invoke(itemNode);
        return true;
    }

    public static bool ReadValue<T>(
        this JsonNode jsonNode, string key, ref ReactiveCollection<T> target)
    {
        T[] array = null;
        if (!jsonNode.ReadValue(key, ref array))
            return false;

        target.Clear();

        foreach (var value in array)
            target.Add(value);

        return true;
    }

    #endregion parse for etc

    #region parse for <T>

    public static bool ReadValue<T>(
        this JsonNode jsonNode, string key, ref T target)
    {
        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        target = ToObject<T>(itemNode);
        return true;
    }

    public static bool ReadValue<T>(
        this JsonNode jsonNode, string key, ref ReactiveProperty<T> target)
    {
        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        target.Value = ToObject<T>(itemNode);
        return true;
    }

    public static bool TryGetValue<T>(
        this JsonNode jsonNode, string key, out T target)
    {
        target = default;

        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        target = ToObject<T>(itemNode);
        return true;
    }

    #endregion parse for <T>

    #region parse for DateTime?

    public static bool ReadValue(
        this JsonNode jsonNode, string key, ref ReactiveProperty<DateTime?> target)
    {
        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        if (DateTime.TryParse(itemNode, out DateTime dateTime))
            target.Value = dateTime;
        else
            target.Value = null;

        return true;
    }

    public static bool ReadValue(
        this JsonNode jsonNode, string key, ref DateTime? target)
    {
        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        if (DateTime.TryParse(itemNode, out DateTime dateTime))
            target = dateTime;
        else
            target = null;

        return true;
    }

    public static bool TryGetValue(
        this JsonNode jsonNode, string key, out DateTime? target)
    {
        target = default;

        JsonNode itemNode = jsonNode.GetValue(key);
        if (itemNode == null)
            return false;

        if (DateTime.TryParse(itemNode, out DateTime dateTime))
            target = dateTime;
        else
            target = null;

        return true;
    }

    #endregion parse for DateTime?

    private struct Wrapper<T>
    {
#pragma warning disable 0649
        public T[] array;
#pragma warning restore 0649
    }

    public static Array GetJsonArray(string json, Type jsonType)
    {
        string newJson = "{ \"array\": " + json + "}";

        Type type = typeof(Wrapper<>).MakeGenericType(jsonType);
        object jsonArray = JsonUtility.FromJson(newJson, type);
        System.Reflection.FieldInfo fi = type.GetField("array");
        object array = null;
        if (fi != null)
            array = fi.GetValue(jsonArray);

        return array as Array;
    }

    public static T[] GetJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.array = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }
}

public interface IJsonParsable
{
    void ParseFromJson(JsonNode jsonNode);
}

#if UNUSED
[AttributeUsage(AttributeTargets.Class)]
public sealed class JsonContainerKeyAttribute : Attribute
{
    public string Key { get; private set; }

    public JsonContainerKeyAttribute(string key)
    {
        Key = key;
    }
}
#endif