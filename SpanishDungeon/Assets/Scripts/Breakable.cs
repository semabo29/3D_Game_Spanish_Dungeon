using UnityEngine;
using System.Collections.Generic;

// ======================================================================
//  Objetos rompibles: macetas, botijos y cajas de naranjas
// ======================================================================
public class Breakable : MonoBehaviour
{
    public static readonly List<Breakable> Active = new List<Breakable>();

    private int kind;
    private GameObject coinPrefab;
    private bool broken;
    private float enemyCheck;

    public static Breakable Create(Transform parent, Vector3 objectPos, int kind, GameObject coinPrefab)
    {
        Mesh mesh = kind == 0 ? VoxelModels.Maceta(Random.Range(0, 4)) : kind == 1 ? VoxelModels.Botijo() : VoxelModels.CajaNaranjas();
        Vector3 floor = new Vector3(objectPos.x, 0f, objectPos.z);
        GameObject go = VoxelLib.Spawn("Rompible", mesh, parent, floor, Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f));
        Breakable b = go.AddComponent<Breakable>();
        b.kind = kind;
        b.coinPrefab = coinPrefab;
        return b;
    }

    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    public static Breakable At(Vector3 pos)
    {
        foreach (Breakable b in Active)
        {
            if (b == null || b.broken) continue;
            if (Mathf.Abs(b.transform.position.x - pos.x) < 0.3f && Mathf.Abs(b.transform.position.z - pos.z) < 0.3f) return b;
        }
        return null;
    }

    void Update()
    {
        // Los enemigos también lo pisotean todo
        enemyCheck -= Time.deltaTime;
        if (enemyCheck > 0f) return;
        enemyCheck = 0.2f;
        if (Near(FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None)) ||
            Near(FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None)) ||
            Near(FindObjectsByType<Guiri>(FindObjectsSortMode.None)) ||
            Near(FindObjectsByType<Pedro>(FindObjectsSortMode.None)))
        {
            Smash(Vector3.up, false);
        }
    }

    private bool Near<T>(T[] list) where T : Component
    {
        foreach (T c in list)
        {
            if (Mathf.Abs(c.transform.position.x - transform.position.x) < 0.4f && Mathf.Abs(c.transform.position.z - transform.position.z) < 0.4f) return true;
        }
        return false;
    }

    public void Smash(Vector3 direction, bool byPlayer)
    {
        if (broken) return;
        broken = true;
        Vector3 pos = transform.position;

        Sfx.Play(SfxType.Smash, 0.8f, kind == 2 ? 0.8f : 1.1f);
        GameFeel.Shake(0.06f, 0.12f);

        switch (kind)
        {
            case 0:
                GameFeel.VoxelBurst(pos, VoxelLib.Hex("#C1572B"), 12, 4f);
                GameFeel.VoxelBurst(pos + Vector3.up * 0.4f, VoxelLib.Hex("#3E8E3B"), 6, 3f);
                GameFeel.VoxelBurst(pos + Vector3.up * 0.5f, VoxelLib.Hex("#E0283A"), 5, 3f);
                if (byPlayer && Random.value < 0.35f) GameFeel.FloatingText(pos, "¡Mis geranios!", new Color(1f, 0.6f, 0.7f), 3.5f);
                break;
            case 1:
                GameFeel.VoxelBurst(pos, VoxelLib.Hex("#D8A56B"), 14, 4f);
                GameFeel.VoxelBurst(pos + Vector3.up * 0.3f, new Color(0.5f, 0.75f, 1f), 8, 2.5f);
                PlayerController pc = PlayerController.Instance;
                if (byPlayer && pc != null && pc.IsDrunk)
                {
                    pc.Sober();
                    GameFeel.FloatingText(pos, "¡Agua fresquita!", new Color(0.5f, 0.85f, 1f), 4f);
                }
                break;
            default:
                GameFeel.VoxelBurst(pos, VoxelLib.Hex("#C08A4F"), 14, 4f);
                SpawnOranges(pos, direction);
                if (byPlayer && Random.value < 0.4f) GameFeel.FloatingText(pos, "¡De Valencia!", new Color(1f, 0.6f, 0.15f), 3.5f);
                break;
        }

        if (byPlayer)
        {
            RunStats.Smashes++;
            Achievements.OnSmash();
            if (coinPrefab != null && Random.value < 0.45f)
            {
                GameObject coin = Instantiate(coinPrefab, new Vector3(pos.x, 0.5f, pos.z), Quaternion.identity, transform.parent);
                coin.AddComponent<PopIn>();
            }
        }

        Destroy(gameObject);
    }

    private void SpawnOranges(Vector3 pos, Vector3 direction)
    {
        int n = Random.Range(4, 7);
        for (int i = 0; i < n; i++)
        {
            GameObject o = new GameObject("Naranja");
            o.layer = 2;
            o.transform.position = pos + Vector3.up * 0.35f + Random.insideUnitSphere * 0.15f;
            o.AddComponent<MeshFilter>().sharedMesh = VoxelModels.Naranja();
            o.AddComponent<MeshRenderer>().sharedMaterial = VoxelLib.Lit;
            SphereCollider sc = o.AddComponent<SphereCollider>();
            sc.radius = 0.11f;
            sc.sharedMaterial = new PhysicMaterial("Naranja") { bounciness = 0.6f, bounceCombine = PhysicMaterialCombine.Maximum };
            Rigidbody rb = o.AddComponent<Rigidbody>();
            rb.mass = 0.2f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            Vector3 dir = (direction.y > 0.9f ? Random.insideUnitSphere : direction.normalized + Random.insideUnitSphere * 0.6f);
            dir.y = Mathf.Abs(dir.y) * 0.5f + 0.5f;
            rb.velocity = dir.normalized * Random.Range(2f, 4f);
            o.AddComponent<ShrinkAndDie>().lifetime = Random.Range(3f, 4.5f);
        }
    }
}
