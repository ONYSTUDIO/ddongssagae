using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class UIParticleScaler : MonoBehaviour
{
    public RectTransform referenceTransfrom;
    public Vector2 referenceSize;

    private void Awake()
    {
        if (referenceTransfrom)
        {
            referenceTransfrom
                .OnRectTransformDimensionsChangeAsObservable()
                .Subscribe(OnUpdateScale)
                .AddTo(this);
        }
    }

    private void OnEnable()
    {
        UpdateScale();
    }

    private void OnUpdateScale(Unit _)
    {
        UpdateScale();
    }

    private void UpdateScale()
    {
        if (referenceTransfrom == null)
            return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(
            referenceTransfrom.rect.width / referenceSize.x,
            referenceTransfrom.rect.height / referenceSize.y,
            rectTransform.localScale.z);
    }

    private void OnValidate()
    {
        if (referenceTransfrom)
        {
            if (referenceSize == Vector2.zero)
                referenceSize = referenceTransfrom.rect.size;
        }
        else
            referenceSize = Vector2.zero;

        UpdateScale();
    }
}