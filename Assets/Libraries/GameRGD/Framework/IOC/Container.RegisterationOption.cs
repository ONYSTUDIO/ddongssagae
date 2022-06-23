namespace Helen
{
    public interface IRegisterationOption
    {
        IRegisterationOption WithParams(params object[] customParams);
    }

    public interface IReferenceOption : IRegisterationOption
    {
        IReferenceOption DoNotDispose();
    }

    public interface IInstanceOption : IRegisterationOption
    {
        IInstanceOption DoNotDispose();
    }
}