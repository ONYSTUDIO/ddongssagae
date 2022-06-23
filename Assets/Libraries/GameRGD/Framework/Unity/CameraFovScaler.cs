using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFovScaler : MonoBehaviour
{
    private Camera targetCarmera;

    [SerializeField] private float referenceFov = 45.0f;
    [SerializeField] private float referenceAspect = 800.0f / 1600.0f;

    public float VerticalSize => referenceFov;
    public float HorizontalSize => referenceFov * referenceAspect;
    public float FieldOfView
    {
        get => referenceFov;
        set
        {
            referenceFov = value;
            UpdateTargetCameraFov();
        }
    }

    private void Awake()
    {
        targetCarmera = GetComponent<Camera>();
    }

    private void Start()
    {
        UpdateTargetCameraFov();
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void Update()
    {
        UpdateTargetCameraFov();
    }
#endif

    private void UpdateTargetCameraFov()
    {
        if (targetCarmera)
            targetCarmera.fieldOfView = HorizontalSize / targetCarmera.aspect;
    }
}

public static class CameraFovScalerExtensions
{
    public static float GetFieldOfView(this Camera camera)
    {
        var scaler = camera.GetComponent<CameraFovScaler>();
        if (scaler)
            return scaler.FieldOfView;
        else
            return camera.fieldOfView;
    }

    public static void SetFieldOfView(this Camera camera, float value)
    {
        var scaler = camera.GetComponent<CameraFovScaler>();
        if (scaler)
            scaler.FieldOfView = value;
        else
            camera.fieldOfView = value;
    }
}