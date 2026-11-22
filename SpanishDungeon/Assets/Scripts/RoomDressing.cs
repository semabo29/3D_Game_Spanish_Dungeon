using UnityEngine;
using System.Collections.Generic;

// Datos de la sala cargada que sirven para decorarla
public class RoomLayout
{
    public int room;
    public int lines;
    public Vector3 start;
    public readonly List<Vector3> floor = new List<Vector3>();
    public readonly List<Vector3> doors = new List<Vector3>();
    public readonly List<Vector3> exits = new List<Vector3>();
    public float minZ = float.MaxValue, maxZ = float.MinValue;

    public bool FarFromKeyCells(Vector3 p, float minDist)
    {
        if (Flat(p, start) < minDist) return false;
        foreach (Vector3 d in doors) if (Flat(p, d) < minDist) return false;
        foreach (Vector3 e in exits) if (Flat(p, e) < minDist) return false;
        return true;
    }

    private static float Flat(Vector3 a, Vector3 b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
}

// Decora cada sala al cargarla: objetos rompibles, power-ups, palomas y guirnaldas
public static class RoomDressing
{
    public static void Dress(RoomLayout layout, Transform parent, GameObject coinPrefab)
    {
        List<Vector3> free = new List<Vector3>();
        foreach (Vector3 p in layout.floor)
        {
            if (layout.FarFromKeyCells(p, 2f) && p.x > 0.5f && p.x < layout.lines - 1.5f) free.Add(p);
        }
        Shuffle(free);
        int cursor = 0;
        Vector3? Take()
        {
            if (cursor >= free.Count) return null;
            return free[cursor++];
        }

        // Farolas del mapa: modelo voxel nuevo (y encendidas de noche)
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("Farola")) StreetLamp.Restyle(child);
        }

        // Objetos rompibles
        int breakables = Random.Range(1, 4);
        for (int i = 0; i < breakables; i++)
        {
            Vector3? p = Take();
            if (p == null) break;
            Breakable.Create(parent, p.Value, Random.Range(0, 3), coinPrefab);
        }

        // Power-up
        PowerUpType? type = ChoosePowerUp(layout.room);
        if (type.HasValue)
        {
            Vector3? p = Take();
            if (p != null) PowerUpPickup.Create(parent, p.Value, type.Value);
        }

        // Palomas
        int pigeons = 0;
        switch (layout.room)
        {
            case 1: case 3: case 10: pigeons = Random.Range(3, 6); break;
            case 7: case 8: pigeons = 0; break;
            default: pigeons = Random.Range(0, 3); break;
        }
        for (int i = 0; i < pigeons; i++)
        {
            Vector3? p = Take();
            if (p == null) break;
            Pigeon.Create(parent, p.Value);
        }

        // Gatos callejeros
        int cats = 0;
        switch (layout.room)
        {
            case 3: case 4: case 7: case 9: cats = Random.value < 0.75f ? Random.Range(1, 3) : 0; break;
            default: cats = Random.value < 0.25f ? 1 : 0; break;
        }
        for (int i = 0; i < cats; i++)
        {
            Vector3? p = Take();
            if (p == null) break;
            StrayCat.Create(parent, p.Value);
        }

        BuildGarlands(layout, parent);
    }

    private static PowerUpType? ChoosePowerUp(int room)
    {
        if (room == 1) return null;
        if (room == 9) return Random.value < 0.5f ? PowerUpType.Bocadillo : PowerUpType.Abanico;
        if (room == 10) return PowerUpType.Abanico;
        if (Random.value > 0.6f) return null;

        float r = Random.value;
        if (r < 0.28f) return PowerUpType.Bocadillo;
        if (r < 0.48f) return PowerUpType.Churros;
        if (r < 0.70f) return PowerUpType.Abanico;
        if (r < 0.88f) return PowerUpType.Siesta;
        return PowerUpType.Jamon;
    }

    private static void BuildGarlands(RoomLayout layout, Transform parent)
    {
        Color[] colors;
        bool lanterns = false;
        int count = 1;
        switch (layout.room)
        {
            case 1: colors = new[] { VoxelLib.Hex("#E3122B"), Color.white }; break;
            case 2: colors = new[] { VoxelLib.Hex("#C8102E"), VoxelLib.Hex("#FFC400") }; break;
            case 4: case 9: colors = new[] { VoxelLib.Hex("#FF5C8A"), VoxelLib.Hex("#FFC400"), VoxelLib.Hex("#3FB5E0"), VoxelLib.Hex("#3FBF6A") }; break;
            case 7: colors = new[] { VoxelLib.Hex("#FF4FD8"), VoxelLib.Hex("#3FF2FF") }; lanterns = true; break;
            case 8: colors = new[] { VoxelLib.Hex("#FF6B6B"), VoxelLib.Hex("#FFD84A"), VoxelLib.Hex("#7CF08A"), VoxelLib.Hex("#7AC8FF"), VoxelLib.Hex("#E86A92") }; lanterns = true; count = 2; break;
            case 10: colors = new[] { VoxelLib.Hex("#C8102E"), VoxelLib.Hex("#FFC400"), VoxelLib.Hex("#C8102E") }; break;
            default: return;
        }
        if (layout.minZ > layout.maxZ) return;

        for (int g = 0; g < count; g++)
        {
            float x = count == 1 ? Mathf.Round(layout.lines * 0.5f) : (g == 0 ? 2f : layout.lines - 4f);
            Vector3 a = new Vector3(x, 2.6f, layout.minZ - 0.4f);
            Vector3 b = new Vector3(x, 2.6f, layout.maxZ + 0.4f);
            Garland.Build(parent, a, b, colors, lanterns);
        }
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}

// ======================================================================
//  Power-ups
// ======================================================================
public enum PowerUpType { Bocadillo, Churros, Abanico, Siesta, Jamon }

// ======================================================================
//  Guirnaldas de banderines o farolillos sobre la sala
// ======================================================================
public static class Garland
{
    public static void Build(Transform parent, Vector3 a, Vector3 b, Color[] colors, bool lanterns)
    {
        GameObject root = new GameObject("Guirnalda");
        root.transform.SetParent(parent, false);
        root.transform.position = a;

        float length = Vector3.Distance(a, b);
        int segments = Mathf.Max(4, Mathf.RoundToInt(length / 0.35f));
        float sag = 0.45f;
        Vector3 prev = a;
        Color cord = new Color(0.15f, 0.12f, 0.1f);

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = Vector3.Lerp(a, b, t) + Vector3.down * sag * 4f * t * (1f - t);

            if (i > 0)
            {
                GameObject seg = new GameObject("Cuerda");
                seg.transform.SetParent(root.transform, false);
                seg.transform.position = (prev + p) * 0.5f;
                seg.transform.rotation = Quaternion.LookRotation(p - prev);
                seg.transform.localScale = new Vector3(0.025f, 0.025f, Vector3.Distance(prev, p));
                seg.AddComponent<MeshFilter>().sharedMesh = VoxelLib.Cube;
                MeshRenderer r = seg.AddComponent<MeshRenderer>();
                r.sharedMaterial = VoxelLib.Lit;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", cord);
                r.SetPropertyBlock(mpb);
            }

            if (i > 0 && i < segments)
            {
                Color c = colors[i % colors.Length];
                Mesh mesh = lanterns ? VoxelModels.Farolillo(c) : VoxelModels.Banderin(c);
                float drop = lanterns ? 0.32f : 0.3f;
                GameObject flag = VoxelLib.Spawn("Banderin", mesh, root.transform, p + Vector3.down * drop, Quaternion.LookRotation(Vector3.right), false);
                flag.AddComponent<Sway>();
            }
            prev = p;
        }
    }
}
