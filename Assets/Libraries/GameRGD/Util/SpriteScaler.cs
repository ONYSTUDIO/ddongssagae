using UnityEngine;

namespace DoubleuGames.GameRGD
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteScaler : MonoBehaviour
    {
        private void Start()
        {
            var sprite = GetComponent<SpriteRenderer>();
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != default)
            {
                var rect = canvas.GetComponent<RectTransform>();
                sprite.transform.localScale = Vector3.one / rect.localScale.x;
            }
        }
    }
}
