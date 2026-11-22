using UnityEngine;
using System.Collections;
using TMPro;

// Efectos de "game feel" reutilizables: temblor de cámara, hitstop,
// explosiones de vóxeles y textos flotantes. Se crea solo la primera vez que se usa.
public class GameFeel : MonoBehaviour
{
    private static GameFeel instance;

    private static GameFeel Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameFeel");
                instance = go.AddComponent<GameFeel>();
            }
            return instance;
        }
    }

    private Coroutine hitstopRoutine;
    private Material voxelMaterial;

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // ---------- Temblor de cámara ----------

    public static void Shake(float intensity = 0.15f, float duration = 0.2f)
    {
        CameraController cam = CameraController.Instance;
        if (cam != null) cam.AddShake(intensity, duration);
    }

    // ---------- Hitstop (congelación breve del tiempo) ----------

    public static void Hitstop(float duration = 0.06f)
    {
        // No interferimos con la pausa ni con las pantallas finales
        if (Time.timeScale < 0.99f) return;
        GameFeel gf = Instance;
        if (gf.hitstopRoutine != null) gf.StopCoroutine(gf.hitstopRoutine);
        gf.hitstopRoutine = gf.StartCoroutine(gf.HitstopRoutine(duration));
    }

    private IEnumerator HitstopRoutine(float duration)
    {
        const float slowScale = 0.05f;
        Time.timeScale = slowScale;
        yield return new WaitForSecondsRealtime(duration);
        // Solo restauramos si nadie más ha tocado el tiempo (pausa, game over...)
        if (Mathf.Approximately(Time.timeScale, slowScale)) Time.timeScale = 1f;
        hitstopRoutine = null;
    }

    // ---------- Explosión de vóxeles ----------

    public static void VoxelBurst(Vector3 position, Color color, int count = 14, float force = 4f)
    {
        Instance.SpawnVoxels(position, color, count, force);
    }

    private void SpawnVoxels(Vector3 position, Color color, int count, float force)
    {
        if (voxelMaterial == null)
        {
            GameObject probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            voxelMaterial = new Material(probe.GetComponent<Renderer>().sharedMaterial);
            Destroy(probe);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Voxel";
            Destroy(cube.GetComponent<Collider>());

            float size = Random.Range(0.08f, 0.18f);
            cube.transform.localScale = Vector3.one * size;
            cube.transform.position = position + Random.insideUnitSphere * 0.25f + Vector3.up * 0.3f;
            cube.transform.rotation = Random.rotation;

            Renderer r = cube.GetComponent<Renderer>();
            r.sharedMaterial = voxelMaterial;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            float shade = Random.Range(0.75f, 1.15f);
            block.SetColor("_Color", new Color(color.r * shade, color.g * shade, color.b * shade, 1f));
            r.SetPropertyBlock(block);

            Rigidbody rb = cube.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            Vector3 dir = Random.insideUnitSphere;
            dir.y = Mathf.Abs(dir.y) + 0.6f;
            rb.velocity = dir.normalized * Random.Range(force * 0.5f, force);
            rb.angularVelocity = Random.insideUnitSphere * 10f;

            cube.AddComponent<ShrinkAndDie>().lifetime = Random.Range(0.6f, 1.1f);
        }
    }

    // ---------- Texto flotante ----------

    public static void FloatingText(Vector3 worldPos, string text, Color color, float size = 4f)
    {
        GameObject go = new GameObject("FloatingText");
        go.transform.position = worldPos + Vector3.up * 1.4f;

        Camera cam = Camera.main;
        if (cam != null) go.transform.rotation = cam.transform.rotation;

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color32(0, 0, 0, 255);
        tmp.rectTransform.sizeDelta = new Vector2(8f, 2f);
        tmp.sortingOrder = 100;

        go.AddComponent<FloatAndFade>();
    }
}

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
        transform.localScale = startScale * (1f - t * t);
    }
}

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
