using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class CDontDestroyOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
        }
    }
}
