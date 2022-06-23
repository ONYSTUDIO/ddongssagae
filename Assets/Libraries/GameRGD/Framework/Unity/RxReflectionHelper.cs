using NiceJson;
using System;
using System.Linq;
using System.Reflection;
using UniRx;
using Cysharp.Threading.Tasks;

public static class RxReflectionHelper
{
    public static RxProperty[] GetRxProperties(
        Type type,
        bool canRead = true,
        bool canWrite = true,
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    {
        return type
            .GetProperties(bindingFlags)
            .Where(pi => pi.CanRead == canRead && pi.CanWrite == canWrite)
            .Select(pi => RxProperty.Create(pi))
            .Where(rx => rx != null)
            .ToArray();
    }

    private static MethodInfo asUnitObservableMethodInfo;

    public static MethodInfo GetAsUnitObservableMethodInfo()
    {
        if (asUnitObservableMethodInfo == null)
            asUnitObservableMethodInfo = typeof(UniRx.Observable).GetMethod("AsUnitObservable");
        return asUnitObservableMethodInfo;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<보류 중>")]
    public class RxProperty
    {
        public static RxProperty Create(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
                return new RxProperty(propertyInfo);
            return null;
        }

        public string Name => PropertyInfo.Name;

        public Type ArgumentType { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public PropertyInfo ValuePropertyInfo { get; private set; }

        protected RxProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            ArgumentType = propertyInfo.PropertyType.GetGenericArguments().FirstOrDefault();
            ValuePropertyInfo = propertyInfo.PropertyType.GetProperty("Value");
        }

        public object CreateInstance()
        {
            return Activator.CreateInstance(PropertyInfo.PropertyType);
        }

        public void SetValue(object obj, object value)
        {
            PropertyInfo.SetValue(obj, value);
        }

        public object GetValue(object obj)
        {
            return PropertyInfo.GetValue(obj);
        }

        public void SetRxValue(object obj, JsonNode jsonNode)
        {
            SetRxValue(obj, JsonHelper.ToObject(jsonNode, ArgumentType));
        }

        public void SetRxValue(object obj, object value)
        {
            object rxObj = GetValue(obj);
            ValuePropertyInfo.SetValue(rxObj, value);
        }

        public object GetRxValue(object obj)
        {
            object rxObj = GetValue(obj);
            return ValuePropertyInfo.GetValue(rxObj);
        }

        public IObservable<Unit> AsUnitObservable(object obj)
        {
            IObservable<Unit> observable = RxReflectionHelper
                .GetAsUnitObservableMethodInfo()
                .MakeGenericMethod(ArgumentType)
                .Invoke(null, new object[] { GetValue(obj) }) as IObservable<Unit>;
            return observable.Skip(1);
        }
    }
}