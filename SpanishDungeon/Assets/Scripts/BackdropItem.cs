using UnityEngine;
using System.Collections.Generic;

// Elemento del fondo: paralaje, deriva horizontal, balanceo, giro y aleteo
public class BackdropItem : MonoBehaviour
{
    public static Vector2 CameraDelta;

    public float parallax;
    public float drift;
    public float wrap;
    public float bobAmplitude;
    public float bobSpeed = 1f;
    public float spin;
    public float flap;
    public float lifetime = -1f;

    private Vector3 basePos;
    private Vector3 baseScale;
    private float phase;
    private bool initialized;

    void Start()
    {
        if (!initialized) Rebase();
        phase = Random.value * 10f;
    }

    public void Rebase()
    {
        basePos = transform.localPosition;
        baseScale = transform.localScale;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) Rebase();
        float dt = Time.deltaTime;

        if (drift != 0f)
        {
            basePos.x += drift * dt;
            if (wrap > 0f)
            {
                if (basePos.x > wrap) basePos.x -= wrap * 2f;
                else if (basePos.x < -wrap) basePos.x += wrap * 2f;
            }
        }

        Vector3 p = basePos;
        bool still = GameSettings.ReduceMotion;
        if (!still)
        {
            p.x -= CameraDelta.x * parallax;
            p.y -= CameraDelta.y * parallax;
        }
        if (bobAmplitude > 0f && !still) p.y += Mathf.Sin((Time.time + phase) * bobSpeed) * bobAmplitude;
        transform.localPosition = p;

        if (spin != 0f) transform.Rotate(0f, 0f, spin * dt, Space.Self);

        if (flap > 0f)
        {
            float s = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin((Time.time + phase) * flap));
            transform.localScale = new Vector3(baseScale.x, baseScale.y * s, baseScale.z);
        }

        if (lifetime > 0f)
        {
            lifetime -= dt;
            if (lifetime <= 0f) Destroy(gameObject);
        }
    }
}
