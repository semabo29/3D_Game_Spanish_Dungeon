using UnityEngine;

// Gaviotas ladronas de Benidorm y Lloret: de vez en cuando una se lanza en picado a la
// casilla donde estás para robarte una moneda. Se marca la casilla y se oye el graznido,
// así que si te mueves a tiempo la esquivas.
public class SeagullThief : MonoBehaviour
{
    private static Color C(string hex) => VoxelLib.Hex(hex);

    private static readonly string[] Taunts = { "¡Mía!", "¡Gracias, guapo!", "¡Al buche!", "¡Ja, ja, ja!" };
    private static readonly string[] Misses = { "¡Fallé!", "¡Aaargh!", "¡La próxima es mía!" };

    public static Mesh Model() => VoxelLib.Cached("gaviota", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.04f };
        Color white = C("#F4F4F2"), grey = C("#A9B2BC"), black = C("#202226"), beak = C("#F2C230");
        // Cuerpo
        v.Box(-1, 0, -4, 1, 2, 3, white);
        v.Box(-1, 1, 4, 1, 3, 5, white);           // cabeza
        v.Box(0, 2, 6, 0, 2, 7, beak);              // pico
        v.Set(0, 1, 7, C("#D0302A"));               // la manchita roja
        v.Set(-1, 3, 5, black).Set(1, 3, 5, black); // ojos
        v.Box(-1, 1, -6, 1, 1, -5, grey);           // cola
        // Alas abiertas
        v.Box(-8, 2, -2, -2, 2, 1, grey);
        v.Box(2, 2, -2, 8, 2, 1, grey);
        v.Box(-8, 2, -2, -7, 2, 1, black);
        v.Box(7, 2, -2, 8, 2, 1, black);
        return v.ToMesh(0.05f, true);
    });

    // Se crea una vez por sala; se destruye con ella
    public static void CreateSpawner(Transform mapRoot)
    {
        GameObject go = new GameObject("GaviotasLadronas");
        go.transform.SetParent(mapRoot, false);
        go.AddComponent<SeagullSpawner>();
    }

    private enum State { Circling, Diving, Leaving }

    private State state;
    private Transform model;
    private Vector3 target;
    private Vector3 start;
    private Vector3 velocity;
    private float t;
    private float diveTime = 1.25f;
    private DangerMarker marker;
    private GameObject coin;
    private float life = 3f;

    public static void Launch(Transform parent, Vector3 playerTile)
    {
        GameObject go = new GameObject("Gaviota");
        go.transform.SetParent(parent, false);
        SeagullThief g = go.AddComponent<SeagullThief>();
        g.target = new Vector3(playerTile.x, 0.9f, playerTile.z);
        g.start = g.target + new Vector3(Random.value < 0.5f ? -6f : 6f, 5f, Random.Range(-3f, 3f));
        go.transform.position = g.start;
        g.model = VoxelLib.Spawn("Model", Model(), go.transform, go.transform.position, Quaternion.identity).transform;
        g.state = State.Diving;

        g.marker = DangerMarker.Create(parent, new Vector3(playerTile.x, 0f, playerTile.z));
        g.marker.Set(0.8f, true);

        Sfx.Play(SfxType.Coo, 0.7f, 1.9f);
        GameFeel.FloatingText(g.start + Vector3.down * 0.5f, "¡KIAAA!", new Color(1f, 1f, 1f), 3.5f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (model != null)
        {
            float flap = Mathf.Sin(Time.time * (state == State.Diving ? 10f : 26f));
            model.localScale = new Vector3(1f + flap * 0.08f, 1f - flap * 0.25f, 1f);
        }

        switch (state)
        {
            case State.Diving:
            {
                t += dt / diveTime;
                float k = Mathf.Clamp01(t);
                // Curva de picado: baja deprisa al final
                Vector3 p = Vector3.Lerp(start, target, k);
                p.y = Mathf.Lerp(start.y, target.y, k * k);
                Vector3 dir = p - transform.position;
                transform.position = p;
                if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir.normalized);
                if (k >= 1f) Strike();
                break;
            }
            case State.Leaving:
            {
                velocity += Vector3.up * 3f * dt;
                transform.position += velocity * dt;
                Vector3 flat = velocity;
                if (flat.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(flat.normalized);
                life -= dt;
                if (life <= 0f) Destroy(gameObject);
                break;
            }
        }
    }

    private void Strike()
    {
        state = State.Leaving;
        if (marker != null) Destroy(marker.gameObject);
        transform.SetParent(null, true);

        Vector3 away = new Vector3(start.x - target.x, 0f, start.z - target.z).normalized * -1f;
        velocity = away * 5f + Vector3.up * 2f;

        PlayerController player = PlayerController.Instance;
        PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;
        bool hit = false;
        if (stats != null)
        {
            Vector3 d = player.transform.position - target;
            d.y = 0f;
            hit = d.magnitude < 0.6f && stats.StealCoin();
        }

        GameFeel.Dust(new Vector3(target.x, 0.05f, target.z), new Color(0.95f, 0.95f, 0.95f), 6);
        Sfx.Play(SfxType.Flap, 0.7f, 1.1f);
        if (hit)
        {
            // Se lleva la moneda en el pico
            coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(coin.GetComponent<Collider>());
            coin.transform.SetParent(transform, false);
            coin.transform.localPosition = new Vector3(0f, -0.05f, 0.35f);
            coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            coin.transform.localScale = new Vector3(0.16f, 0.02f, 0.16f);
            Renderer r = coin.GetComponent<Renderer>();
            r.sharedMaterial = VoxelLib.Lit;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor("_Color", new Color(1f, 0.8f, 0.15f));
            r.SetPropertyBlock(mpb);

            Sfx.Play(SfxType.Coo, 0.8f, 2.2f);
            GameFeel.FloatingText(target + Vector3.up * 0.8f, "-1  " + Taunts[Random.Range(0, Taunts.Length)], new Color(1f, 0.5f, 0.4f), 4f);
            if (GameHUD.Instance != null) GameHUD.Instance.ShowToast("¡Una gaviota te ha robado una moneda!", new Color(1f, 0.6f, 0.4f), 1.4f);
            Achievements.OnSeagullTheft();
        }
        else
        {
            GameFeel.FloatingText(target + Vector3.up * 0.8f, Misses[Random.Range(0, Misses.Length)], new Color(0.8f, 0.9f, 1f), 3.5f);
        }
    }

    void OnDestroy()
    {
        if (marker != null) Destroy(marker.gameObject);
    }
}

// Decide cuándo atacar: solo si tienes monedas y no está pasando nada importante
public class SeagullSpawner : MonoBehaviour
{
    private float timer;

    void Start()
    {
        timer = Random.Range(12f, 20f);
    }

    void Update()
    {
        if (MapGenerator.InputLocked || PlayerStats.GameEnded) return;
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = Random.Range(18f, 30f);

        PlayerController player = PlayerController.Instance;
        if (player == null) return;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null || stats.IsDead || stats.Coins <= 0) return;

        Vector3 p = player.transform.position;
        SeagullThief.Launch(transform.parent, new Vector3(p.x, 0f, p.z));
    }
}
