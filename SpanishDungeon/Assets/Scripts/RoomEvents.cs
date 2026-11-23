using UnityEngine;
using System.Collections;

// "ÚLTIMA HORA": algunas salas traen un suceso aleatorio que cambia un poco la partida
public static class RoomEvents
{
    public enum Kind { None, HappyHour, RadarStrike, CoinRain }

    public static Kind Current { get; private set; } = Kind.None;

    public static bool DoubleCoins => Current == Kind.HappyHour;
    public static bool RadarsOnStrike => Current == Kind.RadarStrike;

    public static void Clear() => Current = Kind.None;

    // Se llama al decorar la sala. 'freeCell' da casillas libres para colocar cosas.
    public static void Roll(RoomLayout layout, Transform parent, GameObject coinPrefab, System.Func<Vector3?> freeCell)
    {
        Current = Kind.None;
        if (layout.room < 2 || layout.room > 9) return;
        if (Random.value > 0.35f) return;

        bool hasRadars = parent.GetComponentsInChildren<ArrowTrap>(true).Length > 0;
        float r = Random.value;
        if (hasRadars && r < 0.33f) Current = Kind.RadarStrike;
        else if (r < 0.66f) Current = Kind.HappyHour;
        else Current = Kind.CoinRain;

        string text = "";
        switch (Current)
        {
            case Kind.HappyHour:
                text = "ÚLTIMA HORA: ¡Happy hour! Las monedas valen doble en esta sala";
                break;
            case Kind.RadarStrike:
                text = "ÚLTIMA HORA: Los radares están de huelga. Hoy no multan";
                break;
            case Kind.CoinRain:
                text = "ÚLTIMA HORA: ¡Se le ha roto la hucha a un concejal! Llueven monedas";
                if (coinPrefab != null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3? p = freeCell();
                        if (p == null) break;
                        Object.Instantiate(coinPrefab, p.Value, Quaternion.identity, parent);
                    }
                }
                break;
        }

        GameObject go = new GameObject("UltimaHora");
        go.transform.SetParent(parent, false);
        go.AddComponent<RoomEventAnnouncer>().text = text;
    }
}

// Anuncia el suceso cuando ya se ha ido el cartel de la sala
public class RoomEventAnnouncer : MonoBehaviour
{
    public string text;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(2.2f);
        if (GameHUD.Instance != null && !PlayerStats.GameEnded)
        {
            GameHUD.Instance.ShowToast(text, new Color(1f, 0.95f, 0.55f), 2.6f);
            Sfx.Play(SfxType.Select, 0.8f, 0.9f);
        }
    }
}
