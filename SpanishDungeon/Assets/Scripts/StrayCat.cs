using UnityEngine;

// Gatos callejeros: dormitan en la sala, respiran y salen por patas si te acercas
public class StrayCat : MonoBehaviour
{
    private Transform model;
    private Vector3 baseScale;
    private bool fleeing;
    private Vector3 velocity;
    private float life = 1.4f;
    private float seed;

    private static Color C(string hex) => VoxelLib.Hex(hex);

    private static readonly string[] Lines = { "¡Miau!", "¡Fffff!", "¡Mrrraw!", "¡Miau, pesao!", "Prrr..." };

    public static Mesh Model(int variant) => VoxelLib.Cached("gato" + variant, () =>
    {
        Color fur, stripe;
        switch (variant)
        {
            case 1: fur = C("#1C1C22"); stripe = C("#2A2A33"); break;       // negro
            case 2: fur = C("#E9E6DF"); stripe = C("#9A938A"); break;       // blanco y gris
            default: fur = C("#E8902E"); stripe = C("#B8611A"); break;      // atigrado naranja
        }
        VoxelModel v = new VoxelModel { jitter = 0.06f };
        // Cuerpo y rayas
        v.Box(-2, 2, -4, 2, 5, 3, fur);
        for (int z = -3; z <= 2; z += 2) v.Paint(-2, 5, z, 2, 5, z, stripe);
        // Patas
        v.Box(-2, 0, -4, -1, 1, -3, fur);
        v.Box(1, 0, -4, 2, 1, -3, fur);
        v.Box(-2, 0, 2, -1, 1, 3, fur);
        v.Box(1, 0, 2, 2, 1, 3, fur);
        // Cabeza, orejas, ojos y hocico
        v.Box(-2, 5, 4, 2, 8, 7, fur);
        v.Set(-2, 9, 5, fur).Set(2, 9, 5, fur).Set(-2, 9, 6, stripe).Set(2, 9, 6, stripe);
        Color eye = VoxelModel.Glow(C(variant == 1 ? "#FFD23A" : "#8CF05A"));
        v.Set(-1, 7, 8, eye).Set(1, 7, 8, eye);
        v.Set(0, 6, 8, C("#FF8FA8"));
        // Cola levantada
        v.Box(0, 5, -5, 0, 10, -5, fur);
        v.Set(0, 11, -4, stripe);
        return v.ToMesh(0.04f);
    });

    public static void Create(Transform parent, Vector3 objectPos)
    {
        GameObject go = new GameObject("Gato");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(objectPos.x + Random.Range(-0.2f, 0.2f), 0f, objectPos.z + Random.Range(-0.2f, 0.2f));
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        StrayCat cat = go.AddComponent<StrayCat>();
        GameObject m = VoxelLib.Spawn("Model", Model(Random.Range(0, 3)), go.transform, go.transform.position, go.transform.rotation);
        cat.model = m.transform;
        cat.baseScale = m.transform.localScale;
        cat.seed = Random.value * 10f;
    }

    void Update()
    {
        if (model == null) return;
        if (!fleeing)
        {
            float breath = Mathf.Sin((Time.time + seed) * 2.2f) * 0.04f;
            model.localScale = new Vector3(baseScale.x * (1f - breath * 0.5f), baseScale.y * (1f + breath), baseScale.z);

            PlayerController player = PlayerController.Instance;
            if (player != null)
            {
                Vector3 d = transform.position - player.transform.position;
                d.y = 0f;
                if (d.magnitude < 1.8f) Flee(d);
            }
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
            float hop = Mathf.Abs(Mathf.Sin(Time.time * 22f)) * 0.12f;
            model.localPosition = Vector3.up * hop;
            life -= Time.deltaTime;
            if (life < 0.35f) model.localScale = baseScale * Mathf.Max(0.01f, life / 0.35f);
            if (life <= 0f) Destroy(gameObject);
        }
    }

    private void Flee(Vector3 away)
    {
        fleeing = true;
        if (away.sqrMagnitude < 0.01f) away = Vector3.right;
        velocity = away.normalized * 4.5f;
        transform.rotation = Quaternion.LookRotation(away.normalized);
        transform.SetParent(null, true);
        model.localScale = baseScale;
        model.localRotation = Quaternion.identity;
        Sfx.Play(SfxType.Meow, 0.55f, Random.Range(0.9f, 1.25f));
        GameFeel.FloatingText(transform.position + Vector3.up * 0.4f, Lines[Random.Range(0, Lines.Length)], new Color(1f, 0.75f, 0.4f), 3f);
        GameFeel.Dust(transform.position, new Color(0.8f, 0.75f, 0.65f), 4);
        Achievements.OnCat();
    }
}
