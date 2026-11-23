using UnityEngine;
using System;

// Sombreros para Torrento. Se desbloquean consiguiendo logros y se eligen en Opciones.
public static class Hats
{
    public struct Def
    {
        public string name;
        public int required;     // logros necesarios
        public string joke;
        public Func<Mesh> build;
    }

    private static Color C(string hex) => VoxelLib.Hex(hex);
    private static Color G(string hex) => VoxelModel.Glow(VoxelLib.Hex(hex));

    public static readonly Def[] All =
    {
        new Def { name = "Ninguno", required = 0, joke = "Torrento a pelo, como Dios lo trajo al mundo.", build = null },
        new Def { name = "Boina", required = 1, joke = "La de los domingos. Huele a puro.", build = Boina },
        new Def { name = "Pañuelo rojo", required = 2, joke = "Del 6 al 14 de julio. Bueno, todo el año.", build = Panuelo },
        new Def { name = "Sombrero cordobés", required = 4, joke = "Elegancia andaluza de ala ancha.", build = Cordobes },
        new Def { name = "Sombrero de paja", required = 6, joke = "Para la siega, la playa y el chiringuito.", build = Paja },
        new Def { name = "Casco de obra", required = 8, joke = "Para supervisar obras con las manos atrás.", build = Casco },
        new Def { name = "Montera", required = 10, joke = "Prestada de un torero. Sin devolución.", build = Montera },
        new Def { name = "Peineta", required = 13, joke = "Para ir de Feria como Dios manda.", build = Peineta },
        new Def { name = "Flotador", required = 16, joke = "Confiscado a un guiri. Por si acaso.", build = Flotador },
        new Def { name = "Corona", required = -1, joke = "Para quien tiene TODOS los logros. Ni una cacería menos.", build = Corona },
    };

    // required < 0 significa "todos los logros"
    public static int Required(int i) => All[i].required < 0 ? Achievements.All.Count : All[i].required;

    public static bool IsUnlocked(int i) => i >= 0 && i < All.Length && Achievements.UnlockedCount >= Required(i);

    public static int Next(int current, int delta)
    {
        int n = All.Length;
        int i = current;
        for (int step = 0; step < n; step++)
        {
            i = (i + (delta >= 0 ? 1 : -1) + n) % n;
            if (IsUnlocked(i)) return i;
        }
        return current;
    }

    public static string Hint(int current)
    {
        string text = All[current].joke;
        for (int i = 0; i < All.Length; i++)
        {
            if (Achievements.UnlockedCount < Required(i))
            {
                text += "  (Siguiente: " + All[i].name + " con " + Required(i) + " logros)";
                break;
            }
        }
        return text;
    }

    public static Mesh Mesh(int i)
    {
        if (i <= 0 || i >= All.Length || All[i].build == null) return null;
        return VoxelLib.Cached("hat" + i, All[i].build);
    }

    private const float Size = 0.04f;

    // Anillo de voxeles (para bandas y flotadores)
    private static void Ring(VoxelModel v, float radius, float tube, int y0, int y1, Color c, Color? alt = null)
    {
        int r = Mathf.CeilToInt(radius + tube);
        for (int x = -r; x <= r; x++)
            for (int z = -r; z <= r; z++)
            {
                float d = Mathf.Sqrt(x * x + z * z);
                if (Mathf.Abs(d - radius) > tube) continue;
                Color col = c;
                if (alt.HasValue && ((Mathf.Atan2(z, x) + Mathf.PI) / (Mathf.PI * 2f) * 8f) % 2f < 1f) col = alt.Value;
                for (int y = y0; y <= y1; y++) v.Set(x, y, z, col);
            }
    }

    private static Mesh Boina()
    {
        VoxelModel v = new VoxelModel { jitter = 0.04f };
        Color c = C("#1E2433");
        v.CylinderY(0, 0, 0, 5.6f, 1, c);
        v.CylinderY(0, 1, 0, 4.6f, 1, c);
        v.Box(0, 2, 0, 0, 2, 0, C("#2C3448"));
        return v.ToMesh(Size);
    }

    private static Mesh Panuelo()
    {
        VoxelModel v = new VoxelModel { jitter = 0.05f };
        Color red = C("#D0101F");
        Ring(v, 4.6f, 0.6f, 0, 1, red);
        // Nudo y puntas por detrás
        v.Box(-1, 0, -6, 1, 1, -5, red);
        v.Box(-2, -2, -7, -1, -1, -6, red);
        v.Box(1, -3, -7, 2, -1, -6, red);
        return v.ToMesh(Size);
    }

    private static Mesh Cordobes()
    {
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Color black = C("#15151A");
        v.CylinderY(0, 0, 0, 8f, 1, black);
        v.CylinderY(0, 1, 0, 4.6f, 4, black);
        Ring(v, 4.6f, 0.5f, 1, 1, C("#3A3A44"));
        return v.ToMesh(Size);
    }

    private static Mesh Paja()
    {
        VoxelModel v = new VoxelModel { jitter = 0.09f };
        Color straw = C("#E8C15A"), dark = C("#C99A35");
        for (int x = -8; x <= 8; x++)
            for (int z = -8; z <= 8; z++)
            {
                float d = Mathf.Sqrt(x * x + z * z);
                if (d <= 8.4f) v.Set(x, d > 7f ? -1 : 0, z, (x + z) % 2 == 0 ? straw : dark);
            }
        v.Sphere(0, 2, 0, 4.4f, straw, 0.7f);
        v.ClearBox(-6, -2, -6, 6, 0, 6);
        v.CylinderY(0, 0, 0, 4.4f, 1, straw);
        Ring(v, 4.5f, 0.6f, 1, 1, C("#C8102E"));
        return v.ToMesh(Size);
    }

    private static Mesh Casco()
    {
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Color yellow = C("#FFC21A");
        v.Sphere(0, 0, 0, 5.2f, yellow, 0.85f);
        v.ClearBox(-7, -7, -7, 7, -1, 7);
        v.Box(-4, 0, 4, 4, 0, 7, yellow); // visera
        v.Box(0, 1, -5, 0, 5, 5, C("#FFD54A")); // cresta
        return v.ToMesh(Size);
    }

    private static Mesh Montera()
    {
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Color black = C("#121212");
        v.Box(-5, 0, -3, 5, 2, 3, black);
        v.Sphere(-5, 2, 0, 2.4f, black);
        v.Sphere(5, 2, 0, 2.4f, black);
        v.Box(-1, 3, -1, 1, 3, 1, C("#2A2A2A"));
        return v.ToMesh(Size);
    }

    private static Mesh Peineta()
    {
        VoxelModel v = new VoxelModel { jitter = 0.06f };
        Color shell = C("#6B3A1E"), light = C("#9C5A2E");
        string[] rows =
        {
            "..XX.XXX.XX..",
            ".XXXXXXXXXXX.",
            "XXOXXXOXXXOXX",
            "XXXXXXXXXXXXX",
            ".XXXXXXXXXXX.",
            "..XXXXXXXXX..",
            "...XXXXXXX...",
        };
        var palette = new System.Collections.Generic.Dictionary<char, Color> { { 'X', shell }, { 'O', light } };
        v.Extrude(rows, palette, -6, 0, -4, 1);
        v.Box(-3, 0, -3, 3, 0, 2, C("#1A1A1A")); // mantilla
        v.Set(4, 1, 2, C("#D0101F")).Set(5, 1, 2, C("#D0101F")).Set(4, 2, 2, C("#D0101F")); // clavel
        return v.ToMesh(Size);
    }

    private static Mesh Flotador()
    {
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Ring(v, 5.2f, 1.6f, 0, 2, C("#FF6FA8"), C("#FFFFFF"));
        v.Box(0, 3, -2, 0, 4, -1, C("#FFE14A")); // patito
        v.Box(0, 4, 0, 0, 4, 0, C("#FF9A1A"));
        return v.ToMesh(Size);
    }

    private static Mesh Corona()
    {
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Color gold = G("#FFC83A");
        Ring(v, 4.6f, 0.6f, 0, 1, gold);
        for (int k = 0; k < 8; k++)
        {
            float a = k / 8f * Mathf.PI * 2f;
            int x = Mathf.RoundToInt(Mathf.Cos(a) * 4.6f), z = Mathf.RoundToInt(Mathf.Sin(a) * 4.6f);
            v.Box(x, 2, z, x, 3, z, gold);
            v.Set(x, 4, z, k % 2 == 0 ? G("#E0203A") : G("#3A8CFF"));
        }
        return v.ToMesh(Size);
    }
}

// Pone el sombrero elegido sobre la cabeza del personaje y lo hace seguir
// la animación (saltitos, aplastamiento, parpadeo de invulnerabilidad).
[DefaultExecutionOrder(200)]
public class HatWearer : MonoBehaviour
{
    private GameObject hat;
    private Transform anchor;
    private Renderer anchorRenderer;
    private Renderer hatRenderer;
    private Vector3 localOffset;
    private Quaternion anchorBaseRot;
    private float anchorBaseScaleY;
    private Quaternion hatBaseRot;

    void Start()
    {
        GameSettings.Changed += Rebuild;
        Rebuild();
    }

    void OnDestroy()
    {
        GameSettings.Changed -= Rebuild;
        if (hat != null) Destroy(hat);
    }

    private void Rebuild()
    {
        if (hat != null) Destroy(hat);
        hat = null;
        Mesh mesh = Hats.Mesh(GameSettings.Hat);
        if (mesh == null) return;

        // Elegimos el modelo visual más grande como ancla y buscamos lo alto de la cabeza
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool any = false;
        float bestVolume = -1f;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!(r is MeshRenderer || r is SkinnedMeshRenderer) || !r.enabled) continue;
            if (r.transform == transform) continue;
            if (!any) { bounds = r.bounds; any = true; } else bounds.Encapsulate(r.bounds);
            float vol = r.bounds.size.x * r.bounds.size.y * r.bounds.size.z;
            if (vol > bestVolume) { bestVolume = vol; anchor = r.transform; anchorRenderer = r; }
        }
        if (!any || anchor == null) return;

        Vector3 top = new Vector3(bounds.center.x, bounds.max.y - 0.04f, bounds.center.z);
        localOffset = anchor.InverseTransformPoint(top);
        anchorBaseRot = anchor.rotation;
        anchorBaseScaleY = Mathf.Max(0.0001f, anchor.lossyScale.y);

        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        hatBaseRot = Quaternion.LookRotation(fwd.normalized);

        hat = VoxelLib.Spawn("Sombrero", mesh, null, top, hatBaseRot, true);
        hatRenderer = hat.GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (hat == null || anchor == null) return;
        hat.transform.position = anchor.TransformPoint(localOffset);
        hat.transform.rotation = anchor.rotation * Quaternion.Inverse(anchorBaseRot) * hatBaseRot;
        float k = anchor.lossyScale.y / anchorBaseScaleY;
        hat.transform.localScale = new Vector3(1f / Mathf.Sqrt(Mathf.Max(0.2f, k)), k, 1f / Mathf.Sqrt(Mathf.Max(0.2f, k)));
        if (hatRenderer != null && anchorRenderer != null) hatRenderer.enabled = anchorRenderer.enabled && anchor.gameObject.activeInHierarchy;
    }

    void OnDisable()
    {
        if (hat != null) hat.SetActive(false);
    }

    void OnEnable()
    {
        if (hat != null) hat.SetActive(true);
    }
}
