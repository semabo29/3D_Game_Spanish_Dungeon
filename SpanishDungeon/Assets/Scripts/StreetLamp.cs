using UnityEngine;

// Farolas de las salas: sustituimos el modelo antiguo (demasiado alto, tapaba la sala)
// por una farola fernandina voxel más baja, con el farol encendido y, de noche, luz de verdad.
public class StreetLamp : MonoBehaviour
{
    private Light lamp;
    private float baseIntensity;
    private float seed;

    private static Color C(string hex) => VoxelLib.Hex(hex);

    public static Mesh Model() => VoxelLib.Cached("farola_fernandina", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.04f };
        Color iron = C("#1F2B26"), ironLight = C("#2F4038"), frame = C("#141A17");
        Color glass = VoxelModel.Glow(C("#FFD27A")), glassHot = VoxelModel.Glow(C("#FFF1C2"));

        // Pie
        v.Box(-2, 0, -2, 2, 1, 2, iron);
        v.Box(-1, 2, -1, 1, 4, 1, ironLight);
        // Fuste con anillas
        v.Box(-1, 5, -1, 1, 21, 1, iron);
        v.Box(-2, 10, -2, 2, 10, 2, ironLight);
        v.Box(-2, 17, -2, 2, 17, 2, ironLight);
        v.Box(-2, 22, -2, 2, 22, 2, iron);
        // Farol
        v.Box(-3, 23, -3, 3, 23, 3, frame);
        v.Box(-2, 24, -2, 2, 29, 2, glass);
        v.Box(-1, 25, -1, 1, 28, 1, glassHot);
        v.Box(-3, 24, -3, -3, 29, -3, frame);
        v.Box(3, 24, -3, 3, 29, -3, frame);
        v.Box(-3, 24, 3, -3, 29, 3, frame);
        v.Box(3, 24, 3, 3, 29, 3, frame);
        // Tejadillo y remate
        v.Box(-3, 30, -3, 3, 30, 3, frame);
        v.Box(-2, 31, -2, 2, 31, 2, iron);
        v.Box(-1, 32, -1, 1, 32, 1, iron);
        v.Set(0, 33, 0, ironLight);
        return v.ToMesh(0.05f);
    });

    // Se llama al decorar la sala con cada farola que ha colocado el mapa
    public static void Restyle(Transform original)
    {
        if (original == null || original.GetComponent<StreetLamp>() != null) return;
        foreach (Renderer r in original.GetComponentsInChildren<Renderer>()) r.enabled = false;

        Vector3 floor = new Vector3(original.position.x, 0f, original.position.z);
        GameObject model = VoxelLib.Spawn("FarolaVoxel", Model(), null, floor, Quaternion.Euler(0f, 45f, 0f), true);
        model.transform.SetParent(original, true);

        StreetLamp sl = original.gameObject.AddComponent<StreetLamp>();
        sl.seed = Random.value * 10f;
        if (Backdrop.IsNight)
        {
            GameObject lg = new GameObject("Luz");
            lg.transform.SetParent(original, false);
            lg.transform.position = floor + Vector3.up * 1.35f;
            sl.lamp = lg.AddComponent<Light>();
            sl.lamp.type = LightType.Point;
            sl.lamp.color = new Color(1f, 0.78f, 0.45f);
            sl.lamp.range = 3.4f;
            sl.baseIntensity = 1.5f;
            sl.lamp.intensity = sl.baseIntensity;
            sl.lamp.shadows = LightShadows.None;
            sl.lamp.cullingMask = ~(1 << Backdrop.BackdropLayer);
        }
    }

    void Update()
    {
        if (lamp == null) return;
        // Parpadeo suave, como de bombilla vieja (fijo si los destellos están desactivados)
        float flicker = GameSettings.Flashes
            ? Mathf.PerlinNoise(seed, Time.time * 3f) * 0.35f + (Mathf.PerlinNoise(seed + 5f, Time.time * 20f) > 0.92f ? -0.6f : 0f)
            : 0.15f;
        lamp.intensity = baseIntensity * (0.8f + flicker);
    }
}
