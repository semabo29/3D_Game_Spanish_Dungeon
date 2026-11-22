using UnityEngine;
using System.Collections.Generic;

// Catálogo de modelos voxel hechos a mano (por código).
// Todos miran hacia -Z, tienen el pivote abajo en el centro y se cachean la primera vez.
public static class VoxelModels
{
    private static Color C(string hex) => VoxelLib.Hex(hex);
    private static Color G(string hex) => VoxelModel.Glow(VoxelLib.Hex(hex));

    private static Color Pick(System.Random rng, params string[] hexes) => C(hexes[rng.Next(hexes.Length)]);

    // ======================================================================
    //  OBJETOS RECOGIBLES
    // ======================================================================

    public const float PickupVoxel = 0.06f;

    // Bocadillo de calamares: +1 corazón
    public static Mesh Bocadillo() => VoxelLib.Cached("bocadillo", () =>
    {
        VoxelModel v = new VoxelModel();
        Color bread = C("#D9A55B"), crust = C("#B5742F"), squid = C("#F4E7C5"), squidDark = C("#E0C48A"), lemon = C("#F7E04B");
        v.Box(1, 0, 0, 12, 1, 5, bread);
        v.Box(0, 1, 1, 13, 1, 4, bread);
        // Relleno de calamares a la romana asomando
        for (int x = 0; x <= 13; x++)
        {
            Color c = (x % 3 == 0) ? squidDark : squid;
            v.Box(x, 2, 0, x, 2, 5, c);
        }
        v.Set(-1, 2, 2, squid).Set(14, 2, 3, squidDark).Set(5, 2, -1, squid).Set(9, 2, 6, squid);
        v.Box(0, 3, 1, 13, 3, 4, bread);
        v.Box(1, 3, 0, 12, 3, 5, bread);
        v.Box(1, 4, 1, 12, 4, 4, crust);
        v.Box(3, 5, 2, 10, 5, 3, crust);
        // Cortes del pan
        v.Set(3, 5, 2, bread).Set(6, 5, 3, bread).Set(9, 5, 2, bread);
        // Rodaja de limón de regalo
        v.Box(12, 4, -1, 13, 5, -1, lemon);
        return v.ToMesh(PickupVoxel);
    });

    // Churros con chocolate: velocidad
    public static Mesh Churros() => VoxelLib.Cached("churros", () =>
    {
        VoxelModel v = new VoxelModel();
        Color cup = C("#FFFFFF"), cupBand = C("#2F6FB0"), choco = C("#4A2210"), churro = C("#D9963F"), churroDark = C("#B8752A"), sugar = C("#FFF7E0");
        v.CylinderY(4, 0, 4, 3.2f, 5, cup);
        v.CylinderY(4, 2, 4, 3.3f, 1, cupBand);
        v.CylinderY(4, 4, 4, 2.4f, 1, choco);
        // Asa de la taza
        v.Box(8, 1, 4, 8, 3, 4, cup).Set(7, 1, 4, cup).Set(7, 3, 4, cup);
        // Tres churros inclinados
        Vector3[] bases = { new Vector3(3, 4, 3), new Vector3(5, 4, 5), new Vector3(4, 4, 4) };
        Vector3[] tops = { new Vector3(0, 13, 1), new Vector3(9, 12, 7), new Vector3(5, 15, 3) };
        for (int i = 0; i < 3; i++)
        {
            int steps = 14;
            for (int s = 0; s <= steps; s++)
            {
                Vector3 p = Vector3.Lerp(bases[i], tops[i], s / (float)steps);
                Color c = (s % 2 == 0) ? churro : churroDark;
                v.Set(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z), c);
                v.Set(Mathf.RoundToInt(p.x) + 1, Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z), c);
                if (s % 4 == 1) v.Set(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z) - 1, sugar);
            }
        }
        return v.ToMesh(PickupVoxel);
    });

    // Abanico flamenco: escudo de un golpe
    public static Mesh Abanico() => VoxelLib.Cached("abanico", () =>
    {
        VoxelModel v = new VoxelModel();
        var pal = new Dictionary<char, Color>
        {
            { 'r', C("#D91E2E") }, { 'd', C("#8E0F1C") }, { 'w', C("#FFF4E6") }, { 'k', C("#3B2414") }, { 'g', C("#E8B83A") }
        };
        string[] rows =
        {
            "    rrrrrrrrr    ",
            "  rrwrrdrrdrrwrr  ",
            " rrrrrdrrrrdrrrrr ",
            "rrwrrrdrwrrdrrrwrr",
            "rdrrrrdrrrrdrrrrdr",
            " rdrrrdrrwrdrrrdr ",
            "  rdrrdrrrrdrrdr  ",
            "   rdrdrrrrdrdr   ",
            "    rddrrrrddr    ",
            "     gddrrddg     ",
            "      gkkkkg      ",
            "       kkkk       ",
            "        kk        ",
        };
        v.Extrude(rows, pal, 0, 0, 0, 2);
        return v.ToMesh(PickupVoxel);
    });

    // Almohada de la siesta: congela a los enemigos
    public static Mesh Almohada() => VoxelLib.Cached("almohada", () =>
    {
        VoxelModel v = new VoxelModel();
        Color cloth = C("#BFD9FF"), stripe = C("#FFFFFF"), lace = C("#F7E9FF");
        v.Box(0, 1, 0, 13, 3, 8, cloth);
        v.Box(1, 0, 1, 12, 4, 7, cloth);
        v.Box(1, 4, 2, 12, 4, 6, cloth);
        for (int x = 1; x <= 12; x += 3) v.Paint(x, 0, 0, x, 4, 8, stripe);
        // Esquinas redondeadas y puntillas
        v.ClearBox(0, 1, 0, 0, 3, 0).ClearBox(13, 1, 0, 13, 3, 0).ClearBox(0, 1, 8, 0, 3, 8).ClearBox(13, 1, 8, 13, 3, 8);
        v.Set(-1, 2, 4, lace).Set(14, 2, 4, lace).Set(-1, 2, 2, lace).Set(14, 2, 6, lace);
        // Gorro de dormir encima, por supuesto
        Color cap = C("#E04848"), pom = C("#FFFFFF");
        v.Box(5, 5, 3, 9, 5, 6, cap).Box(6, 6, 3, 8, 6, 5, cap).Box(7, 7, 4, 8, 7, 4, cap).Set(9, 8, 4, cap).Set(10, 8, 4, pom).Set(10, 9, 4, pom);
        return v.ToMesh(PickupVoxel);
    });

    // Jamón: vale 5 monedas
    public static Mesh Jamon() => VoxelLib.Cached("jamon", () =>
    {
        VoxelModel v = new VoxelModel();
        Color hoof = C("#1C1410"), skin = C("#E6C79A"), rind = C("#9A4B22"), meat = C("#A8323A"), fat = C("#F1E2C8");
        for (int x = 0; x < 17; x++)
        {
            float r = x < 3 ? 0.9f : x < 6 ? 1.4f : Mathf.Min(1.4f + (x - 6) * 0.45f, 4.2f);
            if (x > 14) r -= (x - 14) * 0.8f;
            int ri = Mathf.CeilToInt(r);
            for (int y = -ri; y <= ri; y++)
                for (int z = -ri; z <= ri; z++)
                {
                    float d2 = y * y + z * z;
                    if (d2 > r * r + 0.3f) continue;
                    Color c;
                    if (x < 2) c = hoof;
                    else if (x < 6) c = skin;
                    else c = d2 > (r - 1f) * (r - 1f) ? rind : meat;
                    v.Set(x, 5 + y - (x / 6), 5 + z, c);
                }
        }
        // Corte: grasa y carne a la vista
        for (int y = -3; y <= 3; y++)
            for (int z = -3; z <= 3; z++)
            {
                float d2 = y * y + z * z;
                if (d2 <= 9.3f) v.Set(13, 5 + y - 2, 5 + z, d2 > 5f ? fat : meat);
            }
        // Cuerda del jamonero
        v.Line(new Vector3(1, 6, 5), new Vector3(-1, 10, 5), C("#E8DCC0"));
        return v.ToMesh(PickupVoxel);
    });

    // ======================================================================
    //  OBJETOS ROMPIBLES
    // ======================================================================

    public const float PropVoxel = 0.07f;

    public static Mesh Maceta(int variant) => VoxelLib.Cached("maceta" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 31 + 7);
        VoxelModel v = new VoxelModel();
        Color pot = C("#C1572B"), potLight = C("#D9744A"), soil = C("#4A2E1C");
        Color leaf = C("#3E8E3B"), leafDark = C("#2C6B2B");
        Color flower = Pick(rng, "#E0283A", "#FF5C8A", "#F2F2F2", "#FF8C1A");
        v.CylinderY(4, 0, 4, 2.6f, 2, pot);
        v.CylinderY(4, 2, 4, 3.2f, 3, pot);
        v.CylinderY(4, 5, 4, 3.8f, 1, potLight);
        v.CylinderY(4, 5, 4, 2.6f, 1, soil);
        v.Sphere(4, 8, 4, 3.2f, leaf, 0.8f);
        v.Sphere(2, 7, 3, 1.6f, leafDark);
        v.Sphere(6, 8, 6, 1.6f, leafDark);
        for (int i = 0; i < 7; i++)
        {
            int fx = 1 + rng.Next(7), fz = 1 + rng.Next(7), fy = 9 + rng.Next(3);
            v.Box(fx, fy, fz, fx + 1, fy, fz + 1, flower);
            v.Set(fx, fy + 1, fz, flower);
        }
        return v.ToMesh(PropVoxel);
    });

    public static Mesh Botijo() => VoxelLib.Cached("botijo", () =>
    {
        VoxelModel v = new VoxelModel();
        Color clay = C("#D8A56B"), clayDark = C("#B9824A"), band = C("#8C4A2A");
        v.Sphere(5, 5, 5, 4.4f, clay, 0.9f);
        v.CylinderY(5, 0, 5, 2.5f, 2, clayDark);
        v.CylinderY(5, 5, 5, 4.6f, 1, band);
        // Pitorro y boca
        v.Line(new Vector3(8, 7, 5), new Vector3(11, 10, 5), clayDark, 2);
        v.CylinderY(3, 8, 5, 1.2f, 3, clayDark);
        // Asa
        v.Line(new Vector3(3, 10, 5), new Vector3(5, 12, 5), clay);
        v.Line(new Vector3(5, 12, 5), new Vector3(8, 9, 5), clay);
        return v.ToMesh(PropVoxel);
    });

    public static Mesh CajaNaranjas() => VoxelLib.Cached("caja_naranjas", () =>
    {
        VoxelModel v = new VoxelModel();
        Color wood = C("#C08A4F"), woodDark = C("#8E5E2E"), orange = C("#FF8A1E"), leaf = C("#3F8F3A");
        v.Box(0, 0, 0, 10, 0, 8, woodDark);
        for (int y = 1; y <= 5; y++)
        {
            Color c = (y % 2 == 0) ? woodDark : wood;
            if (y == 3) continue; // hueco entre listones
            v.Box(0, y, 0, 10, y, 0, c).Box(0, y, 8, 10, y, 8, c);
            v.Box(0, y, 0, 0, y, 8, c).Box(10, y, 0, 10, y, 8, c);
        }
        v.Box(1, 1, 1, 9, 4, 7, C("#6B3E1A"));
        // Naranjas asomando
        int[,] pos = { { 2, 2 }, { 5, 2 }, { 8, 2 }, { 3, 5 }, { 7, 5 }, { 5, 6 } };
        for (int i = 0; i < pos.GetLength(0); i++)
        {
            v.Sphere(pos[i, 0], 5, pos[i, 1], 1.6f, orange);
        }
        v.Set(5, 7, 6, leaf).Set(3, 7, 5, leaf);
        return v.ToMesh(PropVoxel);
    });

    public static Mesh Naranja() => VoxelLib.Cached("naranja", () =>
    {
        VoxelModel v = new VoxelModel();
        v.Sphere(2, 2, 2, 2.2f, C("#FF8A1E"));
        v.Set(2, 5, 2, C("#3F8F3A"));
        return v.ToMesh(0.05f, true);
    });

    // ======================================================================
    //  FAUNA Y DECORACIÓN DE SALA
    // ======================================================================

    public static Mesh Paloma(int variant) => VoxelLib.Cached("paloma" + variant, () =>
    {
        VoxelModel v = new VoxelModel();
        Color body = variant == 1 ? C("#D8D8D8") : C("#8E949C"), wing = variant == 1 ? C("#B8B8B8") : C("#6B7078");
        Color neck = C("#4CA38A"), neck2 = C("#8A5AA8"), beak = C("#3A3030"), feet = C("#E0623A"), eye = C("#E8452B");
        v.Box(1, 1, 2, 4, 3, 7, body);
        v.Box(1, 4, 2, 4, 4, 5, body);
        v.Box(0, 2, 3, 0, 3, 7, wing).Box(5, 2, 3, 5, 3, 7, wing);
        v.Box(1, 2, 8, 4, 3, 9, wing);
        v.Box(1, 3, 0, 4, 6, 2, body);
        v.Paint(1, 3, 1, 4, 4, 2, neck);
        v.Set(1, 4, 2, neck2).Set(4, 4, 2, neck2);
        v.Set(1, 5, 0, eye).Set(4, 5, 0, eye);
        v.Box(2, 4, -1, 3, 4, -1, beak);
        v.Set(2, 0, 4, feet).Set(3, 0, 4, feet);
        return v.ToMesh(0.045f);
    });

    // Flecha que señala la salida cuando se abre la puerta
    public static Mesh ExitArrow() => VoxelLib.Cached("exit_arrow", () =>
    {
        VoxelModel v = new VoxelModel();
        var pal = new Dictionary<char, Color> { { 'g', G("#3CF06A") }, { 'w', G("#E8FFE8") } };
        string[] rows =
        {
            "  ggggg  ",
            "  gwwwg  ",
            "  gwwwg  ",
            "  gwwwg  ",
            "ggggwgggg",
            " gwwwwwg ",
            "  gwwwg  ",
            "   gwg   ",
            "    g    ",
        };
        v.Extrude(rows, pal, 0, 0, 0, 2);
        return v.ToMesh(0.07f);
    });

    // Banderín triangular para las guirnaldas
    public static Mesh Banderin(Color color) => VoxelLib.Cached("banderin" + ColorUtility.ToHtmlStringRGB(color), () =>
    {
        VoxelModel v = new VoxelModel();
        for (int y = 0; y < 6; y++)
        {
            int half = y / 2;
            v.Box(-half, y, 0, half, y, 0, color);
        }
        return v.ToMesh(0.05f);
    });

    // Farolillo de feria (brilla)
    public static Mesh Farolillo(Color color) => VoxelLib.Cached("farolillo" + ColorUtility.ToHtmlStringRGB(color), () =>
    {
        VoxelModel v = new VoxelModel();
        Color glow = VoxelModel.Glow(color);
        Color dots = VoxelModel.Glow(Color.Lerp(color, Color.white, 0.7f));
        v.Sphere(3, 3, 3, 3f, glow, 0.85f);
        v.Set(3, 5, 0, dots).Set(1, 3, 0, dots).Set(5, 2, 0, dots).Set(3, 1, 1, dots);
        v.CylinderY(3, 6, 3, 1f, 1, C("#333333"));
        v.CylinderY(3, -1, 3, 1f, 1, C("#333333"));
        return v.ToMesh(0.045f);
    });

    // ======================================================================
    //  FONDO: CIELO
    // ======================================================================

    public static Mesh Sun(bool sunglasses) => VoxelLib.Cached("sun" + sunglasses, () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.02f };
        Color core = G("#FFD34D"), rim = G("#FFB52E"), ray = G("#FFE38A");
        for (int x = -7; x <= 7; x++)
            for (int y = -7; y <= 7; y++)
            {
                float d = Mathf.Sqrt(x * x + y * y);
                if (d <= 7.2f) v.Set(x, y, 0, d > 6f ? rim : core);
            }
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI * 2f / 12f;
            Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
            v.Line(dir * 8.6f, dir * (i % 2 == 0 ? 11f : 10f), ray);
        }
        if (sunglasses)
        {
            Color glass = G("#1A1A22"), shine = G("#6E7BA8"), smile = G("#C9661A");
            v.Box(-5, 0, -1, -1, 2, -1, glass).Box(1, 0, -1, 5, 2, -1, glass).Box(-1, 2, -1, 1, 2, -1, glass);
            v.Set(-4, 2, -1, shine).Set(2, 2, -1, shine);
            v.Box(-6, 2, -1, -6, 2, -1, glass).Box(6, 2, -1, 6, 2, -1, glass);
            v.Set(-3, -3, -1, smile).Set(3, -3, -1, smile).Box(-2, -4, -1, 2, -4, -1, smile);
        }
        return v.ToMesh(0.1f, true);
    });

    public static Mesh Moon() => VoxelLib.Cached("moon", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Color c = G("#F4F1D0"), crater = G("#D9D4AE");
        for (int x = -6; x <= 6; x++)
            for (int y = -6; y <= 6; y++)
            {
                if (x * x + y * y > 37) continue;
                float dx = x - 3f, dy = y - 1.5f;
                if (dx * dx + dy * dy < 26f) continue;
                v.Set(x, y, 0, c);
            }
        v.Set(-4, 1, -1, crater).Set(-3, -3, -1, crater).Set(-1, -5, -1, crater);
        return v.ToMesh(0.1f, true);
    });

    public static Mesh Cloud(int variant) => VoxelLib.Cached("cloud" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 53 + 11);
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Color white = C("#FFFFFF"), shade = C("#DCE6F2");
        int puffs = 3 + rng.Next(3);
        float x = 0;
        for (int i = 0; i < puffs; i++)
        {
            float r = 2.5f + (float)rng.NextDouble() * 2.5f;
            v.Sphere(x, r * 0.6f, 0, r, white, 0.7f);
            x += r * 1.3f;
        }
        // Base plana y algo más oscura
        v.Box(-3, 0, -2, Mathf.RoundToInt(x), 0, 2, shade);
        return v.ToMesh(0.13f);
    });

    public static Mesh Bird() => VoxelLib.Cached("bird", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0f };
        Color c = C("#2B2B33");
        v.Set(0, 2, 0, c).Set(1, 1, 0, c).Set(2, 0, 0, c).Set(3, 1, 0, c).Set(4, 2, 0, c);
        return v.ToMesh(0.09f, true);
    });

    // ======================================================================
    //  FONDO: EDIFICIOS Y PAISAJE
    // ======================================================================

    // Casa de pueblo con balcones y contraventanas
    public static Mesh House(int variant, bool night) => VoxelLib.Cached("house" + variant + night, () =>
    {
        System.Random rng = new System.Random(variant * 977 + 13);
        VoxelModel v = new VoxelModel();
        int w = 10 + rng.Next(6), h = 8 + rng.Next(6), d = 6;
        Color wall = Pick(rng, "#F4F1E8", "#E8C27A", "#E89A7A", "#F2DA8B", "#D9E4EC", "#F0B8A0");
        Color roof = Pick(rng, "#B8502E", "#A9442A", "#C2633A");
        Color roofDark = C("#7E3520");
        Color shutter = Pick(rng, "#3E7A4B", "#2F5D8C", "#7A4A2A", "#5E8F3A");
        Color glass = C("#2B3A55"), lit = G("#FFD27A"), rail = C("#222222");
        Color[] flowers = { C("#E0283A"), C("#FF5C8A"), C("#FFFFFF") };

        v.Box(0, 0, 0, w - 1, h - 1, d - 1, wall);
        for (int k = 0; k <= d / 2; k++) v.Box(-1, h + k, k, w, h + k, d - 1 - k, roof);
        v.Box(-1, h, -1, w, h, -1, roofDark);

        for (int fy = 3; fy < h - 1; fy += 4)
        {
            for (int wx = 1; wx < w - 2; wx += 4)
            {
                bool on = night && rng.NextDouble() < 0.6;
                v.Box(wx, fy, 0, wx + 1, fy + 1, 0, on ? lit : glass);
                v.Box(wx - 1, fy, -1, wx - 1, fy + 1, -1, shutter).Box(wx + 2, fy, -1, wx + 2, fy + 1, -1, shutter);
                if (rng.NextDouble() < 0.7)
                {
                    v.Box(wx - 1, fy - 1, -1, wx + 2, fy - 1, -2, rail);
                    if (rng.NextDouble() < 0.8) v.Set(wx + rng.Next(2), fy, -2, flowers[rng.Next(flowers.Length)]);
                }
            }
        }
        int door = w / 2;
        v.Box(door - 1, 0, 0, door, 2, 0, C("#6B3E1F"));
        if (rng.NextDouble() < 0.5) v.Box(w - 3, h + 1, 2, w - 2, h + 4, 3, C("#8C8C8C")); // chimenea
        return v.ToMesh(0.12f);
    });

    // Campanario de iglesia
    public static Mesh Tower(int variant) => VoxelLib.Cached("tower" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 41 + 3);
        VoxelModel v = new VoxelModel();
        int h = 18 + rng.Next(6);
        Color stone = Pick(rng, "#D9C9A3", "#CDB78E", "#E3D6B8"), dark = C("#3A2C22"), roof = C("#7E8B92"), bell = C("#D4A53A");
        v.Box(0, 0, 0, 5, h, 5, stone);
        v.Box(-1, h - 6, -1, 6, h - 6, 6, stone);
        for (int y = h - 4; y <= h - 2; y++)
        {
            v.Set(2, y, 0, dark).Set(3, y, 0, dark).Set(0, y, 2, dark).Set(0, y, 3, dark).Set(5, y, 2, dark).Set(5, y, 3, dark);
        }
        v.Set(2, h - 4, 0, bell).Set(3, h - 4, 0, bell);
        for (int k = 0; k < 4; k++) v.Box(-1 + k, h + 1 + k, -1 + k, 6 - k, h + 1 + k, 6 - k, roof);
        v.Box(2, h + 5, 2, 3, h + 8, 3, C("#2B2B2B")).Box(1, h + 7, 2, 4, h + 7, 3, C("#2B2B2B"));
        // Nido de cigüeña, cómo no
        v.Box(1, h + 9, 1, 4, h + 9, 4, C("#7A5A33")).Set(2, h + 10, 2, C("#FFFFFF")).Set(2, h + 11, 2, C("#FFFFFF")).Set(2, h + 11, 1, C("#E0623A"));
        return v.ToMesh(0.12f);
    });

    // Plaza de toros vista desde fuera
    public static Mesh Bullring() => VoxelLib.Cached("bullring", () =>
    {
        VoxelModel v = new VoxelModel();
        Color ochre = C("#E2B36A"), white = C("#F6EFE0"), arch = C("#8C2A1E"), red = C("#C8102E"), yellow = C("#FFC400");
        const int w = 64;
        for (int x = 0; x < w; x++)
        {
            int z0 = Mathf.RoundToInt((x - w / 2f) * (x - w / 2f) / 110f);
            v.Box(x, 0, z0, x, 8, z0 + 3, ochre);
            v.Box(x, 9, z0, x, 9, z0 + 3, white);
            v.Set(x, 0, z0, white);
            if (x % 5 == 1 || x % 5 == 2)
            {
                v.Set(x, 3, z0, arch).Set(x, 4, z0, arch).Set(x, 5, z0, arch);
                v.Set(x, 7, z0, arch);
            }
            if (x % 16 == 8)
            {
                v.Box(x, 10, z0 + 1, x, 15, z0 + 1, C("#3A3A3A"));
                v.Box(x + 1, 13, z0 + 1, x + 4, 13, z0 + 1, red).Box(x + 1, 12, z0 + 1, x + 4, 12, z0 + 1, yellow).Box(x + 1, 11, z0 + 1, x + 4, 11, z0 + 1, red);
            }
        }
        return v.ToMesh(0.12f);
    });

    // Edificio modernista de ensanche con balcones
    public static Mesh Eixample(int variant, bool night) => VoxelLib.Cached("eixample" + variant + night, () =>
    {
        System.Random rng = new System.Random(variant * 7 + 99);
        VoxelModel v = new VoxelModel();
        int w = 13 + rng.Next(4), h = 16 + rng.Next(6), d = 8;
        Color wall = Pick(rng, "#E7CFA0", "#D9B98A", "#E9D9C0", "#CFA77A"), rail = C("#1E1E1E"), glass = C("#33405A"), lit = G("#FFDB8A");
        v.Box(0, 0, 0, w - 1, h - 1, d - 1, wall);
        v.ClearBox(0, 0, 0, 0, h - 1, 0).ClearBox(w - 1, 0, 0, w - 1, h - 1, 0);
        for (int fy = 3; fy < h - 1; fy += 3)
            for (int wx = 2; wx < w - 2; wx += 3)
            {
                v.Box(wx, fy, 0, wx, fy + 1, 0, night && rng.NextDouble() < 0.55 ? lit : glass);
                v.Box(wx - 1, fy - 1, -1, wx + 1, fy - 1, -1, rail);
            }
        v.Box(0, h, 0, w - 1, h, 0, C("#BFA57E"));
        v.Box(2, h, 3, 4, h + 2, 5, C("#9FA8AE"));
        // Bajo comercial con toldo
        Color awning = Pick(rng, "#2F8F5B", "#C8323A", "#2F6FB0");
        v.Box(1, 2, -1, w - 2, 2, -2, awning);
        return v.ToMesh(0.11f);
    });

    // Basílica de torres modernistas (silueta genérica)
    public static Mesh Spires() => VoxelLib.Cached("spires", () =>
    {
        VoxelModel v = new VoxelModel();
        Color stone = C("#CDB78E"), dark = C("#8E7A58");
        Color[] tops = { C("#E0283A"), C("#FFC400"), C("#3FA34D"), C("#FFFFFF") };
        int[] heights = { 30, 38, 38, 30 };
        for (int t = 0; t < 4; t++)
        {
            int cx = 3 + t * 6;
            int hgt = heights[t];
            for (int y = 0; y < hgt; y++)
            {
                float r = Mathf.Lerp(2.6f, 0.9f, y / (float)hgt);
                v.CylinderY(cx, y, 3, r, 1, (y % 4 == 0) ? dark : stone);
            }
            v.Sphere(cx, hgt + 1, 3, 1.6f, tops[t]);
        }
        v.Box(0, 0, 1, 23, 10, 6, stone);
        for (int x = 2; x < 22; x += 3) v.Box(x, 3, 1, x, 7, 1, dark);
        return v.ToMesh(0.1f);
    });

    // Rascacielos de Benidorm (o de Magaluf, de noche)
    public static Mesh Skyscraper(int variant, bool night) => VoxelLib.Cached("sky" + variant + night, () =>
    {
        System.Random rng = new System.Random(variant * 313 + 5);
        VoxelModel v = new VoxelModel();
        int w = 8 + rng.Next(4), h = 26 + rng.Next(20), d = 7;
        Color glass = night ? C("#1C2433") : Pick(rng, "#7FB2DA", "#9CC7E0", "#6FA0C8", "#A8D0D8");
        Color frame = night ? C("#3A4058") : Pick(rng, "#EEF2F5", "#F5E9D6", "#E0E6EA");
        Color lit = G(rng.NextDouble() < 0.5 ? "#FFE08A" : "#FFC27A");
        v.Box(0, 0, 0, w - 1, h - 1, d - 1, glass);
        for (int y = 0; y < h; y += 3) v.Paint(0, y, 0, w - 1, y, d - 1, frame);
        v.Paint(0, 0, 0, 0, h - 1, d - 1, frame).Paint(w - 1, 0, 0, w - 1, h - 1, d - 1, frame);
        if (night)
        {
            for (int y = 1; y < h; y++)
            {
                if (y % 3 == 0) continue;
                for (int x = 1; x < w - 1; x++)
                    if (rng.NextDouble() < 0.35) v.Set(x, y, 0, lit);
            }
        }
        else
        {
            // Balcones con toallas de colores tendidas
            Color[] towels = { C("#FF5C8A"), C("#FFC400"), C("#3FB5E0"), C("#FFFFFF") };
            for (int y = 2; y < h; y += 3)
                if (rng.NextDouble() < 0.5) v.Set(1 + rng.Next(w - 2), y, -1, towels[rng.Next(towels.Length)]);
        }
        v.Box(w / 2, h, d / 2, w / 2, h + 5, d / 2, C("#C0C0C0"));
        v.Set(w / 2, h + 6, d / 2, G("#FF3030"));
        return v.ToMesh(0.1f);
    });

    public static Mesh Palm(int variant) => VoxelLib.Cached("palm" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 17 + 1);
        VoxelModel v = new VoxelModel();
        Color trunk = C("#8B5A2B"), trunkDark = C("#6B4421"), leaf = C("#2E8B3E"), leaf2 = C("#4CAF50"), coco = C("#5A3A1A");
        int h = 16 + rng.Next(6);
        float bend = (float)(rng.NextDouble() * 2 - 1) * 4f;
        Vector3 top = Vector3.zero;
        for (int i = 0; i <= h; i++)
        {
            float t = i / (float)h;
            int x = Mathf.RoundToInt(bend * t * t);
            v.Box(x, i, 0, x + 1, i, 1, (i % 2 == 0) ? trunk : trunkDark);
            top = new Vector3(x, i, 0);
        }
        for (int f = 0; f < 7; f++)
        {
            float a = f * Mathf.PI * 2f / 7f + (float)rng.NextDouble() * 0.3f;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
            for (int j = 0; j <= 8; j++)
            {
                Vector3 p = top + dir * j + Vector3.up * (1.5f - j * j / 14f);
                v.Set(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z), (j % 2 == 0) ? leaf : leaf2);
                v.Set(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y) - 1, Mathf.RoundToInt(p.z), leaf);
            }
        }
        v.Sphere(top.x, top.y - 1, top.z - 1, 1f, coco);
        v.Sphere(top.x + 1, top.y - 1, top.z + 1, 1f, coco);
        return v.ToMesh(0.11f);
    });

    // Molino manchego (el cuerpo; las aspas van aparte para girar)
    public static Mesh WindmillBody() => VoxelLib.Cached("windmill_body", () =>
    {
        VoxelModel v = new VoxelModel();
        Color white = C("#F2F2EE"), stone = C("#A9A39A"), roof = C("#3A3A3A"), door = C("#5A3A1A"), win = C("#2A3040");
        for (int y = 0; y < 14; y++)
        {
            float r = Mathf.Lerp(4.2f, 3.4f, y / 13f);
            v.CylinderY(5, y, 5, r, 1, y < 2 ? stone : white);
        }
        v.Cone(5, 14, 5, 4.6f, 5, roof);
        v.Box(4, 0, 0, 6, 3, 1, door);
        v.Box(5, 8, 1, 5, 9, 1, win);
        v.Box(3, 11, 1, 3, 11, 1, win);
        return v.ToMesh(0.12f);
    });

    public static Mesh WindmillSails() => VoxelLib.Cached("windmill_sails", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.02f };
        Color wood = C("#7A5230"), cloth = C("#E9E2D0");
        for (int s = 0; s < 4; s++)
        {
            Vector2 dir = s == 0 ? Vector2.up : s == 1 ? Vector2.right : s == 2 ? Vector2.down : Vector2.left;
            Vector2 side = new Vector2(dir.y, -dir.x);
            for (int i = 1; i <= 13; i++)
            {
                Vector2 p = dir * i;
                v.Set(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), 0, wood);
                if (i >= 4)
                    for (int k = 1; k <= 3; k++)
                    {
                        Vector2 q = p + side * k;
                        bool lattice = (i % 3 == 0) || k == 3;
                        v.Set(Mathf.RoundToInt(q.x), Mathf.RoundToInt(q.y), 0, lattice ? wood : cloth);
                    }
            }
        }
        v.Box(-1, -1, -1, 1, 1, 0, wood);
        return v.ToMesh(0.12f, true);
    });

    public static Mesh Castle() => VoxelLib.Cached("castle", () =>
    {
        VoxelModel v = new VoxelModel();
        Color stone = C("#BFAF8E"), dark = C("#8E7F62"), gate = C("#3A2C22");
        v.Box(0, 0, 0, 27, 7, 5, stone);
        for (int x = 0; x <= 27; x += 2) v.Set(x, 8, 0, stone).Set(x, 8, 5, stone);
        v.Box(-2, 0, -1, 4, 12, 6, stone).Box(23, 0, -1, 29, 12, 6, stone);
        v.Box(10, 0, -1, 17, 15, 6, stone);
        for (int x = -2; x <= 4; x += 2) v.Set(x, 13, -1, stone);
        for (int x = 23; x <= 29; x += 2) v.Set(x, 13, -1, stone);
        for (int x = 10; x <= 17; x += 2) v.Set(x, 16, -1, stone);
        v.Box(12, 0, -1, 15, 4, -1, gate);
        v.Paint(-2, 0, -1, 29, 1, 6, dark);
        v.Box(1, 9, -1, 1, 10, -1, gate).Box(26, 9, -1, 26, 10, -1, gate).Box(13, 10, -1, 14, 12, -1, gate);
        return v.ToMesh(0.1f);
    });

    // Silueta de toro negro de carretera
    public static Mesh BullSilhouette() => VoxelLib.Cached("bull_sil", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.02f };
        var pal = new Dictionary<char, Color> { { '#', C("#111111") } };
        string[] rows =
        {
            "  #  #                            ",
            "  ## ##                           ",
            "   ####                           ",
            "   #####    #######               ",
            "  ###########################     ",
            "  ############################  # ",
            "   ############################ # ",
            "     ##########################  #",
            "      #########################   ",
            "      #######################     ",
            "       ####  ######   #######     ",
            "       ###     ##      ##  ##     ",
            "       ##      ##      ##   ##    ",
            "       ##      ##      ##   ##    ",
            "      ###     ###     ###  ###    ",
        };
        v.Extrude(rows, pal, 0, 0, 0, 2);
        return v.ToMesh(0.12f);
    });

    public static Mesh Hill(int variant, Color top, Color side) => VoxelLib.Cached("hill" + variant + ColorUtility.ToHtmlStringRGB(top), () =>
    {
        System.Random rng = new System.Random(variant * 23 + 2);
        VoxelModel v = new VoxelModel { jitter = 0.08f };
        int w = 40 + rng.Next(30);
        float hgt = 6 + rng.Next(8);
        for (int x = 0; x < w; x++)
        {
            float t = (x - w / 2f) / (w / 2f);
            int hh = Mathf.RoundToInt(hgt * Mathf.Sqrt(Mathf.Max(0f, 1f - t * t)));
            if (hh <= 0) continue;
            v.Box(x, 0, 0, x, hh - 1, 5, side);
            v.Box(x, hh, 0, x, hh, 5, top);
        }
        return v.ToMesh(0.14f);
    });

    public static Mesh OliveTree(int variant) => VoxelLib.Cached("olive" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 61 + 4);
        VoxelModel v = new VoxelModel();
        v.Box(2, 0, 2, 3, 3, 3, C("#6B5640"));
        v.Sphere(2.5f, 5, 2.5f, 3f, Pick(rng, "#6E8B3D", "#7C9A4A", "#5E7A35"), 0.7f);
        return v.ToMesh(0.12f);
    });

    // Cartel azul de autovía
    public static Mesh RoadSign(string text) => VoxelLib.Cached("sign" + text, () =>
    {
        VoxelModel v = new VoxelModel();
        Color blue = C("#1F5FBF"), white = C("#FFFFFF"), pole = C("#9AA0A6");
        int w = text.Length * 4 + 5;
        v.Box(0, 8, 0, w - 1, 16, 0, white);
        v.Box(1, 9, 0, w - 2, 15, 0, blue);
        v.Text(text, 3, 10, -1, 1, white);
        v.Box(2, 0, 1, 2, 8, 1, pole).Box(w - 3, 0, 1, w - 3, 8, 1, pole);
        return v.ToMesh(0.12f);
    });

    public static Mesh Car(int variant) => VoxelLib.Cached("car" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 101 + 9);
        VoxelModel v = new VoxelModel();
        Color body = Pick(rng, "#D62828", "#F2F2F2", "#1D3557", "#FFB703", "#2A9D8F", "#E76F51");
        Color win = C("#9FD3F0"), tyre = C("#1A1A1A"), light = G("#FFF2B0");
        v.Box(0, 1, 0, 10, 3, 4, body);
        v.Box(2, 4, 0, 7, 5, 4, body);
        v.Box(3, 4, -1, 6, 5, -1, win).Paint(2, 4, 0, 7, 5, 0, win);
        v.Box(10, 2, 0, 10, 2, 1, light);
        v.CylinderZ(2, 1, -1, 1.2f, 1, tyre).CylinderZ(8, 1, -1, 1.2f, 1, tyre);
        if (rng.NextDouble() < 0.4) v.Box(3, 6, 1, 6, 6, 3, C("#7A5230")); // maletas en la baca
        return v.ToMesh(0.09f);
    });

    // Caseta de feria de rayas
    public static Mesh Caseta(int variant) => VoxelLib.Cached("caseta" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 19 + 8);
        VoxelModel v = new VoxelModel();
        Color stripe = Pick(rng, "#C8102E", "#2F8F5B", "#E86A92");
        Color white = C("#FFFFFF");
        int w = 12;
        for (int x = 0; x < w; x++)
        {
            Color c = (x / 2) % 2 == 0 ? stripe : white;
            v.Box(x, 0, 0, x, 6, 6, c);
            for (int k = 0; k <= 3; k++) v.Box(x, 7 + k, k, x, 7 + k, 6 - k, c);
            if (x % 2 == 0) v.Set(x, 6, -1, c);
        }
        v.Box(3, 0, 0, 8, 4, 0, C("#3A2418"));
        Color[] bulbs = { G("#FFD84A"), G("#FF6B6B"), G("#7CF08A"), G("#7AC8FF") };
        for (int x = 0; x < w; x += 2) v.Set(x, 5, -2, bulbs[rng.Next(bulbs.Length)]);
        return v.ToMesh(0.11f);
    });

    // Portada de la feria, llena de bombillas
    public static Mesh Portada() => VoxelLib.Cached("portada", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.03f };
        Color frame = C("#F4EDE0"), bulb = G("#FFE07A"), bulb2 = G("#FFFFFF"), red = G("#FF5A4A");
        const int w = 34;
        v.Box(0, 0, 0, w - 1, 24, 2, frame);
        // Tres arcos
        int[] centers = { 7, 17, 27 };
        foreach (int cx in centers)
        {
            for (int x = -4; x <= 4; x++)
                for (int y = 0; y <= 12 + Mathf.RoundToInt(Mathf.Sqrt(Mathf.Max(0, 16 - x * x)) * 1.2f); y++)
                    v.Clear(cx + x, y, 0).Clear(cx + x, y, 1).Clear(cx + x, y, 2);
        }
        // Remate escalonado
        for (int k = 0; k < 6; k++) v.Box(k * 2, 25 + k, 0, w - 1 - k * 2, 25 + k, 2, frame);
        v.Box(15, 31, 0, 18, 34, 2, frame);
        // Bombillas en todos los bordes frontales
        for (int x = 0; x < w; x++)
            for (int y = 0; y < 36; y++)
            {
                if (!v.Has(x, y, 0)) continue;
                bool edge = !v.Has(x - 1, y, 0) || !v.Has(x + 1, y, 0) || !v.Has(x, y + 1, 0) || !v.Has(x, y - 1, 0);
                if (edge && (x + y) % 2 == 0) v.Set(x, y, -1, ((x + y) % 6 == 0) ? red : ((x * y) % 3 == 0 ? bulb2 : bulb));
            }
        return v.ToMesh(0.1f);
    });

    // Hotel con letrero de neón
    public static Mesh Hotel(int variant, string sign, bool night) => VoxelLib.Cached("hotel" + variant + sign + night, () =>
    {
        System.Random rng = new System.Random(variant * 71 + 2);
        VoxelModel v = new VoxelModel();
        int w = Mathf.Max(sign.Length * 4 + 3, 11 + rng.Next(4)), h = 18 + rng.Next(10), d = 7;
        Color wall = night ? C("#2A2D45") : Pick(rng, "#F2EDE3", "#F6D8B8", "#DDE8EE");
        Color glass = night ? C("#161A28") : C("#6C9BC2"), lit = G("#FFE08A");
        Color neon = night ? G(rng.NextDouble() < 0.5 ? "#FF4FD8" : "#3FF2FF") : C("#E03A3A");
        v.Box(0, 0, 0, w - 1, h - 1, d - 1, wall);
        for (int y = 2; y < h - 1; y += 2)
            for (int x = 1; x < w - 1; x += 2)
                v.Set(x, y, 0, night && rng.NextDouble() < 0.45 ? lit : glass);
        for (int y = 2; y < h - 1; y += 4) v.Box(0, y - 1, -1, w - 1, y - 1, -1, night ? C("#3B3F5C") : C("#FFFFFF"));
        v.Text(sign, 2, h + 1, 1, 1, neon);
        v.Box(1, h, 2, w - 2, h, 2, C("#444444"));
        return v.ToMesh(0.1f);
    });

    public static Mesh Congreso() => VoxelLib.Cached("congreso", () =>
    {
        VoxelModel v = new VoxelModel();
        Color stone = C("#EDE3CF"), shadow = C("#C9BBA0"), door = C("#4A2C1A"), bronze = C("#7A5A2E"), red = C("#C8102E"), yellow = C("#FFC400");
        const int w = 48;
        v.Box(0, 0, 0, w - 1, 2, 10, shadow);
        v.Box(2, 3, 2, w - 3, 16, 10, stone);
        // Escalinata
        for (int k = 0; k < 3; k++) v.Box(14 + k, k, -3 + k, 33 - k, k, -1 + k, stone);
        // Columnas
        for (int c = 0; c < 6; c++)
        {
            int cx = 15 + c * 3;
            v.CylinderY(cx, 3, 0, 0.9f, 12, stone);
        }
        v.Box(14, 15, -1, 33, 16, 2, stone);
        // Frontón
        for (int k = 0; k < 6; k++) v.Box(14 + k * 2, 17 + k, -1, 33 - k * 2, 17 + k, 2, stone);
        v.Box(22, 4, 2, 25, 10, 2, door);
        // Ventanas laterales
        for (int x = 4; x < w - 4; x += 3)
            if (x < 13 || x > 34) v.Box(x, 7, 1, x, 11, 1, shadow);
        // Leones de bronce
        int[] lions = { 8, 39 };
        foreach (int lx in lions)
        {
            v.Box(lx - 2, 3, -4, lx + 2, 5, 0, shadow);
            v.Box(lx - 1, 6, -4, lx + 1, 8, 0, bronze);
            v.Box(lx - 2, 8, -5, lx + 2, 10, -3, bronze);
            v.Set(lx, 9, -6, C("#3A2810"));
        }
        // Banderas
        int[] flags = { 6, 41 };
        foreach (int fx in flags)
        {
            v.Box(fx, 17, 5, fx, 27, 5, C("#8A8A8A"));
            v.Box(fx + 1, 26, 5, fx + 7, 26, 5, red).Box(fx + 1, 24, 5, fx + 7, 25, 5, yellow).Box(fx + 1, 23, 5, fx + 7, 23, 5, red);
        }
        return v.ToMesh(0.1f);
    });

    public static Mesh Umbrella(int variant) => VoxelLib.Cached("umbrella" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 13 + 77);
        VoxelModel v = new VoxelModel();
        Color a = Pick(rng, "#E63946", "#1D7FD8", "#FFB703", "#2A9D8F"), b = C("#FFFFFF");
        v.Box(5, 0, 5, 5, 8, 5, C("#CCCCCC"));
        for (int y = 0; y < 3; y++)
        {
            float r = 5.5f - y * 1.8f;
            for (int x = -6; x <= 6; x++)
                for (int z = -6; z <= 6; z++)
                    if (x * x + z * z <= r * r)
                    {
                        float ang = Mathf.Atan2(z, x);
                        Color c = (Mathf.FloorToInt((ang + Mathf.PI) / (Mathf.PI / 4f)) % 2 == 0) ? a : b;
                        v.Set(5 + x, 8 + y, 5 + z, c);
                    }
        }
        // Toalla y nevera
        v.Box(0, 0, 0, 3, 0, 6, rng.NextDouble() < 0.5 ? C("#FF5C8A") : C("#3FB5E0"));
        v.Box(8, 0, 1, 10, 2, 3, C("#2F6FB0")).Box(8, 3, 1, 10, 3, 3, C("#FFFFFF"));
        return v.ToMesh(0.1f);
    });

    public static Mesh WaveCap() => VoxelLib.Cached("wavecap", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0.02f };
        Color c = C("#E8F6FF");
        v.Box(0, 0, 0, 5, 0, 1, c).Box(1, 1, 0, 3, 1, 1, c);
        return v.ToMesh(0.1f);
    });

    // Barco de vela pequeño para el mar
    public static Mesh Sailboat(int variant) => VoxelLib.Cached("boat" + variant, () =>
    {
        System.Random rng = new System.Random(variant * 3 + 21);
        VoxelModel v = new VoxelModel();
        Color hull = Pick(rng, "#FFFFFF", "#D62828", "#1D3557"), sail = C("#FFF8E8"), stripe = Pick(rng, "#E63946", "#1D7FD8", "#FFB703");
        v.Box(0, 0, 0, 10, 1, 3, hull).Box(1, -1, 1, 9, -1, 2, hull);
        v.Box(5, 2, 2, 5, 12, 2, C("#6B4421"));
        for (int y = 3; y <= 11; y++)
        {
            int len = Mathf.RoundToInt((12 - y) * 0.6f);
            v.Box(6, y, 2, 6 + len, y, 2, (y == 6) ? stripe : sail);
        }
        return v.ToMesh(0.1f);
    });

    // Rayo para las tormentas del Congreso
    public static Mesh Lightning() => VoxelLib.Cached("lightning", () =>
    {
        VoxelModel v = new VoxelModel { jitter = 0f };
        Color c = G("#FFFDE0");
        Vector3[] pts = { new Vector3(0, 30, 0), new Vector3(3, 22, 0), new Vector3(0, 18, 0), new Vector3(4, 9, 0), new Vector3(1, 6, 0), new Vector3(3, 0, 0) };
        for (int i = 0; i < pts.Length - 1; i++) v.Line(pts[i], pts[i + 1], c);
        return v.ToMesh(0.12f);
    });
}
