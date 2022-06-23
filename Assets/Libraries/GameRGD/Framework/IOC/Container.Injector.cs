namespace Helen
{
    public static partial class DependencyInjector
    {
        private static readonly InstanceFieldInjector fieldInjector = new InstanceFieldInjector();
        private static readonly InstanceMethodInjector methodInjector = new InstanceMethodInjector();

        public static T Inject<T>(
            this IContainer container, T instance, params object[] customParams)
        {
            bool injected = false;
            injected |= fieldInjector.Inject(instance, container);
            injected |= methodInjector.Inject(instance, container, customParams);
            //if (!injected)
            //    Log.Warning($"[IOC] no injection needed - {instance.GetType().UnderlyingSystemType.Name}");
            return instance;
        }

        public static object Inject(
            this IContainer container, object instance, params object[] customParams)
        {
            bool injected = false;
            injected |= fieldInjector.Inject(instance, container);
            injected |= methodInjector.Inject(instance, container, customParams);
            //if (!injected)
            //    Log.Warning($"[IOC] no injection needed - {instance.GetType().UnderlyingSystemType.Name}");
            return instance;
        }
    }
}