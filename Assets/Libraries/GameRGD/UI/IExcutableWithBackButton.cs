using System.Collections.Generic;

namespace DoubleuGames.GameRGD
{
#if UNITY_EDITOR
    using UnityEditor;

    [InitializeOnLoad]
#endif
    public class RandomAccessStack
    {
        public List<IExcutableWithBackButton> BackButtonList = new List<IExcutableWithBackButton>();
        
        public int Count
        {
            get
            {
                return BackButtonList.Count;
            }
        }

        /// <summary>
        /// 인스턴스가 생성되고나면 호출해서 리스트에 넣어두어야함.
        /// </summary>
        /// <param name="button">Button.</param>
        public void Push(IExcutableWithBackButton button)
        {
            BackButtonList.Add(button);
        }

        /// <summary>
        /// 인스턴스가 생성되고나면 제거될때 호출해서 리스트에서 제거.
        /// </summary>
        /// <param name="button">Button.</param>
        public void Remove(IExcutableWithBackButton button)
        {
            BackButtonList.Remove(button);
        }
        
        public IExcutableWithBackButton Pop()
        {
            IExcutableWithBackButton result = Peek();
            if (result != null)
            {
                BackButtonList.RemoveAt(BackButtonList.Count-1);
            }
            return result;
        }
        public IExcutableWithBackButton Peek()
        {
            if (BackButtonList.Count == 0)
            {
                return null;
            }
            IExcutableWithBackButton result = BackButtonList[BackButtonList.Count-1];
            return result;
        }
        public IExcutableWithBackButton GetByIndex(int index, bool deleteResult = false)
        {
            if (index < 0 || index > BackButtonList.Count)
            {
                return null;
            }
            IExcutableWithBackButton result = BackButtonList[index];
            
            if (deleteResult)
            {
                BackButtonList.Remove(result);
            }
            return result;
        }
    }

    public interface IExcutableWithBackButton
    {
        /// <summary>
        /// 백버튼을 처리 할 수 없어서 다음 오브젝트에 이벤를 넘겨줄때는 false, 아니면 true.
        /// </summary>
        bool OnBackButton();
    }
}

