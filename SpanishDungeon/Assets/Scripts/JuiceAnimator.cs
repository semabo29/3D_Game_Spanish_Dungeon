using UnityEngine;
using System.Collections.Generic;

// Animación procedural para cualquier personaje que se mueva por casillas:
// saltito al avanzar, estirar y encoger (squash & stretch), respiración en reposo,
// contoneo, embestida al atacar y polvo al aterrizar.
// Solo toca a los hijos visuales: la raíz (con su lógica y colisión) no se altera.
public class JuiceAnimator : MonoBehaviour
{
    public float hopHeight = 0.22f;
    public float squashAmount = 0.18f;
    public float breathe = 0.025f;
    public float waddle = 7f;
    public bool dustOnLand;
    public Color dustColor = new Color(0.75f, 0.72f, 0.65f);
    public float gridSize = 1f;

    private struct Visual
    {
        public Transform t;
        public Vector3 pos;
        public Vector3 scale;
        public Quaternion rot;
    }

    private readonly List<Visual> visuals = new List<Visual>();
    private Vector3 lastPos;
    private float hopPhase;
    private bool wasMoving;
    private float squash;       // >0 aplastado, <0 estirado
    private float squashVel;
    private Vector3 lunge;
    private float idleTime;
    private float seed;

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponentInChildren<Renderer>() == null) continue;
            visuals.Add(new Visual { t = child, pos = child.localPosition, scale = child.localScale, rot = child.localRotation });
        }
        lastPos = transform.position;
        seed = Random.value * 10f;
    }

    // Golpe de aplastamiento (positivo) o estiramiento (negativo)
    public void Punch(float amount)
    {
        squashVel += amount * 12f;
    }

    // Embestida visual hacia una dirección del mundo
    public void Lunge(Vector3 worldDir, float amount = 0.35f)
    {
        lunge = worldDir.normalized * amount;
        Punch(-0.6f);
    }

    void LateUpdate()
    {
        if (visuals.Count == 0) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 delta = transform.position - lastPos;
        delta.y = 0f;
        float speed = delta.magnitude / dt;
        lastPos = transform.position;

        bool moving = speed > 0.2f && delta.magnitude < gridSize * 0.9f;
        float hop = 0f;
        float tilt = 0f;

        if (moving)
        {
            hopPhase += delta.magnitude / gridSize * Mathf.PI;
            hop = Mathf.Abs(Mathf.Sin(hopPhase)) * hopHeight;
            tilt = Mathf.Sin(hopPhase) * waddle;
            idleTime = 0f;
        }
        else
        {
            if (wasMoving)
            {
                Punch(0.9f);
                if (dustOnLand) GameFeel.Dust(transform.position - Vector3.up * 0.45f, dustColor, 4);
            }
            hopPhase = 0f;
            idleTime += dt;
        }
        wasMoving = moving;

        // Muelle amortiguado para el squash
        float springForce = -squash * 180f - squashVel * 14f;
        squashVel += springForce * dt;
        squash += squashVel * dt;
        squash = Mathf.Clamp(squash, -0.6f, 0.6f);

        float breath = moving ? 0f : Mathf.Sin((Time.time + seed) * 2.6f) * breathe;
        float s = squash * squashAmount + breath;
        if (moving) s -= Mathf.Abs(Mathf.Cos(hopPhase)) * 0.05f; // estirado en el aire
        Vector3 scaleK = new Vector3(1f + s * 0.6f, 1f - s, 1f + s * 0.6f);

        lunge = Vector3.Lerp(lunge, Vector3.zero, 1f - Mathf.Exp(-14f * dt));
        Vector3 lungeLocal = transform.InverseTransformDirection(lunge);

        foreach (Visual v in visuals)
        {
            if (v.t == null) continue;
            v.t.localPosition = v.pos + Vector3.up * hop + lungeLocal;
            v.t.localScale = Vector3.Scale(v.scale, scaleK);
            v.t.localRotation = v.rot * Quaternion.Euler(0f, 0f, tilt);
        }
    }

    void OnDisable()
    {
        foreach (Visual v in visuals)
        {
            if (v.t == null) continue;
            v.t.localPosition = v.pos;
            v.t.localScale = v.scale;
            v.t.localRotation = v.rot;
        }
    }
}
