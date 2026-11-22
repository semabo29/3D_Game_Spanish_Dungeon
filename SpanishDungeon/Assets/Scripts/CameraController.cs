using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Transform target;

    [SerializeField] private float smoothSpeed = 0.125f;

    private Vector3 offset;

    // Posición "limpia" sin temblor, para que el shake no se acumule con el suavizado
    private Vector3 basePosition;
    private float shakeIntensity;
    private float shakeTimer;
    private float shakeDuration;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        basePosition = transform.position;
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AddShake(float intensity, float duration)
    {
        // Nos quedamos con el temblor más fuerte si se solapan
        if (intensity >= shakeIntensity * (shakeTimer / Mathf.Max(shakeDuration, 0.0001f)))
        {
            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeTimer = duration;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            // Suavizado independiente del framerate (equivale al 0.125 original a 60 fps)
            float t = 1f - Mathf.Pow(1f - smoothSpeed, Time.unscaledDeltaTime * 60f);
            basePosition = Vector3.Lerp(basePosition, desiredPosition, t);
        }

        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            float strength = shakeIntensity * Mathf.Clamp01(shakeTimer / shakeDuration);
            shakeOffset = (transform.right * Random.Range(-1f, 1f) + transform.up * Random.Range(-1f, 1f)) * strength;
        }

        transform.position = basePosition + shakeOffset;
    }
}
