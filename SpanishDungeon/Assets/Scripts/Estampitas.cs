using UnityEngine;
using System.Collections.Generic;

// "Las estampitas de la abuela": al salir de las salas 1, 4 y 7 eliges una de tres
// bendiciones que duran toda la partida. Cada partida sale una combinación distinta.
public static class Estampitas
{
    public enum Id { SanCristobal, Iman, Corazon, Rosario, Botijo, Hucha, Cartera, Pijama, Zapatillas, Decimo }

    public class Def
    {
        public Id id;
        public string name, saint, desc;
        public Color color;
    }

    public static readonly List<Def> All = new List<Def>
    {
        new Def { id = Id.SanCristobal, name = "San Cristóbal", saint = "Patrón de los que van con prisa",
                  desc = "El suelo tarda más en hundirse y cae más despacio.", color = new Color(0.55f, 0.75f, 1f) },
        new Def { id = Id.Iman, name = "Imán de Benidorm", saint = "Recuerdo de la nevera de la abuela",
                  desc = "Las monedas cercanas vuelan hacia ti.", color = new Color(1f, 0.6f, 0.3f) },
        new Def { id = Id.Corazon, name = "Corazón de oro", saint = "De la Virgen del pueblo",
                  desc = "+1 corazón máximo y te cura uno ahora.", color = new Color(1f, 0.35f, 0.4f) },
        new Def { id = Id.Rosario, name = "Rosario de nácar", saint = "Para rezar entre golpe y golpe",
                  desc = "Los combos duran el doble y cada combo da una moneda más.", color = new Color(0.9f, 0.85f, 1f) },
        new Def { id = Id.Botijo, name = "Botijo fresquito", saint = "Agua del manantial, como Dios manda",
                  desc = "Empiezas cada sala con un abanico que para un golpe.", color = new Color(0.85f, 0.6f, 0.4f) },
        new Def { id = Id.Hucha, name = "Hucha de cerdito", saint = "Lo que sobra de la paga",
                  desc = "+2 monedas cada vez que pasas de sala.", color = new Color(1f, 0.65f, 0.75f) },
        new Def { id = Id.Cartera, name = "Cartera con cadena", saint = "Bien sujeta al cinturón",
                  desc = "Ni carteristas, ni multas, ni gaviotas: no te quitan monedas.", color = new Color(0.75f, 0.55f, 0.35f) },
        new Def { id = Id.Pijama, name = "Pijama de franela", saint = "Siesta de pijama y orinal",
                  desc = "Tras un golpe eres invulnerable el doble de tiempo.", color = new Color(0.55f, 0.85f, 0.75f) },
        new Def { id = Id.Zapatillas, name = "Zapatillas de estar por casa", saint = "Con suela de goma, para correr",
                  desc = "Te mueves un 20 % más rápido.", color = new Color(0.6f, 0.9f, 0.45f) },
        new Def { id = Id.Decimo, name = "Décimo bendecido", saint = "Frotado en la chepa de un jorobado",
                  desc = "El Gordo de la lotería toca con un tercio menos de monedas.", color = new Color(1f, 0.85f, 0.2f) },
    };

    private static readonly HashSet<Id> owned = new HashSet<Id>();
    public static int Count => owned.Count;

    public static bool Has(Id id) => owned.Contains(id);

    public static void Reset() => owned.Clear();

    public static bool OffersAfterRoom(int room) => room == 1 || room == 4 || room == 7;

    public static Def Get(Id id) => All.Find(d => d.id == id);

    // Tres estampitas al azar que aún no tienes
    public static List<Def> Offer()
    {
        List<Def> pool = new List<Def>();
        foreach (Def d in All) if (!owned.Contains(d.id)) pool.Add(d);
        for (int i = 0; i < pool.Count; i++)
        {
            int j = Random.Range(i, pool.Count);
            Def t = pool[i]; pool[i] = pool[j]; pool[j] = t;
        }
        if (pool.Count > 3) pool.RemoveRange(3, pool.Count - 3);
        return pool;
    }

    public static void Take(Id id, PlayerStats stats)
    {
        if (!owned.Add(id)) return;

        // Álbum: cuántas distintas has conseguido entre todas tus partidas
        string key = "SD_EST_" + id;
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
        if (CollectedEver >= All.Count) Achievements.Unlock("album");

        if (stats == null) return;
        switch (id)
        {
            case Id.Corazon: stats.AddMaxHeart(); break;
            case Id.Botijo:
                PlayerPowerUps pu = stats.GetComponent<PlayerPowerUps>();
                if (pu != null) pu.GiveShield();
                break;
        }
        stats.RefreshUI();
    }

    public static int CollectedEver
    {
        get
        {
            int n = 0;
            foreach (Def d in All) if (PlayerPrefs.GetInt("SD_EST_" + d.id, 0) == 1) n++;
            return n;
        }
    }

    public static string OwnedNames()
    {
        List<string> names = new List<string>();
        foreach (Def d in All) if (owned.Contains(d.id)) names.Add(d.name);
        return string.Join(", ", names);
    }

    // ---------- Efectos ----------

    public static float CrumbleDelayBonus => Has(Id.SanCristobal) ? 2.5f : 0f;
    public static float CrumbleSlowdown => Has(Id.SanCristobal) ? 1.35f : 1f;
    public static float ComboWindowMultiplier => Has(Id.Rosario) ? 2f : 1f;
    public static float InvincibilityMultiplier => Has(Id.Pijama) ? 2f : 1f;
    public static float SpeedMultiplier => Has(Id.Zapatillas) ? 1.2f : 1f;
    public static bool ProtectsCoins => Has(Id.Cartera);
    public static int LotteryCost(int baseCost) => Has(Id.Decimo) ? Mathf.Max(3, Mathf.RoundToInt(baseCost * 2f / 3f)) : baseCost;
    public const float MagnetRadius = 2.6f;
}
