using UnityEngine;
using System.Collections;
using TMPro;

// Texto que sube y se desvanece (usa tiempo real para verse también durante el hitstop)
public class FloatAndFade : MonoBehaviour
{
    public float duration = 1.1f;
    public float riseSpeed = 1.2f;
    private float timer;
    private TextMeshPro tmp;
    private Vector3 baseScale;

    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        timer += dt;
        float t = timer / duration;
        if (t >= 1f) { Destroy(gameObject); return; }

        transform.position += Vector3.up * riseSpeed * dt;

        // Pequeño "pop" al aparecer
        float pop = t < 0.15f ? Mathf.Lerp(0.6f, 1.15f, t / 0.15f) : Mathf.Lerp(1.15f, 1f, (t - 0.15f) / 0.2f);
        transform.localScale = baseScale * pop;

        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
            tmp.color = c;
        }
    }
}
