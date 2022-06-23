namespace Helen
{
    public abstract class ContentModel : Model
    {
        public IContainer Container
        {
            get => container;
            protected set => container = value;
        }
        [Inject]
#pragma warning disable IDE0044 // 읽기 전용 한정자 추가
        private IContainer container = null;
#pragma warning restore IDE0044 // 읽기 전용 한정자 추가
    }

    public static class ContentModelExtensions
    {
        public static T InjectBy<T>(this T contentModel, IContainer container, params object[] customParams)
            where T : ContentModel
        {
            return container.Inject(contentModel, customParams);
        }
    }
}