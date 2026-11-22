using UnityEngine;
using System;

// Opciones del jugador (accesibilidad, audio, interfaz), guardadas en PlayerPrefs
public static class GameSettings
{
    private static bool loaded;

    private static int volume = 10;          // 0..10
    private static int shake = 2;            // 0 = no, 1 = suave, 2 = normal
    private static bool flashes = true;
    private static bool hitstop = true;
    private static int hudScale = 0;         // 0 = 100 %, 1 = 125 %, 2 = 150 %
    private static bool holdToMove = true;
    private static bool dangerMarkers = true;
    private static bool showTimer = true;
    private static bool ambientEffects = true;

    public static event Action Changed;

    private static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;
        volume = PlayerPrefs.GetInt("SD_OPT_Volume", 10);
        shake = PlayerPrefs.GetInt("SD_OPT_Shake", 2);
        flashes = PlayerPrefs.GetInt("SD_OPT_Flashes", 1) == 1;
        hitstop = PlayerPrefs.GetInt("SD_OPT_Hitstop", 1) == 1;
        hudScale = PlayerPrefs.GetInt("SD_OPT_HudScale", 0);
        holdToMove = PlayerPrefs.GetInt("SD_OPT_Hold", 1) == 1;
        dangerMarkers = PlayerPrefs.GetInt("SD_OPT_Danger", 1) == 1;
        showTimer = PlayerPrefs.GetInt("SD_OPT_Timer", 1) == 1;
        ambientEffects = PlayerPrefs.GetInt("SD_OPT_Ambient", 1) == 1;
    }

    public static void Apply()
    {
        EnsureLoaded();
        AudioListener.volume = volume / 10f;
    }

    private static void Save()
    {
        PlayerPrefs.SetInt("SD_OPT_Volume", volume);
        PlayerPrefs.SetInt("SD_OPT_Shake", shake);
        PlayerPrefs.SetInt("SD_OPT_Flashes", flashes ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Hitstop", hitstop ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_HudScale", hudScale);
        PlayerPrefs.SetInt("SD_OPT_Hold", holdToMove ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Danger", dangerMarkers ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Timer", showTimer ? 1 : 0);
        PlayerPrefs.SetInt("SD_OPT_Ambient", ambientEffects ? 1 : 0);
        PlayerPrefs.Save();
        Apply();
        Changed?.Invoke();
    }

    public static float ShakeScale { get { EnsureLoaded(); return shake * 0.5f; } }
    public static bool Flashes { get { EnsureLoaded(); return flashes; } }
    public static bool Hitstop { get { EnsureLoaded(); return hitstop; } }
    public static float HudScale { get { EnsureLoaded(); return 1f + hudScale * 0.25f; } }
    public static bool HoldToMove { get { EnsureLoaded(); return holdToMove; } }
    public static bool DangerMarkers { get { EnsureLoaded(); return dangerMarkers; } }
    public static bool ShowTimer { get { EnsureLoaded(); return showTimer; } }
    public static bool AmbientEffects { get { EnsureLoaded(); return ambientEffects; } }

    // ---------- Lista de opciones para el menú ----------

    public const int OptionCount = 10;

    public static string Label(int i)
    {
        switch (i)
        {
            case 0: return "Volumen general";
            case 1: return "Temblor de cámara";
            case 2: return "Destellos y parpadeos";
            case 3: return "Congelar el golpe (hitstop)";
            case 4: return "Tamaño de la interfaz";
            case 5: return "Mantener tecla para avanzar";
            case 6: return "Avisos de peligro";
            case 7: return "Mostrar cronómetro";
            case 8: return "Efectos ambientales";
            default: return "Pantalla completa";
        }
    }

    public static string Hint(int i)
    {
        switch (i)
        {
            case 0: return "Volumen de todo el juego.";
            case 1: return "Reduce o quita las sacudidas si te marean.";
            case 2: return "Quita los destellos rojos, los relámpagos y la viñeta pulsante.";
            case 3: return "Pequeña pausa al golpear. Desactívala si te resulta incómoda.";
            case 4: return "Agranda los textos y los iconos del HUD.";
            case 5: return "Avanza casilla a casilla mientras mantienes la tecla pulsada.";
            case 6: return "Marca en el suelo los pinchos y avisa antes de que dispare el radar.";
            case 7: return "Muestra el tiempo de la partida arriba del todo.";
            case 8: return "Confeti, polvo, hojas y luciérnagas del fondo.";
            default: return "Alterna entre ventana y pantalla completa.";
        }
    }

    public static string Value(int i)
    {
        EnsureLoaded();
        switch (i)
        {
            case 0: return volume * 10 + " %";
            case 1: return shake == 0 ? "Desactivado" : shake == 1 ? "Suave" : "Normal";
            case 2: return YesNo(flashes);
            case 3: return YesNo(hitstop);
            case 4: return (100 + hudScale * 25) + " %";
            case 5: return YesNo(holdToMove);
            case 6: return YesNo(dangerMarkers);
            case 7: return YesNo(showTimer);
            case 8: return YesNo(ambientEffects);
            default: return YesNo(Screen.fullScreen);
        }
    }

    private static string YesNo(bool b) => b ? "Sí" : "No";

    public static void Change(int i, int delta)
    {
        EnsureLoaded();
        switch (i)
        {
            case 0: volume = Mathf.Clamp(volume + delta, 0, 10); break;
            case 1: shake = (shake + delta + 3) % 3; break;
            case 2: flashes = !flashes; break;
            case 3: hitstop = !hitstop; break;
            case 4: hudScale = (hudScale + delta + 3) % 3; break;
            case 5: holdToMove = !holdToMove; break;
            case 6: dangerMarkers = !dangerMarkers; break;
            case 7: showTimer = !showTimer; break;
            case 8: ambientEffects = !ambientEffects; break;
            default: Screen.fullScreen = !Screen.fullScreen; break;
        }
        Save();
    }
}
