using UnityEngine;
using System.Collections.Generic;

// Pequeño salto de aparición (monedas soltadas, objetos nuevos)
public class PopIn : MonoBehaviour
{
    private Vector3 baseScale;
    private float t;

    void Start()
    {
        baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        t += Time.deltaTime / 0.35f;
        if (t >= 1f)
        {
            transform.localScale = baseScale;
            Destroy(this);
            return;
        }
        transform.localScale = baseScale * EaseOutBack(t);
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }
}
