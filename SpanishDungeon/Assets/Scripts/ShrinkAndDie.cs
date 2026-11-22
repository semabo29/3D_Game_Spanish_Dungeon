using UnityEngine;
using System.Collections;
using TMPro;

// Cubito que encoge y desaparece
public class ShrinkAndDie : MonoBehaviour
{
    public float lifetime = 1f;
    private float timer;
    private Vector3 startScale;

    void Start() { startScale = transform.localScale; }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;
        if (t >= 1f) { Destroy(gameObject); return; }
        // Se queda entero casi todo el rato y encoge al final
        float k = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        transform.localScale = startScale * k;
    }
}
