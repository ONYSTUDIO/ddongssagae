using System;

namespace DoubleuGames.GameRGD
{
    public class Presenter : MonoBase
    {
    }

    public class Presenter<T> : Presenter where T : IView
    {
        private T m_View;
        protected T View
        {
            get
            {
                if (m_View == null)
                {
                    m_View = gameObject.GetComponent<T>();
                    if (m_View == null)
                    {
                        throw new NullReferenceException($"{nameof(m_View)} is null (Type = {typeof(T).Name})");
                    }
                }
                return m_View;
            }
        }
    }
}