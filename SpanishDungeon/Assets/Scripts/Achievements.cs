using UnityEngine;
using System.Collections.Generic;

// Logros guardados en PlayerPrefs. Se desbloquean desde los eventos del juego
// y se anuncian con un cartel en el HUD.
public static class Achievements
{
    public class Def
    {
        public string id, title, desc;
        public Def(string id, string title, string desc) { this.id = id; this.title = title; this.desc = desc; }
    }

    public static readonly List<Def> All = new List<Def>
    {
        new Def("primera",   "¡Olé!",                         "Elimina a tu primer enemigo."),
        new Def("encierro",  "Encierro superado",             "Elimina 3 toros en una misma partida."),
        new Def("carteras",  "Cartera a salvo",               "Elimina 10 carteristas en total."),
        new Def("guiris",    "Guiri go home",                 "Elimina 5 guiris en total."),
        new Def("resaca",    "Resacón en Magaluf",            "Emborráchate 3 veces en una partida."),
        new Def("gordo",     "¡Me ha tocado!",                "Gana la lotería."),
        new Def("mocion",    "Moción de censura",             "Derrota a Pedro."),
        new Def("intocable", "Ni un rasguño",                 "Gana sin recibir ni un golpe."),
        new Def("ave",       "Más rápido que el AVE",         "Gana en menos de 6 minutos."),
        new Def("combo",     "Combo de feria",                "Encadena un combo x4."),
        new Def("rico",      "Más rico que un emérito",       "Recoge 50 monedas en una partida."),
        new Def("vacio",     "Gravedad 1 - Torrento 0",       "Cae al vacío."),
        new Def("primerdia", "Ni el primer día",              "Muere en la primera sala."),
        new Def("multazo",   "Multazo",                       "Recibe 3 multas en una partida."),
        new Def("vandalo",   "Vandalismo cultural",           "Rompe 15 macetas, botijos o cajas en total."),
        new Def("siesta",    "Experto en siestas",            "Usa la almohada de la siesta."),
        new Def("bocata",    "Bocata de calamares",           "Cómete 3 bocadillos en total."),
        new Def("palomas",   "Espantapalomas",                "Espanta 20 palomas en total."),
        new Def("pata",      "Pata negra",                    "Encuentra un jamón."),
        new Def("gatos",     "Amigo de los gatos",            "Espanta a 10 gatos callejeros en total."),
        new Def("escoba",    "Escoba nueva barre bien",       "Limpia 5 salas de enemigos en una partida."),
        new Def("gaviota",   "Atracado por una gaviota",      "Deja que una gaviota te robe una moneda."),
        new Def("modelito",  "Fashion victim",                "Ponle un sombrero a Torrento."),
        new Def("iberico",   "Ibérico de bellota",            "Completa la mazmorra en dificultad Ibérico."),
        new Def("todo",      "Spanish Dungeon al completo",   "Desbloquea todos los demás logros."),
    };

    private const string Prefix = "SD_ACH_";
    private const string CounterPrefix = "SD_CNT_";

    public static bool IsUnlocked(string id) => PlayerPrefs.GetInt(Prefix + id, 0) == 1;

    public static int UnlockedCount
    {
        get
        {
            int n = 0;
            foreach (Def d in All) if (IsUnlocked(d.id)) n++;
            return n;
        }
    }

    public static void Unlock(string id)
    {
        if (IsUnlocked(id)) return;
        Def def = All.Find(d => d.id == id);
        if (def == null) return;

        PlayerPrefs.SetInt(Prefix + id, 1);
        PlayerPrefs.Save();

        if (GameHUD.Instance != null) GameHUD.Instance.ShowAchievement(def.title, def.desc);
        Sfx.Play(SfxType.Achievement, 0.9f, 1f, true);

        if (id != "todo" && UnlockedCount >= All.Count - 1) Unlock("todo");
    }

    public static int AddCounter(string key, int amount = 1)
    {
        int v = PlayerPrefs.GetInt(CounterPrefix + key, 0) + amount;
        PlayerPrefs.SetInt(CounterPrefix + key, v);
        return v;
    }

    // ---------- Eventos ----------

    public static void OnKill(GameObject enemy)
    {
        Unlock("primera");
        if (enemy.GetComponent<EnemyPatrol>() != null)
        {
            RunStats.KillsToro++;
            if (RunStats.KillsToro >= 3) Unlock("encierro");
        }
        else if (enemy.GetComponent<Enemigo_basico>() != null)
        {
            if (AddCounter("carteristas") >= 10) Unlock("carteras");
        }
        else if (enemy.GetComponent<Guiri>() != null)
        {
            if (AddCounter("guiris") >= 5) Unlock("guiris");
        }
    }

    public static void OnCombo(int combo) { if (combo >= 4) Unlock("combo"); }
    public static void OnDrunk() { RunStats.TimesDrunk++; if (RunStats.TimesDrunk >= 3) Unlock("resaca"); }
    public static void OnLottery() => Unlock("gordo");
    public static void OnBossDefeated() => Unlock("mocion");
    public static void OnCoinCollected() { if (RunStats.CoinsCollected >= 50) Unlock("rico"); }
    public static void OnMulta() { RunStats.Multas++; if (RunStats.Multas >= 3) Unlock("multazo"); }
    public static void OnSmash() { if (AddCounter("rotos") >= 15) Unlock("vandalo"); }
    public static void OnSiesta() => Unlock("siesta");
    public static void OnBocadillo() { if (AddCounter("bocadillos") >= 3) Unlock("bocata"); }
    public static void OnPigeon() { if (AddCounter("palomas") >= 20) Unlock("palomas"); }
    public static void OnJamon() => Unlock("pata");
    public static void OnCat() { if (AddCounter("gatos") >= 10) Unlock("gatos"); }
    public static void OnRoomCleared() { if (RunStats.RoomsCleared >= 5) Unlock("escoba"); }
    public static void OnSeagullTheft() => Unlock("gaviota");

    public static void OnDeath(bool fell, int room)
    {
        if (fell) Unlock("vacio");
        if (room <= 1) Unlock("primerdia");
    }

    public static void OnVictory()
    {
        if (RunStats.DamageTaken == 0) Unlock("intocable");
        if (RunStats.ElapsedSeconds < 360f) Unlock("ave");
        if (GameSettings.Difficulty == 2) Unlock("iberico");
    }
}
