/// <summary>
/// 아래와 같은 경우와 같이 오버로딩을 의도대로 할 수 없을 경우 사용하자.
///
/// public class AbstractClass
/// {
///     protected virtual void Something(object parameter)
///     {
///     }
/// }
///
/// public class GeneralClass : AbstractClass
/// {
///     public void Something(object parameter)
///     {
///         base.Something(parameter);
///     }
/// }
///
/// </summary>

public class SafeOverloading
{
    private SafeOverloading()
    {
    }
}