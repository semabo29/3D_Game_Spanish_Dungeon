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
    private PhysicMaterial voxelMaterial;

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    // ---------- Temblor de cámara ----------

    public static void Shake(float intensity = 0.15f, float duration = 0.2f)
    {
        float scale = GameSettings.ShakeScale;
        if (scale <= 0f) return;
        CameraController cam = CameraController.Instance;
        if (cam != null) cam.AddShake(intensity * scale, duration);
    }

    // ---------- Hitstop (congelación breve del tiempo) ----------

    public static void Hitstop(float duration = 0.06f)
    {
        // No interferimos con la pausa ni con las pantallas finales
        if (Time.timeScale < 0.99f || !GameSettings.Hitstop) return;
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

    private void EnsurePhysicMaterial()
    {
        if (voxelMaterial == null)
        {
            voxelMaterial = new PhysicMaterial("VoxelBounce") { bounciness = 0.45f, dynamicFriction = 0.6f, staticFriction = 0.6f, bounceCombine = PhysicMaterialCombine.Maximum };
        }
    }

    private void SpawnVoxels(Vector3 position, Color color, int count, float force)
    {
        EnsurePhysicMaterial();

        for (int i = 0; i < count; i++)
        {
            float size = Random.Range(0.08f, 0.18f);
            float shade = Random.Range(0.75f, 1.15f);
            Color c = new Color(color.r * shade, color.g * shade, color.b * shade, 1f);
            GameObject cube = SpawnDebrisCube(position + Random.insideUnitSphere * 0.25f + Vector3.up * 0.3f, size, c);

            Rigidbody rb = cube.GetComponent<Rigidbody>();
            Vector3 dir = Random.insideUnitSphere;
            dir.y = Mathf.Abs(dir.y) + 0.6f;
            rb.velocity = dir.normalized * Random.Range(force * 0.5f, force);
            rb.angularVelocity = Random.insideUnitSphere * 10f;

            cube.AddComponent<ShrinkAndDie>().lifetime = Random.Range(1.1f, 1.8f);
        }
    }

    // Cubito con física que rebota en el suelo (capa Ignore Raycast para no confundir a nadie)
    private GameObject SpawnDebrisCube(Vector3 position, float size, Color color)
    {
        GameObject cube = new GameObject("Voxel");
        cube.layer = 2;
        cube.transform.position = position;
        cube.transform.rotation = Random.rotation;
        cube.transform.localScale = Vector3.one * size;
        cube.AddComponent<MeshFilter>().sharedMesh = VoxelLib.Cube;
        MeshRenderer r = cube.AddComponent<MeshRenderer>();
        r.sharedMaterial = VoxelLib.Lit;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_Color", color);
        r.SetPropertyBlock(block);

        BoxCollider col = cube.AddComponent<BoxCollider>();
        col.sharedMaterial = voxelMaterial;
        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.mass = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        return cube;
    }

    // Polvillo al aterrizar o al pisar fuerte
    public static void Dust(Vector3 position, Color color, int count = 5)
    {
        if (!GameSettings.AmbientEffects) count = Mathf.Min(count, 2);
        GameFeel gf = Instance;
        gf.EnsurePhysicMaterial();
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), 0.05f, Random.Range(-0.3f, 0.3f));
            GameObject cube = gf.SpawnDebrisCube(position + offset, Random.Range(0.05f, 0.09f), color);
            Destroy(cube.GetComponent<BoxCollider>());
            Rigidbody rb = cube.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.drag = 3f;
            rb.velocity = new Vector3(offset.x * 4f, Random.Range(0.6f, 1.2f), offset.z * 4f);
            cube.AddComponent<ShrinkAndDie>().lifetime = Random.Range(0.35f, 0.55f);
        }
    }

    // ---------- Muñeco de trapo: el enemigo sale volando entero ----------

    public static void Ragdoll(GameObject source, Vector3 direction, float force = 6f)
    {
        if (source == null) return;
        GameFeel gf = Instance;
        gf.EnsurePhysicMaterial();

        GameObject root = new GameObject("Ragdoll_" + source.name);
        root.layer = 2;
        root.transform.position = source.transform.position + Vector3.up * 0.15f;
        root.transform.rotation = source.transform.rotation;

        Bounds bounds = new Bounds(source.transform.position, Vector3.zero);
        bool any = false;
        foreach (MeshRenderer mr in source.GetComponentsInChildren<MeshRenderer>())
        {
            if (!mr.enabled) continue;
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            GameObject part = new GameObject(mr.name);
            part.layer = 2;
            part.transform.SetPositionAndRotation(mr.transform.position, mr.transform.rotation);
            part.transform.localScale = mr.transform.lossyScale;
            part.transform.SetParent(root.transform, true);
            part.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            MeshRenderer pr = part.AddComponent<MeshRenderer>();
            pr.sharedMaterials = mr.sharedMaterials;

            if (!any) { bounds = mr.bounds; any = true; }
            else bounds.Encapsulate(mr.bounds);
        }

        if (!any)
        {
            Destroy(root);
            return;
        }

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.center = root.transform.InverseTransformPoint(bounds.center);
        col.size = new Vector3(0.5f, Mathf.Max(0.3f, bounds.size.y * 0.8f), 0.5f);
        col.sharedMaterial = gf.voxelMaterial;

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        Vector3 dir = direction.sqrMagnitude > 0.01f ? direction.normalized : Random.insideUnitSphere;
        dir.y = 0f;
        rb.velocity = dir * force * 0.6f + Vector3.up * force;
        rb.angularVelocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 14f;

        root.AddComponent<ShrinkAndDie>().lifetime = 1.6f;
    }

    // ---------- Confeti de celebración (funciona con el juego congelado) ----------

    public static void Celebrate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        GameObject go = new GameObject("Celebration");
        go.transform.SetParent(cam.transform, false);
        go.transform.localPosition = new Vector3(0f, -5f, 6f);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.useUnscaledTime = true;
        main.loop = false;
        main.duration = 3f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 0.6f;
        main.maxParticles = 400;
        Gradient g = new Gradient();
        g.SetKeys(new[]
        {
            new GradientColorKey(new Color(0.9f, 0.1f, 0.15f), 0f), new GradientColorKey(new Color(1f, 0.8f, 0.1f), 0.25f),
            new GradientColorKey(new Color(0.3f, 0.85f, 0.4f), 0.5f), new GradientColorKey(new Color(0.3f, 0.6f, 1f), 0.75f),
            new GradientColorKey(Color.white, 1f)
        }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        g.mode = GradientMode.Fixed;
        main.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 120), new ParticleSystem.Burst(0.4f, 80), new ParticleSystem.Burst(0.9f, 80)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 4f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 1f;

        ParticleSystemRenderer pr = go.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = VoxelLib.UnlitAlpha;

        ps.Play();
        Destroy(go, 6f);
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
