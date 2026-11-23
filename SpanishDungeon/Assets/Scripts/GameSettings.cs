using UnityEngine;
using System;

// Opciones del jugador (accesibilidad, audio, interfaz, partida), guardadas en PlayerPrefs
public static class GameSettings
{
    private static bool loaded;

    private static int volume = 10;          // 0..10 (todo el juego)
    private static int musicVolume = 7;      // 0..10
    private static bool newSoundtrack = true;
    private static int shake = 2;            // 0 = no, 1 = suave, 2 = normal
    private static bool flashes = true;
    private static bool hitstop = true;
    private static int hudScale = 0;         // 0 = 100 %, 1 = 125 %, 2 = 150 %
    private static bool holdToMove = true;
    private static bool dangerMarkers = true;
    private static bool showTimer = true;
    private static bool ambientEffects = true;
    private static bool reduceMotion = false;
    private static int difficulty = 1;       // 0 = turista, 1 = normal, 2 = ibérico
    private static int hat = 0;              // índice de Hats.All

    public static event Action Changed;

    private static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;
        volume = Mathf.Clamp(PlayerPrefs.GetInt("SD_OPT_Volume", 10), 0, 10);
        musicVolume = Mathf.Clamp(PlayerPrefs.GetInt("SD_OPT_Music", 7), 0, 10);
        newSoundtrack = PlayerPrefs.GetInt("SD_OPT_Soundtrack", 1) == 1;
        shake = Mathf.Clamp(PlayerPrefs.GetInt("SD_OPT_Shake", 2), 0, 2);
        flashes = PlayerPrefs.GetInt("SD_OPT_Flashes", 1) == 1;
        hitstop = PlayerPrefs.GetInt("SD_OPT_Hitstop", 1) == 1;
        hudScale = Mathf.Clamp(PlayerPrefs.GetInt("SD_OPT_HudScale", 0), 0, 2);
        holdToMove = PlayerPrefs.GetInt("SD_OPT_Hold", 1) == 1;
        dangerMarkers = PlayerPrefs.GetInt("SD_OPT_Danger", 1) == 1;
        showTimer = PlayerPrefs.GetInt("SD_OPT_Timer", 1) == 1;
        ambientEffects = PlayerPrefs.GetInt("SD_OPT_Ambient", 1) == 1;
        reduceMotion = PlayerPrefs.GetInt("SD_OPT_ReduceMotion", 0) == 1;
        difficulty = Mathf.Clamp(PlayerPrefs.GetInt("SD_OPT_Difficulty", 1), 0, 2);
        hat = PlayerPrefs.GetInt("SD_OPT_Hat", 0);
        if (hat < 0 || hat >= Hats.All.Length || !Hats.IsUnlocked(hat)) hat = 0;
    }

    public static void Apply()
    {
        EnsureLoaded();
        AudioListener.volume = volume / 10f;
    }

    private static void Save()
    {
        PlayerPrefs.SetInt("SD_OPT_Volume", volume);
        PlayerPrefs.SetInt("SD_OPT_Music", musicVolume);
        PlayerPrefs.SetInt("SD_OPT_Soundtrack", newSoundtrack ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Shake", shake);
        PlayerPrefs.SetInt("SD_OPT_Flashes", flashes ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Hitstop", hitstop ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_HudScale", hudScale);
        PlayerPrefs.SetInt("SD_OPT_Hold", holdToMove ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Danger", dangerMarkers ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Timer", showTimer ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Ambient", ambientEffects ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_ReduceMotion", reduceMotion ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Difficulty", difficulty);
        PlayerPrefs.SetInt("SD_OPT_Hat", hat);
        PlayerPrefs.Save();
        Apply();
        Changed?.Invoke();
    }

    public static float MusicVolume { get { EnsureLoaded(); return musicVolume / 10f; } }
    public static bool NewSoundtrack { get { EnsureLoaded(); return newSoundtrack; } }
    public static float ShakeScale { get { EnsureLoaded(); return shake * 0.5f; } }
    public static bool Flashes { get { EnsureLoaded(); return flashes; } }
    public static bool Hitstop { get { EnsureLoaded(); return hitstop; } }
    public static float HudScale { get { EnsureLoaded(); return 1f + hudScale * 0.25f; } }
    public static bool HoldToMove { get { EnsureLoaded(); return holdToMove; } }
    public static bool DangerMarkers { get { EnsureLoaded(); return dangerMarkers; } }
    public static bool ShowTimer { get { EnsureLoaded(); return showTimer; } }
    public static bool AmbientEffects { get { EnsureLoaded(); return ambientEffects; } }
    public static bool ReduceMotion { get { EnsureLoaded(); return reduceMotion; } }
    public static int Difficulty { get { EnsureLoaded(); return difficulty; } }
    public static int Hat { get { EnsureLoaded(); return hat; } }

    public static string DifficultyName(int d) => d == 0 ? "Turista" : d == 2 ? "Ibérico" : "Normal";

    // Corazones con los que se empieza según la dificultad
    public static int StartingHealth(int baseHealth)
    {
        EnsureLoaded();
        return difficulty == 0 ? baseHealth + 2 : difficulty == 2 ? Mathf.Max(1, baseHealth - 1) : baseHealth;
    }

    // Multiplicador de puntuación según la dificultad
    public static float ScoreMultiplier { get { EnsureLoaded(); return difficulty == 0 ? 0.75f : difficulty == 2 ? 1.5f : 1f; } }

    // ---------- Lista de opciones para el menú ----------

    private enum Opt
    {
        Volume, Music, Soundtrack, Difficulty, Hat, HoldToMove, DangerMarkers, Shake, Flashes,
        Hitstop, ReduceMotion, HudScale, ShowTimer, Ambient, Fullscreen
    }

    private static readonly Opt[] Order = (Opt[])Enum.GetValues(typeof(Opt));

    public static int OptionCount => Order.Length;

    private static Opt At(int i) => Order[Mathf.Clamp(i, 0, Order.Length - 1)];

    public static string Label(int i)
    {
        switch (At(i))
        {
            case Opt.Volume: return "Volumen general";
            case Opt.Music: return "Volumen de la música";
            case Opt.Soundtrack: return "Banda sonora";
            case Opt.Difficulty: return "Dificultad";
            case Opt.Hat: return "Sombrero de Torrento";
            case Opt.HoldToMove: return "Mantener tecla para avanzar";
            case Opt.DangerMarkers: return "Avisos de peligro";
            case Opt.Shake: return "Temblor de cámara";
            case Opt.Flashes: return "Destellos y parpadeos";
            case Opt.Hitstop: return "Congelar el golpe (hitstop)";
            case Opt.ReduceMotion: return "Movimiento reducido";
            case Opt.HudScale: return "Tamaño de la interfaz";
            case Opt.ShowTimer: return "Mostrar cronómetro";
            case Opt.Ambient: return "Efectos ambientales";
            default: return "Pantalla completa";
        }
    }

    public static string Hint(int i)
    {
        switch (At(i))
        {
            case Opt.Volume: return "Volumen de todo el juego.";
            case Opt.Music: return "Volumen de la música, aparte de los efectos.";
            case Opt.Soundtrack: return NewSoundtrack
                ? "Nueva: un tema distinto para cada sala (charanga, pasodoble, rumba, sevillanas...)."
                : "Original: la música de la versión entregada.";
            case Opt.Difficulty: return Difficulty == 0 ? "Turista: +2 corazones, pero la puntuación vale x0,75. Se aplica en la siguiente partida."
                         : Difficulty == 2 ? "Ibérico: un corazón menos y puntuación x1,5. Para valientes. Se aplica en la siguiente partida."
                         : "Normal: la experiencia original. Se aplica en la siguiente partida.";
            case Opt.Hat: return Hats.Hint(Hat);
            case Opt.HoldToMove: return "Avanza casilla a casilla mientras mantienes la tecla pulsada.";
            case Opt.DangerMarkers: return "Marca en el suelo los pinchos y avisa antes de que dispare el radar.";
            case Opt.Shake: return "Reduce o quita las sacudidas si te marean.";
            case Opt.Flashes: return "Quita los destellos rojos, los relámpagos y la viñeta pulsante.";
            case Opt.Hitstop: return "Pequeña pausa al golpear. Desactívala si te resulta incómoda.";
            case Opt.ReduceMotion: return "Quita el montaje animado de las salas y el movimiento del fondo. Útil si te mareas.";
            case Opt.HudScale: return "Agranda los textos y los iconos del HUD.";
            case Opt.ShowTimer: return "Muestra el tiempo de la partida arriba del todo.";
            case Opt.Ambient: return "Confeti, polvo, hojas y luciérnagas del fondo.";
            default: return "Alterna entre ventana y pantalla completa.";
        }
    }

    public static string Value(int i)
    {
        EnsureLoaded();
        switch (At(i))
        {
            case Opt.Volume: return volume * 10 + " %";
            case Opt.Music: return musicVolume * 10 + " %";
            case Opt.Soundtrack: return newSoundtrack ? "Nueva" : "Original";
            case Opt.Difficulty: return DifficultyName(difficulty);
            case Opt.Hat: return Hats.All[hat].name;
            case Opt.HoldToMove: return YesNo(holdToMove);
            case Opt.DangerMarkers: return YesNo(dangerMarkers);
            case Opt.Shake: return shake == 0 ? "Desactivado" : shake == 1 ? "Suave" : "Normal";
            case Opt.Flashes: return YesNo(flashes);
            case Opt.Hitstop: return YesNo(hitstop);
            case Opt.ReduceMotion: return YesNo(reduceMotion);
            case Opt.HudScale: return (100 + hudScale * 25) + " %";
            case Opt.ShowTimer: return YesNo(showTimer);
            case Opt.Ambient: return YesNo(ambientEffects);
            default: return YesNo(Screen.fullScreen);
        }
    }

    private static string YesNo(bool b) => b ? "Sí" : "No";

    public static void Change(int i, int delta)
    {
        EnsureLoaded();
        switch (At(i))
        {
            case Opt.Volume: volume = Mathf.Clamp(volume + delta, 0, 10); break;
            case Opt.Music: musicVolume = Mathf.Clamp(musicVolume + delta, 0, 10); break;
            case Opt.Soundtrack: newSoundtrack = !newSoundtrack; break;
            case Opt.Difficulty: difficulty = (difficulty + delta + 3) % 3; break;
            case Opt.Hat:
                hat = Hats.Next(hat, delta);
                if (hat != 0) Achievements.Unlock("modelito");
                break;
            case Opt.HoldToMove: holdToMove = !holdToMove; break;
            case Opt.DangerMarkers: dangerMarkers = !dangerMarkers; break;
            case Opt.Shake: shake = (shake + delta + 3) % 3; break;
            case Opt.Flashes: flashes = !flashes; break;
            case Opt.Hitstop: hitstop = !hitstop; break;
            case Opt.ReduceMotion: reduceMotion = !reduceMotion; break;
            case Opt.HudScale: hudScale = (hudScale + delta + 3) % 3; break;
            case Opt.ShowTimer: showTimer = !showTimer; break;
            case Opt.Ambient: ambientEffects = !ambientEffects; break;
            default: Screen.fullScreen = !Screen.fullScreen; break;
        }
        Save();
    }
}
