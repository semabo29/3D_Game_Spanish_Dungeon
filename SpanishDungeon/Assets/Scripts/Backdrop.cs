using UnityEngine;
using System.Collections.Generic;

// Escenario de fondo de cada sala: cielo degradado, paisaje voxel con paralaje,
// iluminación, partículas ambientales y pequeños detalles animados (nubes, pájaros,
// coches por la A-7, molinos girando, rayos sobre el Congreso...).
// Todo cuelga de la cámara, así que nunca molesta al mapa.
public class Backdrop : MonoBehaviour
{
    public static Backdrop Instance { get; private set; }

    // ¿La sala actual es de noche? (farolas encendidas, luz del jugador...)
    public static bool IsNight => Instance != null && Instance.theme != null && Instance.theme.night;

    // Capa propia para iluminar el fondo con su propia luz
    public const int BackdropLayer = 31;

    private enum Ambient { None, Confetti, Leaves, Dust, Fireflies, Papers }

    private class Theme
    {
        public Color skyTop, skyHorizon, groundTop, groundBottom;
        public Color light = Color.white;
        public float lightIntensity = 1f;
        public Color ambient = new Color(0.5f, 0.5f, 0.55f);
        public bool night;
        public Ambient particles = Ambient.None;
        public Color[] particleColors;
        public bool birds;
    }

    private const float Horizon = 1.0f;

    private Camera cam;
    private Transform root;
    private Light mainLight;
    private Light backLight;
    private Light playerLight;
    private ParticleSystem ambientParticles;
    private Vector3 cameraAnchor;
    private bool anchorSet;
    private float yaw;

    private int currentRoom = -1;
    private Theme theme;
    private float birdTimer = 4f;
    private float lightningTimer = 6f;
    private float mainLightBase = 1f;

    public static void ApplyTheme(int room)
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("Backdrop");
            go.AddComponent<Backdrop>();
        }
        Instance.SetTheme(room);
    }

    void Awake()
    {
        Instance = this;
        cam = Camera.main;

        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional) { mainLight = l; break; }
        }

        if (cam != null)
        {
            yaw = cam.transform.eulerAngles.y;
            root = new GameObject("BackdropRoot").transform;
            root.SetParent(cam.transform, false);

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.cullingMask |= 1 << BackdropLayer;
        }

        GameObject bl = new GameObject("BackdropLight");
        bl.transform.SetParent(transform, false);
        backLight = bl.AddComponent<Light>();
        backLight.type = LightType.Directional;
        backLight.shadows = LightShadows.None;
        backLight.cullingMask = 1 << BackdropLayer;
        bl.transform.rotation = Quaternion.Euler(40f, yaw + 35f, 0f);

        if (mainLight != null)
        {
            mainLight.cullingMask &= ~(1 << BackdropLayer);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------- Temas ----------

    private static Theme GetTheme(int room)
    {
        Theme t = new Theme();
        switch (room)
        {
            case 1: // San Fermín
                t.skyTop = C("#5DA9F6"); t.skyHorizon = C("#CFEFFF"); t.groundTop = C("#9BBF6A"); t.groundBottom = C("#5E8740");
                t.light = new Color(1f, 0.96f, 0.88f); t.lightIntensity = 1.05f;
                t.particles = Ambient.Confetti; t.particleColors = new[] { C("#E3122B"), C("#FFFFFF") }; t.birds = true;
                break;
            case 2: // Plaza de toros
                t.skyTop = C("#3F8FE0"); t.skyHorizon = C("#FFE2A8"); t.groundTop = C("#E8C07A"); t.groundBottom = C("#B88A45");
                t.light = new Color(1f, 0.92f, 0.78f); t.lightIntensity = 1.1f;
                t.particles = Ambient.Confetti; t.particleColors = new[] { C("#C8102E"), C("#FFC400") }; t.birds = true;
                break;
            case 3: // Las Ramblas
                t.skyTop = C("#6FBDF2"); t.skyHorizon = C("#E4F4FF"); t.groundTop = C("#BFB8AE"); t.groundBottom = C("#857E74");
                t.particles = Ambient.Leaves; t.particleColors = new[] { C("#7CB342"), C("#C0CA33"), C("#E0A030") }; t.birds = true;
                break;
            case 4: // Benidorm
                t.skyTop = C("#1E8FEA"); t.skyHorizon = C("#A6E4FF"); t.groundTop = C("#3B97D6"); t.groundBottom = C("#16568F");
                t.light = new Color(1f, 0.97f, 0.9f); t.lightIntensity = 1.15f; t.birds = true;
                break;
            case 5: // La Mancha
                t.skyTop = C("#7DBAF0"); t.skyHorizon = C("#FBEFD0"); t.groundTop = C("#E2C066"); t.groundBottom = C("#A98235");
                t.light = new Color(1f, 0.95f, 0.82f); t.lightIntensity = 1.1f;
                t.particles = Ambient.Dust; t.particleColors = new[] { C("#E8D08A"), C("#CFAE62") }; t.birds = true;
                break;
            case 6: // La A-7 al atardecer
                t.skyTop = C("#FF7A45"); t.skyHorizon = C("#FFD98A"); t.groundTop = C("#C68A55"); t.groundBottom = C("#7E4E2C");
                t.light = new Color(1f, 0.78f, 0.58f); t.lightIntensity = 1f; t.ambient = new Color(0.55f, 0.45f, 0.42f);
                t.particles = Ambient.Dust; t.particleColors = new[] { C("#E8B07A"), C("#C88A55") }; t.birds = true;
                break;
            case 7: // Magaluf de noche
                t.skyTop = C("#0B0A2A"); t.skyHorizon = C("#3A1F5C"); t.groundTop = C("#1C2350"); t.groundBottom = C("#070A1C");
                t.light = new Color(0.62f, 0.66f, 1f); t.lightIntensity = 0.55f; t.ambient = new Color(0.28f, 0.26f, 0.4f); t.night = true;
                t.particles = Ambient.Fireflies; t.particleColors = new[] { C("#FF4FD8"), C("#3FF2FF"), C("#FFFFFF") };
                break;
            case 8: // Feria de Abril de noche
                t.skyTop = C("#1A1240"); t.skyHorizon = C("#5C2E6B"); t.groundTop = C("#B98345"); t.groundBottom = C("#5E3B1A");
                t.light = new Color(0.8f, 0.72f, 1f); t.lightIntensity = 0.6f; t.ambient = new Color(0.32f, 0.28f, 0.4f); t.night = true;
                t.particles = Ambient.Confetti; t.particleColors = new[] { C("#FF6B6B"), C("#FFD84A"), C("#7CF08A"), C("#7AC8FF"), C("#E86A92") };
                break;
            case 9: // Lloret de Mar al atardecer
                t.skyTop = C("#FF6F61"); t.skyHorizon = C("#FFC07A"); t.groundTop = C("#3A6FA8"); t.groundBottom = C("#173E6A");
                t.light = new Color(1f, 0.8f, 0.65f); t.lightIntensity = 1f; t.ambient = new Color(0.55f, 0.46f, 0.46f); t.birds = true;
                break;
            default: // El Congreso, tormenta política
                t.skyTop = C("#2A0A16"); t.skyHorizon = C("#B8402A"); t.groundTop = C("#6B5A50"); t.groundBottom = C("#2E2622");
                t.light = new Color(1f, 0.7f, 0.6f); t.lightIntensity = 0.8f; t.ambient = new Color(0.4f, 0.32f, 0.34f);
                t.particles = Ambient.Papers; t.particleColors = new[] { C("#FFFFFF"), C("#F0EAD6") }; t.night = true;
                break;
        }
        return t;
    }

    private static Color C(string hex) => VoxelLib.Hex(hex);

    public void SetTheme(int room)
    {
        if (cam == null || root == null) return;
        if (room == currentRoom) return;
        currentRoom = room;
        theme = GetTheme(room);

        if (!anchorSet)
        {
            cameraAnchor = cam.transform.position;
            anchorSet = true;
        }

        for (int i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);
        if (ambientParticles != null) Destroy(ambientParticles.gameObject);

        cam.backgroundColor = theme.skyHorizon;
        BuildSky();
        BuildScenery(room);
        ApplyLighting();
        if (GameSettings.AmbientEffects) BuildParticles();

        SetLayerRecursive(root, BackdropLayer);
        birdTimer = Random.Range(2f, 5f);
        lightningTimer = Random.Range(4f, 8f);
    }

    private void ApplyLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = theme.ambient;

        if (mainLight != null)
        {
            mainLight.color = theme.light;
            mainLight.intensity = theme.lightIntensity;
            mainLightBase = theme.lightIntensity;
        }

        backLight.color = Color.Lerp(theme.light, Color.white, 0.3f);
        backLight.intensity = theme.night ? 0.45f : 0.95f;

        // De noche Torrento lleva su propia luz para que siempre se vea bien
        PlayerController player = PlayerController.Instance;
        if (theme.night && player != null)
        {
            if (playerLight == null)
            {
                GameObject pl = new GameObject("PlayerLight");
                pl.transform.SetParent(player.transform, false);
                pl.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                playerLight = pl.AddComponent<Light>();
                playerLight.type = LightType.Point;
                playerLight.range = 6f;
                playerLight.shadows = LightShadows.None;
                playerLight.cullingMask = ~(1 << BackdropLayer);
            }
            playerLight.color = new Color(1f, 0.85f, 0.65f);
            playerLight.intensity = 1.4f;
            playerLight.enabled = true;
        }
        else if (playerLight != null)
        {
            playerLight.enabled = false;
        }
    }

    // ---------- Cielo ----------

    private void BuildSky()
    {
        Mesh m = new Mesh { name = "SkyGradient" };
        const float W = 80f, H = 60f;
        Color mid = Color.Lerp(theme.skyTop, theme.skyHorizon, 0.55f);
        Vector3[] v =
        {
            new Vector3(-W, H, 0), new Vector3(W, H, 0),
            new Vector3(-W, Horizon + 2.6f, 0), new Vector3(W, Horizon + 2.6f, 0),
            new Vector3(-W, Horizon, 0), new Vector3(W, Horizon, 0),
            new Vector3(-W, Horizon, 0), new Vector3(W, Horizon, 0),
            new Vector3(-W, -H, 0), new Vector3(W, -H, 0)
        };
        Color[] c =
        {
            theme.skyTop, theme.skyTop, mid, mid, theme.skyHorizon, theme.skyHorizon,
            theme.groundTop, theme.groundTop, theme.groundBottom, theme.groundBottom
        };
        int[] tris = { 0, 1, 2, 2, 1, 3, 2, 3, 4, 4, 3, 5, 6, 7, 8, 8, 7, 9 };
        m.vertices = v;
        m.colors = c;
        m.triangles = tris;
        m.RecalculateBounds();

        GameObject sky = new GameObject("Sky");
        sky.transform.SetParent(root, false);
        sky.transform.localPosition = new Vector3(0f, 0f, 900f);
        sky.AddComponent<MeshFilter>().sharedMesh = m;
        MeshRenderer r = sky.AddComponent<MeshRenderer>();
        r.sharedMaterial = VoxelLib.Sky;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        sky.AddComponent<BackdropItem>().parallax = 0f;
    }

    // ---------- Paisaje ----------

    private BackdropItem Place(Mesh mesh, float x, float y, float depth, float parallax, bool upright = true, float scale = 1f)
    {
        GameObject go = VoxelLib.SpawnLocal(mesh.name, mesh, root, new Vector3(x, y, depth));
        if (upright) go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        else go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * scale;
        BackdropItem item = go.AddComponent<BackdropItem>();
        item.parallax = parallax;
        return item;
    }

    private void Row(System.Func<int, Mesh> factory, float xFrom, float xTo, float y, float depth, float parallax, float gapMin, float gapMax, int seed, float scale = 1f)
    {
        System.Random rng = new System.Random(seed);
        float x = xFrom;
        int i = 0;
        while (x < xTo)
        {
            Mesh m = factory(i);
            float w = m.bounds.size.x * scale;
            Place(m, x + w * 0.5f, y + (float)rng.NextDouble() * 0.08f, depth + (float)rng.NextDouble() * 6f, parallax, true, scale);
            x += w + Mathf.Lerp(gapMin, gapMax, (float)rng.NextDouble());
            i++;
        }
    }

    private void Clouds(int count, float yMin, float yMax, Color? tint = null)
    {
        for (int i = 0; i < count; i++)
        {
            BackdropItem c = Place(VoxelModels.Cloud(i % 5), Random.Range(-13f, 13f), Random.Range(yMin, yMax), 500f + i * 8f, 0.03f);
            c.drift = Random.Range(0.08f, 0.22f);
            c.wrap = 15f;
            if (tint.HasValue) Tint(c.gameObject, tint.Value);
        }
    }

    private void Tint(GameObject go, Color c)
    {
        MeshRenderer r = go.GetComponent<MeshRenderer>();
        if (r == null) return;
        MaterialPropertyBlock b = new MaterialPropertyBlock();
        b.SetColor("_Color", c);
        r.SetPropertyBlock(b);
    }

    private void Sea(float depth, int caps, int boats)
    {
        for (int i = 0; i < caps; i++)
        {
            BackdropItem w = Place(VoxelModels.WaveCap(), Random.Range(-13f, 13f), Random.Range(-3.8f, Horizon - 0.35f), depth + i, 0.12f);
            w.bobAmplitude = 0.04f;
            w.bobSpeed = Random.Range(1f, 2f);
            w.drift = Random.Range(-0.05f, 0.05f);
            w.wrap = 14f;
            if (theme.night) Tint(w.gameObject, new Color(0.45f, 0.5f, 0.8f));
        }
        for (int i = 0; i < boats; i++)
        {
            BackdropItem b = Place(VoxelModels.Sailboat(i), Random.Range(-11f, 11f), Random.Range(Horizon - 1.4f, Horizon - 0.5f), depth - 10f - i, 0.1f, true, 0.8f);
            b.bobAmplitude = 0.03f;
            b.bobSpeed = 1.3f;
            b.drift = Random.Range(0.04f, 0.1f) * (Random.value < 0.5f ? -1f : 1f);
            b.wrap = 14f;
        }
    }

    private void SandStrip(float depth, Color color)
    {
        VoxelModel v = new VoxelModel { jitter = 0.05f };
        v.Box(0, 0, 0, 280, 1, 3, color);
        Mesh m = VoxelLib.Cached("sand" + ColorUtility.ToHtmlStringRGB(color), () => v.ToMesh(0.1f));
        Place(m, 0f, Horizon - 0.25f, depth, 0.08f);
    }

    private void BuildScenery(int room)
    {
        bool night = theme.night;
        switch (room)
        {
            case 1: // San Fermín: casas de colores con balcones y campanarios
                Place(VoxelModels.Sun(false), 5.5f, 3.3f, 600f, 0.02f, false);
                Clouds(4, 2.2f, 3.8f);
                Row(i => VoxelModels.Hill(i, C("#86B35A"), C("#6E9448")), -16f, 16f, Horizon - 0.4f, 420f, 0.05f, -2f, 0f, 1);
                Row(i => i % 4 == 2 ? VoxelModels.Tower(i) : VoxelModels.House(i + 1, night), -14f, 14f, Horizon - 0.2f, 300f, 0.08f, -0.15f, 0.25f, 11);
                break;

            case 2: // Plaza de toros con casas alrededor
                Place(VoxelModels.Sun(false), -5f, 3.4f, 600f, 0.02f, false);
                Clouds(3, 2.4f, 3.8f);
                Place(VoxelModels.Bullring(), 0f, Horizon - 0.3f, 300f, 0.08f);
                Row(i => VoxelModels.House(i + 20, night), -15f, -4.2f, Horizon - 0.15f, 330f, 0.07f, 0f, 0.2f, 2);
                Row(i => VoxelModels.House(i + 40, night), 4.2f, 15f, Horizon - 0.15f, 330f, 0.07f, 0f, 0.2f, 3);
                break;

            case 3: // Las Ramblas: ensanche y la basílica de las torres
                Place(VoxelModels.Sun(false), -6f, 3.5f, 600f, 0.02f, false);
                Clouds(4, 2.4f, 3.9f);
                Place(VoxelModels.Spires(), 3.5f, Horizon - 0.2f, 360f, 0.06f);
                Row(i => VoxelModels.Eixample(i, night), -15f, 15f, Horizon - 0.25f, 300f, 0.08f, 0.05f, 0.3f, 4);
                break;

            case 4: // Benidorm: rascacielos, playa y sol con gafas
                Place(VoxelModels.Sun(true), 6f, 3.2f, 600f, 0.02f, false);
                Clouds(2, 2.8f, 3.9f);
                Row(i => i == 3 ? VoxelModels.Hotel(i, "HOTEL", false) : VoxelModels.Skyscraper(i, false), -15f, 15f, Horizon - 0.1f, 320f, 0.07f, 0.1f, 0.6f, 5);
                SandStrip(280f, C("#F2D9A0"));
                Row(i => VoxelModels.Palm(i), -14f, 14f, Horizon - 0.3f, 260f, 0.09f, 1.2f, 3f, 6);
                Row(i => VoxelModels.Umbrella(i), -13f, 13f, Horizon - 0.32f, 255f, 0.09f, 1.5f, 3.5f, 7, 0.9f);
                Sea(200f, 22, 3);
                break;

            case 5: // La Mancha: molinos, castillo y olivos
                Place(VoxelModels.Sun(false), 5f, 3.4f, 600f, 0.02f, false);
                Clouds(3, 2.6f, 3.9f);
                Row(i => VoxelModels.Hill(i + 5, C("#D9B860"), C("#B8943F")), -16f, 16f, Horizon - 0.45f, 420f, 0.05f, -2f, 0f, 8);
                Place(VoxelModels.Castle(), -6.5f, Horizon - 0.1f, 380f, 0.06f);
                for (int i = 0; i < 5; i++)
                {
                    float x = -2.5f + i * 3.2f + Random.Range(-0.4f, 0.4f);
                    Place(VoxelModels.WindmillBody(), x, Horizon - 0.15f, 320f + i * 3f, 0.08f);
                    BackdropItem sails = Place(VoxelModels.WindmillSails(), x, Horizon - 0.15f + 1.75f, 320f + i * 3f, 0.08f, true);
                    sails.transform.position += sails.transform.rotation * new Vector3(0f, 0f, -0.75f);
                    sails.Rebase();
                    sails.spin = Random.Range(25f, 45f);
                }
                for (int r = 0; r < 4; r++)
                    Row(i => VoxelModels.OliveTree(i + r * 7), -14f, 14f, Horizon - 1f - r * 0.9f, 220f - r * 5f, 0.1f + r * 0.01f, 0.5f, 1f, 30 + r);
                break;

            case 6: // La A-7: autovía, toro en la colina y coches
                Place(VoxelModels.Sun(false), -4f, 1.9f, 600f, 0.02f, false, 1.3f);
                Clouds(3, 2.4f, 3.8f, new Color(1f, 0.8f, 0.7f));
                Row(i => VoxelModels.Hill(i + 9, C("#B07A48"), C("#8E5E36")), -16f, 16f, Horizon - 0.4f, 420f, 0.05f, -2f, 0f, 9);
                Place(VoxelModels.BullSilhouette(), 4.5f, Horizon + 0.55f, 390f, 0.05f);
                BuildRoad(250f);
                Place(VoxelModels.RoadSign("A-7"), -6f, Horizon - 0.25f, 240f, 0.1f);
                Place(VoxelModels.RoadSign("SONRIA"), 7f, Horizon - 0.25f, 240f, 0.1f);
                break;

            case 7: // Magaluf: hoteles, neones y luna
                Place(VoxelModels.Moon(), -5.5f, 3.3f, 600f, 0.02f, false);
                Row(i => i == 2 ? VoxelModels.Hotel(i, "MAGALUF", true) : i % 3 == 0 ? VoxelModels.Hotel(i, i % 2 == 0 ? "BAR" : "DISCO", true) : VoxelModels.Skyscraper(i + 10, true),
                    -15f, 15f, Horizon - 0.1f, 320f, 0.07f, 0.1f, 0.5f, 12);
                Row(i => VoxelModels.Palm(i + 10), -14f, 14f, Horizon - 0.3f, 260f, 0.09f, 1.5f, 3.5f, 13);
                Sea(200f, 18, 0);
                break;

            case 8: // Feria de Abril: portada iluminada y casetas
                Place(VoxelModels.Moon(), 6f, 3.4f, 600f, 0.02f, false);
                Place(VoxelModels.Portada(), 0f, Horizon - 0.2f, 320f, 0.07f);
                Row(i => VoxelModels.Caseta(i), -15f, -2.0f, Horizon - 0.2f, 300f, 0.08f, 0.05f, 0.2f, 14);
                Row(i => VoxelModels.Caseta(i + 7), 2.0f, 15f, Horizon - 0.2f, 300f, 0.08f, 0.05f, 0.2f, 15);
                break;

            case 9: // Lloret: castillo en el acantilado, playa y barcos
                Place(VoxelModels.Sun(true), 4f, 2.2f, 600f, 0.02f, false, 1.2f);
                Clouds(3, 2.6f, 3.9f, new Color(1f, 0.82f, 0.75f));
                Place(VoxelModels.Hill(21, C("#7A8F4A"), C("#8C6A4A")), -6f, Horizon - 0.35f, 400f, 0.05f);
                Place(VoxelModels.Castle(), -6f, Horizon + 0.75f, 390f, 0.05f, true, 0.8f);
                Row(i => VoxelModels.House(i + 60, false), 1f, 15f, Horizon - 0.15f, 320f, 0.07f, 0f, 0.2f, 16);
                SandStrip(280f, C("#E8C890"));
                Row(i => VoxelModels.Umbrella(i + 5), -13f, 13f, Horizon - 0.32f, 255f, 0.09f, 1.5f, 3.5f, 17, 0.9f);
                Sea(200f, 20, 4);
                break;

            default: // El Congreso, bajo una tormenta
                Clouds(6, 2.2f, 3.9f, new Color(0.45f, 0.38f, 0.42f));
                Place(VoxelModels.Congreso(), 0f, Horizon - 0.2f, 320f, 0.07f);
                Row(i => VoxelModels.Eixample(i + 30, true), -15f, -2.8f, Horizon - 0.2f, 340f, 0.07f, 0.05f, 0.3f, 18);
                Row(i => VoxelModels.Eixample(i + 40, true), 2.8f, 15f, Horizon - 0.2f, 340f, 0.07f, 0.05f, 0.3f, 19);
                break;
        }
    }

    private void BuildRoad(float depth)
    {
        Mesh road = VoxelLib.Cached("road", () =>
        {
            VoxelModel v = new VoxelModel { jitter = 0.03f };
            v.Box(0, 0, 0, 300, 0, 9, C("#3A3A3E"));
            v.Box(0, 1, 0, 300, 1, 0, C("#9AA0A6"));
            for (int x = 0; x < 300; x += 6) v.Box(x, 1, 5, x + 2, 1, 5, C("#F2F2F2"));
            return v.ToMesh(0.1f);
        });
        Place(road, 0f, Horizon - 0.75f, depth, 0.1f);
        for (int i = 0; i < 7; i++)
        {
            BackdropItem car = Place(VoxelModels.Car(i), Random.Range(-14f, 14f), Horizon - 0.68f + (i % 2) * 0.12f, depth - 2f - (i % 2) * 2f, 0.1f);
            car.drift = (i % 2 == 0 ? 1f : 1.6f) * Random.Range(1.2f, 2.2f);
            car.wrap = 15f;
        }
    }

    // ---------- Partículas ambientales ----------

    private void BuildParticles()
    {
        if (theme.particles == Ambient.None) return;

        GameObject go = new GameObject("AmbientParticles");
        go.transform.SetParent(cam.transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 9f);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 160;
        main.startSpeed = 0f;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        Gradient g = new Gradient();
        Color[] cols = theme.particleColors ?? new[] { Color.white };
        GradientColorKey[] ck = new GradientColorKey[Mathf.Min(cols.Length, 8)];
        for (int i = 0; i < ck.Length; i++) ck[i] = new GradientColorKey(cols[i], ck.Length == 1 ? 0f : i / (float)(ck.Length - 1));
        g.SetKeys(ck, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        g.mode = GradientMode.Fixed;
        main.startColor = new ParticleSystem.MinMaxGradient(g) { mode = ParticleSystemGradientMode.RandomColor };

        var emission = ps.emission;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        var noise = ps.noise;
        noise.enabled = true;
        var rot = ps.rotationOverLifetime;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                     new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.1f), new GradientAlphaKey(0.9f, 0.8f), new GradientAlphaKey(0f, 1f) });

        switch (theme.particles)
        {
            case Ambient.Confetti:
            case Ambient.Leaves:
            case Ambient.Papers:
                bool papers = theme.particles == Ambient.Papers;
                main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 9f);
                main.startSize = papers ? new ParticleSystem.MinMaxCurve(0.12f, 0.2f) : new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
                emission.rateOverTime = papers ? 3f : 7f;
                shape.position = new Vector3(0f, 5f, 0f);
                shape.scale = new Vector3(18f, 0.5f, 2f);
                vel.x = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
                vel.y = new ParticleSystem.MinMaxCurve(-1.4f, -0.8f);
                vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                noise.strength = 0.6f;
                noise.frequency = 0.4f;
                rot.enabled = true;
                rot.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
                break;

            case Ambient.Dust:
                main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
                emission.rateOverTime = 8f;
                shape.position = new Vector3(-9f, 0f, 0f);
                shape.scale = new Vector3(0.5f, 9f, 2f);
                vel.x = new ParticleSystem.MinMaxCurve(1.5f, 3f);
                vel.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.3f);
                vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                noise.strength = 0.8f;
                noise.frequency = 0.6f;
                break;

            case Ambient.Fireflies:
                main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
                emission.rateOverTime = 10f;
                shape.position = Vector3.zero;
                shape.scale = new Vector3(18f, 10f, 2f);
                vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
                vel.y = new ParticleSystem.MinMaxCurve(-0.05f, 0.25f);
                vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                noise.strength = 0.3f;
                noise.frequency = 0.8f;
                fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                             new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0.2f, 0.5f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
                break;
        }
        col.color = new ParticleSystem.MinMaxGradient(fade);

        ParticleSystemRenderer pr = go.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = VoxelLib.UnlitAlpha;
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        pr.receiveShadows = false;

        ps.Play();
        ambientParticles = ps;
    }

    public void SetAmbientEffects(bool on)
    {
        if (theme == null) return;
        if (!on && ambientParticles != null) Destroy(ambientParticles.gameObject);
        else if (on && ambientParticles == null) BuildParticles();
    }

    // ---------- Animación ----------

    void LateUpdate()
    {
        if (cam == null || theme == null) return;

        Vector3 delta = cam.transform.InverseTransformDirection(cam.transform.position - cameraAnchor);
        BackdropItem.CameraDelta = new Vector2(delta.x, delta.y);

        if (theme.birds)
        {
            birdTimer -= Time.deltaTime;
            if (birdTimer <= 0f)
            {
                birdTimer = Random.Range(6f, 14f);
                SpawnFlock();
            }
        }

        if (currentRoom == 10)
        {
            lightningTimer -= Time.deltaTime;
            if (lightningTimer <= 0f)
            {
                lightningTimer = Random.Range(7f, 13f);
                StartCoroutine(Lightning());
            }
        }
    }

    private void SpawnFlock()
    {
        float dir = Random.value < 0.5f ? 1f : -1f;
        float y = Random.Range(2.3f, 3.6f);
        int n = Random.Range(3, 6);
        for (int i = 0; i < n; i++)
        {
            BackdropItem b = Place(VoxelModels.Bird(), -14f * dir - i * 0.5f * dir, y - Mathf.Abs(i - n / 2) * 0.18f, 450f, 0.03f, false, 0.9f);
            b.drift = 1.6f * dir;
            b.flap = Random.Range(7f, 10f);
            b.lifetime = 20f;
            SetLayerRecursive(b.transform, BackdropLayer);
        }
    }

    private System.Collections.IEnumerator Lightning()
    {
        BackdropItem bolt = Place(VoxelModels.Lightning(), Random.Range(-10f, 10f), Horizon + 0.2f, 450f, 0.03f, false);
        SetLayerRecursive(bolt.transform, BackdropLayer);
        bool flash = GameSettings.Flashes;
        if (flash && mainLight != null) mainLight.intensity = mainLightBase * 2.2f;
        if (flash) backLight.intensity = 2f;
        Sfx.Play(SfxType.Thunder, 0.6f);
        yield return new WaitForSeconds(0.08f);
        if (mainLight != null) mainLight.intensity = mainLightBase;
        yield return new WaitForSeconds(0.06f);
        if (flash && mainLight != null) mainLight.intensity = mainLightBase * 1.6f;
        yield return new WaitForSeconds(0.08f);
        if (mainLight != null) mainLight.intensity = mainLightBase;
        backLight.intensity = 0.45f;
        if (bolt != null) Destroy(bolt.gameObject);
    }

    private static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++) SetLayerRecursive(t.GetChild(i), layer);
    }
}
