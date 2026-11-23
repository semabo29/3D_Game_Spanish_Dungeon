using UnityEngine;

// Sala perfecta: si eliminas a todos los enemigos sin recibir ni un golpe, premio
public class RoomClearBonus : MonoBehaviour
{
    private const int BonusCoins = 3;
    private int initial = -1;
    private float timer;
    private bool done;
    private int damageAtStart;

    void Start()
    {
        damageAtStart = RunStats.DamageTaken;
    }

    private static readonly string[] Shouts =
    {
        "¡SALA PERFECTA!", "¡NI UN RASGUÑO!", "¡ASÍ SE HACE, MAJO!", "¡IMPECABLE!", "¡QUÉ ARTE!",
    };

    public static void Create(Transform mapRoot)
    {
        GameObject go = new GameObject("RoomClearBonus");
        go.transform.SetParent(mapRoot, false);
        go.AddComponent<RoomClearBonus>();
    }

    private static int CountEnemies()
    {
        MapGenerator map = MapGenerator.Instance;
        if (map == null) return 0;
        int n = 0;
        foreach (EnemyPatrol e in map.GetComponentsInChildren<EnemyPatrol>()) if (e.gameObject.activeInHierarchy) n++;
        foreach (Enemigo_basico e in map.GetComponentsInChildren<Enemigo_basico>()) if (e.gameObject.activeInHierarchy) n++;
        foreach (Guiri e in map.GetComponentsInChildren<Guiri>()) if (e.gameObject.activeInHierarchy) n++;
        return n;
    }

    void Update()
    {
        if (done || MapGenerator.InputLocked) return;
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = 0.3f;

        int n = CountEnemies();
        if (initial < 0)
        {
            initial = n;
            if (initial == 0) done = true; // sala sin enemigos: no hay premio
            return;
        }
        if (n > 0) return;

        done = true;
        PlayerController player = PlayerController.Instance;
        if (player == null) return;
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null || stats.IsDead || PlayerStats.GameEnded) return;
        if (RunStats.DamageTaken != damageAtStart) return; // te han dado: sin premio

        RunStats.RoomsCleared++;
        Achievements.OnRoomCleared();
        for (int i = 0; i < BonusCoins; i++) stats.AddCoin();
        Sfx.Play(SfxType.Combo, 0.9f, 0.8f);
        GameFeel.VoxelBurst(player.transform.position + Vector3.up * 0.8f, new Color(1f, 0.82f, 0.2f), 18, 5f);
        GameFeel.FloatingText(player.transform.position + Vector3.up * 1.4f, "+" + BonusCoins, new Color(1f, 0.85f, 0.2f), 5f);
        if (GameHUD.Instance != null)
            GameHUD.Instance.ShowToast(Shouts[Random.Range(0, Shouts.Length)] + "  +" + BonusCoins + " monedas", new Color(1f, 0.85f, 0.2f), 1.8f);
    }
}
