using UnityEngine;
using System;
using System.Collections.Generic;

// Constructor de modelos voxel por código.
// Se rellenan vóxeles en una rejilla (x = ancho, y = alto, z = fondo; el frente mira a -Z)
// y se genera una malla con solo las caras visibles y color por vértice.
// Un color con alfa 0 se dibuja emisivo (ventanas encendidas, neones, farolillos...).
public class VoxelModel
{
    private readonly Dictionary<Vector3Int, Color32> voxels = new Dictionary<Vector3Int, Color32>();

    // Variación aleatoria de brillo por vóxel para que no parezca plástico
    public float jitter = 0.06f;

    public int Count => voxels.Count;

    public VoxelModel Set(int x, int y, int z, Color c)
    {
        voxels[new Vector3Int(x, y, z)] = c;
        return this;
    }

    public bool Has(int x, int y, int z) => voxels.ContainsKey(new Vector3Int(x, y, z));

    public VoxelModel Clear(int x, int y, int z)
    {
        voxels.Remove(new Vector3Int(x, y, z));
        return this;
    }

    // Caja con límites inclusivos
    public VoxelModel Box(int x0, int y0, int z0, int x1, int y1, int z1, Color c)
    {
        for (int x = Mathf.Min(x0, x1); x <= Mathf.Max(x0, x1); x++)
            for (int y = Mathf.Min(y0, y1); y <= Mathf.Max(y0, y1); y++)
                for (int z = Mathf.Min(z0, z1); z <= Mathf.Max(z0, z1); z++)
                    Set(x, y, z, c);
        return this;
    }

    public VoxelModel ClearBox(int x0, int y0, int z0, int x1, int y1, int z1)
    {
        for (int x = Mathf.Min(x0, x1); x <= Mathf.Max(x0, x1); x++)
            for (int y = Mathf.Min(y0, y1); y <= Mathf.Max(y0, y1); y++)
                for (int z = Mathf.Min(z0, z1); z <= Mathf.Max(z0, z1); z++)
                    Clear(x, y, z);
        return this;
    }

    // Pinta solo los vóxeles que ya existen dentro de la caja
    public VoxelModel Paint(int x0, int y0, int z0, int x1, int y1, int z1, Color c)
    {
        for (int x = Mathf.Min(x0, x1); x <= Mathf.Max(x0, x1); x++)
            for (int y = Mathf.Min(y0, y1); y <= Mathf.Max(y0, y1); y++)
                for (int z = Mathf.Min(z0, z1); z <= Mathf.Max(z0, z1); z++)
                    if (Has(x, y, z)) Set(x, y, z, c);
        return this;
    }

    public VoxelModel Sphere(float cx, float cy, float cz, float r, Color c, float squashY = 1f)
    {
        int rx = Mathf.CeilToInt(r);
        int ry = Mathf.CeilToInt(r * squashY);
        for (int x = -rx; x <= rx; x++)
            for (int y = -ry; y <= ry; y++)
                for (int z = -rx; z <= rx; z++)
                {
                    float yy = y / Mathf.Max(0.01f, squashY);
                    if (x * x + yy * yy + z * z <= r * r + 0.3f)
                        Set(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), Mathf.RoundToInt(cz + z), c);
                }
        return this;
    }

    // Cilindro vertical
    public VoxelModel CylinderY(float cx, int y0, float cz, float r, int height, Color c)
    {
        int ri = Mathf.CeilToInt(r);
        for (int y = y0; y < y0 + height; y++)
            for (int x = -ri; x <= ri; x++)
                for (int z = -ri; z <= ri; z++)
                    if (x * x + z * z <= r * r + 0.3f)
                        Set(Mathf.RoundToInt(cx + x), y, Mathf.RoundToInt(cz + z), c);
        return this;
    }

    // Cilindro tumbado a lo largo del eje Z (ruedas)
    public VoxelModel CylinderZ(float cx, float cy, int z0, float r, int depth, Color c)
    {
        int ri = Mathf.CeilToInt(r);
        for (int z = z0; z < z0 + depth; z++)
            for (int x = -ri; x <= ri; x++)
                for (int y = -ri; y <= ri; y++)
                    if (x * x + y * y <= r * r + 0.3f)
                        Set(Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), z, c);
        return this;
    }

    // Cono vertical (tejados de molino, torres)
    public VoxelModel Cone(float cx, int y0, float cz, float r, int height, Color c)
    {
        for (int y = 0; y < height; y++)
        {
            float rr = Mathf.Lerp(r, 0.4f, y / (float)Mathf.Max(1, height - 1));
            int ri = Mathf.CeilToInt(rr);
            for (int x = -ri; x <= ri; x++)
                for (int z = -ri; z <= ri; z++)
                    if (x * x + z * z <= rr * rr + 0.3f)
                        Set(Mathf.RoundToInt(cx + x), y0 + y, Mathf.RoundToInt(cz + z), c);
        }
        return this;
    }

    // Línea de vóxeles (troncos, palos, hojas de palmera)
    public VoxelModel Line(Vector3 a, Vector3 b, Color c, int thickness = 1)
    {
        int steps = Mathf.CeilToInt(Vector3.Distance(a, b) * 1.5f) + 1;
        for (int i = 0; i <= steps; i++)
        {
            Vector3 p = Vector3.Lerp(a, b, i / (float)steps);
            int x = Mathf.RoundToInt(p.x), y = Mathf.RoundToInt(p.y), z = Mathf.RoundToInt(p.z);
            if (thickness <= 1) Set(x, y, z, c);
            else Box(x, y, z, x + thickness - 1, y + thickness - 1, z + thickness - 1, c);
        }
        return this;
    }

    // Extruye un dibujo ASCII (fila 0 = arriba). Los espacios y '.' quedan vacíos.
    public VoxelModel Extrude(string[] rows, Dictionary<char, Color> palette, int x0, int y0, int z0, int depth)
    {
        int h = rows.Length;
        for (int r = 0; r < h; r++)
        {
            string row = rows[r];
            for (int i = 0; i < row.Length; i++)
            {
                char ch = row[i];
                if (ch == ' ' || ch == '.') continue;
                if (!palette.TryGetValue(ch, out Color c)) continue;
                for (int d = 0; d < depth; d++) Set(x0 + i, y0 + (h - 1 - r), z0 + d, c);
            }
        }
        return this;
    }

    // Texto con la fuente pixelada de 3x5 (para carteles y neones)
    public VoxelModel Text(string text, int x0, int y0, int z0, int depth, Color c)
    {
        int cursor = x0;
        foreach (char ch in text.ToUpperInvariant())
        {
            string[] glyph = PixelFont.Get(ch);
            if (glyph != null)
            {
                for (int r = 0; r < 5; r++)
                    for (int i = 0; i < 3; i++)
                        if (glyph[r][i] == '#')
                            for (int d = 0; d < depth; d++) Set(cursor + i, y0 + (4 - r), z0 + d, c);
            }
            cursor += 4;
        }
        return this;
    }

    public static Color Glow(Color c)
    {
        c.a = 0f;
        return c;
    }

    // ---------- Malla ----------

    private static readonly Vector3Int[] Dirs =
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
    };

    private static readonly Vector3[][] Corners =
    {
        new[] { new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1) },
        new[] { new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0) },
        new[] { new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) },
        new[] { new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1) },
        new[] { new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1) },
        new[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) }
    };

    // Malla con el pivote abajo y centrada en X/Z (o en el centro si centerY)
    public Mesh ToMesh(float voxelSize, bool centerY = false)
    {
        Mesh mesh = new Mesh();
        if (voxels.Count == 0) return mesh;

        Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        foreach (Vector3Int p in voxels.Keys)
        {
            min = Vector3Int.Min(min, p);
            max = Vector3Int.Max(max, p);
        }
        Vector3 pivot = new Vector3((min.x + max.x + 1) * 0.5f, centerY ? (min.y + max.y + 1) * 0.5f : min.y, (min.z + max.z + 1) * 0.5f);

        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Color32> colors = new List<Color32>();
        List<int> tris = new List<int>();

        foreach (KeyValuePair<Vector3Int, Color32> kv in voxels)
        {
            Vector3Int p = kv.Key;
            Color32 col = Shade(kv.Value, p);

            for (int f = 0; f < 6; f++)
            {
                if (voxels.ContainsKey(p + Dirs[f])) continue;

                int start = verts.Count;
                for (int k = 0; k < 4; k++)
                {
                    verts.Add((new Vector3(p.x, p.y, p.z) + Corners[f][k] - pivot) * voxelSize);
                    normals.Add(Dirs[f]);
                    colors.Add(col);
                }
                tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
                tris.Add(start); tris.Add(start + 2); tris.Add(start + 3);
            }
        }

        if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    private Color32 Shade(Color32 c, Vector3Int p)
    {
        if (jitter <= 0f) return c;
        // Ruido determinista por posición
        int h = p.x * 73856093 ^ p.y * 19349663 ^ p.z * 83492791;
        float n = ((h & 0xffff) / 65535f) * 2f - 1f;
        float k = 1f + n * jitter;
        return new Color32(
            (byte)Mathf.Clamp(c.r * k, 0, 255),
            (byte)Mathf.Clamp(c.g * k, 0, 255),
            (byte)Mathf.Clamp(c.b * k, 0, 255),
            c.a);
    }
}

// Materiales, caché de mallas y utilidades para crear objetos voxel
public static class VoxelLib
{
    private static Material litMaterial;
    private static Material unlitAlphaMaterial;
    private static Material skyMaterial;
    private static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();
    private static Mesh cubeMesh;
    private static Mesh quadMesh;

    public static Material Lit
    {
        get
        {
            if (litMaterial == null)
            {
                Shader s = Shader.Find("SpanishDungeon/VoxelLit");
                if (s == null) s = Shader.Find("Standard");
                litMaterial = new Material(s) { name = "VoxelLit (runtime)" };
                litMaterial.enableInstancing = true;
            }
            return litMaterial;
        }
    }

    public static Material UnlitAlpha
    {
        get
        {
            if (unlitAlphaMaterial == null)
            {
                Shader s = Shader.Find("SpanishDungeon/UnlitAlpha");
                if (s == null) s = Shader.Find("Sprites/Default");
                unlitAlphaMaterial = new Material(s) { name = "UnlitAlpha (runtime)" };
            }
            return unlitAlphaMaterial;
        }
    }

    public static Material Sky
    {
        get
        {
            if (skyMaterial == null)
            {
                Shader s = Shader.Find("SpanishDungeon/SkyGradient");
                if (s == null) s = Shader.Find("Sprites/Default");
                skyMaterial = new Material(s) { name = "Sky (runtime)" };
            }
            return skyMaterial;
        }
    }

    // Devuelve una malla ya construida o la construye la primera vez
    public static Mesh Cached(string key, Func<Mesh> build)
    {
        if (meshCache.TryGetValue(key, out Mesh m) && m != null) return m;
        m = build();
        m.name = key;
        meshCache[key] = m;
        return m;
    }

    public static Mesh Cube
    {
        get
        {
            if (cubeMesh == null)
            {
                VoxelModel v = new VoxelModel { jitter = 0f };
                v.Set(0, 0, 0, Color.white);
                cubeMesh = v.ToMesh(1f, true);
                cubeMesh.name = "VoxelCube";
            }
            return cubeMesh;
        }
    }

    public static Mesh Quad
    {
        get
        {
            if (quadMesh == null)
            {
                quadMesh = new Mesh { name = "VoxelQuad" };
                quadMesh.SetVertices(new List<Vector3> { new Vector3(-0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, -0.5f, 0) });
                quadMesh.SetColors(new List<Color32> { Color.white, Color.white, Color.white, Color.white });
                quadMesh.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
                quadMesh.RecalculateBounds();
            }
            return quadMesh;
        }
    }

    public static GameObject Spawn(string name, Mesh mesh, Transform parent, Vector3 position, Quaternion rotation, bool castShadows = true)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = Lit;
        r.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = castShadows;
        return go;
    }

    // Igual que Spawn pero con posición/rotación locales (para colgar cosas de la cámara)
    public static GameObject SpawnLocal(string name, Mesh mesh, Transform parent, Vector3 localPos, bool castShadows = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = Lit;
        r.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = castShadows;
        return go;
    }

    public static Color Hex(string hex)
    {
        if (!hex.StartsWith("#")) hex = "#" + hex;
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}

// Fuente pixelada de 3x5 para letreros voxel
public static class PixelFont
{
    private static Dictionary<char, string[]> glyphs;

    public static string[] Get(char c)
    {
        if (glyphs == null) Build();
        return glyphs.TryGetValue(c, out string[] g) ? g : null;
    }

    private static void Build()
    {
        glyphs = new Dictionary<char, string[]>();
        void G(char c, string a, string b, string d, string e, string f) { glyphs[c] = new[] { a, b, d, e, f }; }

        G('A', ".#.", "#.#", "###", "#.#", "#.#");
        G('B', "##.", "#.#", "##.", "#.#", "##.");
        G('C', ".##", "#..", "#..", "#..", ".##");
        G('D', "##.", "#.#", "#.#", "#.#", "##.");
        G('E', "###", "#..", "##.", "#..", "###");
        G('F', "###", "#..", "##.", "#..", "#..");
        G('G', ".##", "#..", "#.#", "#.#", ".##");
        G('H', "#.#", "#.#", "###", "#.#", "#.#");
        G('I', "###", ".#.", ".#.", ".#.", "###");
        G('J', "..#", "..#", "..#", "#.#", ".#.");
        G('K', "#.#", "#.#", "##.", "#.#", "#.#");
        G('L', "#..", "#..", "#..", "#..", "###");
        G('M', "#.#", "###", "###", "#.#", "#.#");
        G('N', "##.", "#.#", "#.#", "#.#", "#.#");
        G('Ñ', "###", "...", "##.", "#.#", "#.#");
        G('O', ".#.", "#.#", "#.#", "#.#", ".#.");
        G('P', "##.", "#.#", "##.", "#..", "#..");
        G('Q', ".#.", "#.#", "#.#", "##.", ".##");
        G('R', "##.", "#.#", "##.", "#.#", "#.#");
        G('S', ".##", "#..", ".#.", "..#", "##.");
        G('T', "###", ".#.", ".#.", ".#.", ".#.");
        G('U', "#.#", "#.#", "#.#", "#.#", "###");
        G('V', "#.#", "#.#", "#.#", "#.#", ".#.");
        G('W', "#.#", "#.#", "###", "###", "#.#");
        G('X', "#.#", "#.#", ".#.", "#.#", "#.#");
        G('Y', "#.#", "#.#", ".#.", ".#.", ".#.");
        G('Z', "###", "..#", ".#.", "#..", "###");
        G('0', "###", "#.#", "#.#", "#.#", "###");
        G('1', ".#.", "##.", ".#.", ".#.", "###");
        G('2', "##.", "..#", ".#.", "#..", "###");
        G('3', "##.", "..#", ".#.", "..#", "##.");
        G('4', "#.#", "#.#", "###", "..#", "..#");
        G('5', "###", "#..", "##.", "..#", "##.");
        G('6', ".##", "#..", "###", "#.#", "###");
        G('7', "###", "..#", ".#.", ".#.", ".#.");
        G('8', "###", "#.#", "###", "#.#", "###");
        G('9', "###", "#.#", "###", "..#", "##.");
        G('-', "...", "...", "###", "...", "...");
        G('!', ".#.", ".#.", ".#.", "...", ".#.");
        G('?', "##.", "..#", ".#.", "...", ".#.");
        G('.', "...", "...", "...", "...", ".#.");
        G('€', ".##", "#..", "###", "#..", ".##");
        G(' ', "...", "...", "...", "...", "...");
    }
}

// Sprites pixel art generados por código (corazones, monedas, iconos del HUD)
public static class PixelArt
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Make(string key, string[] rows, Dictionary<char, Color> palette)
    {
        if (cache.TryGetValue(key, out Sprite s) && s != null) return s;

        int h = rows.Length;
        int w = 0;
        foreach (string r in rows) w = Mathf.Max(w, r.Length);

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
        {
            string row = rows[h - 1 - y];
            for (int x = 0; x < w; x++)
            {
                char ch = x < row.Length ? row[x] : ' ';
                tex.SetPixel(x, y, palette.TryGetValue(ch, out Color c) ? c : clear);
            }
        }
        tex.Apply();
        s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        s.name = key;
        cache[key] = s;
        return s;
    }

    // Degradado radial para la viñeta de poca vida
    public static Sprite Vignette()
    {
        const string key = "vignette";
        if (cache.TryGetValue(key, out Sprite s) && s != null) return s;
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.4142f;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, d));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        cache[key] = s;
        return s;
    }

    public static Sprite White()
    {
        const string key = "white";
        if (cache.TryGetValue(key, out Sprite s) && s != null) return s;
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        s = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
        cache[key] = s;
        return s;
    }
}
