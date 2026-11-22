using UnityEngine;

// Estadísticas de la partida actual y récords guardados en PlayerPrefs.
public static class RunStats
{
    public static int Kills;
    public static int CoinsCollected;
    public static int CoinsLost;
    public static int DamageTaken;
    public static int BestCombo;
    public static int LotteryWins;
    public static int KillsToro;
    public static int TimesDrunk;
    public static int Multas;
    public static int Smashes;
    public static int PowerUps;
    public static int RoomsCleared;
    public static string DeathCause = "";
    public static float StartTime;
    public static float EndTime = -1f;

    private const string KeyHighScore = "SD_HighScore";
    private const string KeyBestTime = "SD_BestTime";
    private const string KeyBestRoom = "SD_BestRoom";
    private const string KeyWins = "SD_Wins";

    public static void Reset()
    {
        Kills = 0;
        CoinsCollected = 0;
        CoinsLost = 0;
        DamageTaken = 0;
        BestCombo = 0;
        LotteryWins = 0;
        KillsToro = 0;
        TimesDrunk = 0;
        Multas = 0;
        Smashes = 0;
        PowerUps = 0;
        RoomsCleared = 0;
        DeathCause = "";
        StartTime = Time.time;
        EndTime = -1f;
    }

    public static float ElapsedSeconds => (EndTime >= 0f ? EndTime : Time.time) - StartTime;

    public static int CalculateScore(int coins, int room, bool victory)
    {
        int score = coins * 10 + (room - 1) * 150 + Kills * 25 + BestCombo * 20 - DamageTaken * 30;
        if (victory)
        {
            score += 1500;
            // Bonus por rapidez: hasta 1000 puntos si terminas en menos de 10 minutos
            score += Mathf.RoundToInt(Mathf.Clamp01(1f - ElapsedSeconds / 600f) * 1000f);
        }
        score = Mathf.RoundToInt(score * GameSettings.ScoreMultiplier);
        return Mathf.Max(0, score);
    }

    public static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return m.ToString("00") + ":" + s.ToString("00");
    }

    public static int HighScore => PlayerPrefs.GetInt(KeyHighScore, 0);
    public static float BestTime => PlayerPrefs.GetFloat(KeyBestTime, -1f);
    public static int BestRoom => PlayerPrefs.GetInt(KeyBestRoom, 0);
    public static int Wins => PlayerPrefs.GetInt(KeyWins, 0);

    // Guarda la partida y devuelve true si se ha batido el récord de puntuación
    public static bool SaveRun(int score, int room, bool victory)
    {
        EndTime = Time.time;
        bool newRecord = score > HighScore;
        if (newRecord) PlayerPrefs.SetInt(KeyHighScore, score);
        if (room > BestRoom) PlayerPrefs.SetInt(KeyBestRoom, room);
        if (victory)
        {
            PlayerPrefs.SetInt(KeyWins, Wins + 1);
            if (BestTime < 0f || ElapsedSeconds < BestTime) PlayerPrefs.SetFloat(KeyBestTime, ElapsedSeconds);
        }
        PlayerPrefs.Save();
        return newRecord;
    }
}
