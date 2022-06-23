using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    [AddComponentMenu("UI/StatusImage")]
    public class StatusImage : SpriteContainer
    {
        private Image mImage;

        private void Awake()
        {
            if (mImage == null) mImage = GetComponent<Image>();
        }

        public void ChangeStatus(int status)
        {
            if (mImage == null) mImage = GetComponent<Image>();
            if (mImage == null) throw new NullReferenceException(nameof(mImage));

            mImage.sprite = GetSprite(status);
        }
    }
}
