using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class SpriteContainer : MonoBehaviour
    {
        [SerializeField] protected List<Sprite> SpriteList = null;

        public Sprite GetSprite(int index)
        {
            if (index < 0 || SpriteList.Count <= index)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return SpriteList[index];
        }
    }
}
