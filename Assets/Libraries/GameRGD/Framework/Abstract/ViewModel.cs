namespace Helen
{
    public abstract class ViewModel : Model
    {
    }

    public static class ViewModelModelExtensions
    {
        public static T InjectBy<T>(this T viewModel, IContainer container, params object[] customParams)
            where T : ViewModel
        {
            return container.Inject(viewModel, customParams);
        }
    }
}