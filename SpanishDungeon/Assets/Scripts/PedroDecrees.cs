using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Ataque especial de Pedro: el "Sello de Hacienda". Cada pocos segundos se para,
// marca una cruz de casillas alrededor de Torrento y, tras el aviso, cae un sello gigante.
// En la fase final lanza dos: uno donde estás y otro hacia donde vas.
public class PedroDecrees : MonoBehaviour
{
    public float firstDelay = 4.5f;
    public float interval = 6.5f;
    public float phase2Interval = 4.5f;
    public float warning = 1.1f;

    private Pedro pedro;
    private PedroStats stats;
    private float timer;
    private Vector3 lastPlayerTile;

    private static readonly string[] Shouts = { "¡Sello de Hacienda!", "¡Inspección fiscal!", "¡Decreto ley!", "¡Paralela que te crió!" };

    void Start()
    {
        pedro = GetComponent<Pedro>();
        stats = GetComponent<PedroStats>();
        timer = firstDelay;
    }

    void Update()
    {
        if (stats != null && stats.IsDead()) return;
        if (PlayerStats.GameEnded || MapGenerator.InputLocked) return;
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        bool phase2 = pedro != null && pedro.Phase2;
        timer = phase2 ? phase2Interval : interval;
        StartCoroutine(Cast(player, phase2));
    }

    private IEnumerator Cast(PlayerController player, bool phase2)
    {
        if (pedro != null) pedro.Casting = true;
        GameFeel.FloatingText(transform.position + Vector3.up * 0.8f, Shouts[Random.Range(0, Shouts.Length)], new Color(1f, 0.35f, 0.3f), 5f);
        Sfx.Play(SfxType.Thunder, 0.5f, 1.6f);

        Vector3 tile = Snap(player.targetPosition);
        List<Vector3> centers = new List<Vector3> { tile };
        if (phase2)
        {
            // Segundo sello hacia donde se mueve el jugador (o al lado si está quieto)
            Vector3 dir = tile - lastPlayerTile;
            if (dir.sqrMagnitude < 0.5f) dir = Random.value < 0.5f ? Vector3.forward : Vector3.back;
            dir = Mathf.Abs(dir.x) > Mathf.Abs(dir.z) ? new Vector3(Mathf.Sign(dir.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(dir.z));
            centers.Add(tile + dir * 2f);
        }
        lastPlayerTile = tile;

        Transform parent = MapGenerator.Instance != null ? MapGenerator.Instance.transform : null;
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
        foreach (Vector3 c in centers)
        {
            cells.Add(Key(c));
            cells.Add(Key(c + Vector3.right));
            cells.Add(Key(c + Vector3.left));
            cells.Add(Key(c + Vector3.forward));
            cells.Add(Key(c + Vector3.back));
        }

        List<DangerMarker> markers = new List<DangerMarker>();
        foreach (Vector2Int k in cells)
        {
            DangerMarker m = DangerMarker.Create(parent, new Vector3(k.x, 0f, k.y));
            m.Set(0.85f, true, true);
            markers.Add(m);
        }

        // Sellos que bajan desde el cielo durante el aviso
        List<Transform> stamps = new List<Transform>();
        foreach (Vector3 c in centers)
        {
            GameObject s = VoxelLib.Spawn("SelloHacienda", VoxelModels.Sello(), parent, new Vector3(c.x, 6f, c.z), Quaternion.identity);
            s.transform.localScale = Vector3.one * 2.4f;
            stamps.Add(s.transform);
        }

        // Pedro se queda quieto mientras lanza (un respiro para atacarle)
        float t = 0f;
        while (t < warning)
        {
            t += Time.deltaTime;
            float k = t / warning;
            for (int i = 0; i < stamps.Count; i++)
            {
                if (stamps[i] == null) continue;
                Vector3 p = stamps[i].position;
                p.y = Mathf.Lerp(6f, 1.4f, k * k * 0.6f);
                stamps[i].position = p;
                stamps[i].Rotate(0f, 120f * Time.deltaTime, 0f);
            }
            if (k > 0.45f && pedro != null) pedro.Casting = false;
            yield return null;
        }
        if (pedro != null) pedro.Casting = false;

        // ¡Pum!
        float fall = 0f;
        while (fall < 0.12f)
        {
            fall += Time.deltaTime;
            for (int i = 0; i < stamps.Count; i++)
            {
                if (stamps[i] == null) continue;
                Vector3 p = stamps[i].position;
                p.y = Mathf.Lerp(1.4f, 0.05f, fall / 0.12f);
                stamps[i].position = p;
            }
            yield return null;
        }

        GameFeel.Shake(0.25f, 0.2f);
        Sfx.Play(SfxType.Smash, 0.9f, 0.6f);
        foreach (Vector3 c in centers) GameFeel.VoxelBurst(c + Vector3.up * 0.2f, new Color(0.8f, 0.1f, 0.12f), 14, 4f);

        if (player != null && !PlayerStats.GameEnded)
        {
            PlayerStats ps = player.GetComponent<PlayerStats>();
            if (ps != null && cells.Contains(Key(player.targetPosition)) && cells.Contains(Key(player.transform.position)))
                ps.TakeDamage("sellado por Hacienda");
        }

        foreach (DangerMarker m in markers) if (m != null) Destroy(m.gameObject);
        yield return new WaitForSeconds(0.35f);
        foreach (Transform s in stamps) if (s != null) Destroy(s.gameObject);
    }

    private static Vector3 Snap(Vector3 p) => new Vector3(Mathf.Round(p.x), 0f, Mathf.Round(p.z));
    private static Vector2Int Key(Vector3 p) => new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.z));
}
