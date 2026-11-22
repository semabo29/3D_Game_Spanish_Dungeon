using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Efectos activos de los power-ups sobre Torrento
public class PlayerPowerUps : MonoBehaviour
{
    public const float ChurrosDuration = 7f;
    public const float SiestaDuration = 5f;

    private float speedTimer;
    private bool shield;
    private float siestaTimer;
    private Transform shieldVisual;
    private readonly List<Behaviour> sleeping = new List<Behaviour>();
    private float zzzTimer;
    private float trailTimer;
    private PlayerStats stats;

    public float SpeedMultiplier => speedTimer > 0f ? 1.6f : 1f;
    public bool HasShield => shield;
    public bool SiestaActive => siestaTimer > 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public void Apply(PowerUpType type)
    {
        RunStats.PowerUps++;
        Sfx.Play(SfxType.PowerUp, 0.9f);
        GameFeel.Shake(0.08f, 0.15f);
        Color c = PowerUpPickup.TintOf(type);
        string toast;

        switch (type)
        {
            case PowerUpType.Bocadillo:
                Achievements.OnBocadillo();
                if (stats != null && stats.Heal(1))
                {
                    toast = "¡Bocata de calamares! +1 corazón";
                    GameFeel.FloatingText(transform.position, "+1 CORAZÓN", new Color(0.3f, 1f, 0.4f), 5f);
                }
                else
                {
                    toast = "¡Qué lleno estoy! (+3 monedas)";
                    if (stats != null) for (int i = 0; i < 3; i++) stats.AddCoin();
                }
                break;

            case PowerUpType.Churros:
                speedTimer = ChurrosDuration;
                toast = "¡Subidón de azúcar! Más velocidad";
                GameFeel.FloatingText(transform.position, "¡AZÚCAR!", c, 5f);
                break;

            case PowerUpType.Abanico:
                shield = true;
                toast = "¡Abanico flamenco! Te protege de un golpe";
                GameFeel.FloatingText(transform.position, "¡OLÉ!", c, 5f);
                CreateShieldVisual();
                break;

            case PowerUpType.Siesta:
                Achievements.OnSiesta();
                toast = "¡Hora de la siesta! Todos a dormir";
                StartSiesta();
                break;

            default:
                Achievements.OnJamon();
                toast = "¡PATA NEGRA! +5 monedas";
                GameFeel.FloatingText(transform.position, "¡PATA NEGRA!", c, 5f);
                if (stats != null) for (int i = 0; i < 5; i++) stats.AddCoin();
                break;
        }

        if (GameHUD.Instance != null) GameHUD.Instance.ShowToast(toast, c, 1.5f);
    }

    // Devuelve true si el abanico ha parado el golpe
    public bool ConsumeShield()
    {
        if (!shield) return false;
        shield = false;
        Sfx.Play(SfxType.ShieldBreak, 0.9f);
        GameFeel.FloatingText(transform.position, "¡PARADO!", new Color(1f, 0.4f, 0.45f), 5f);
        GameFeel.VoxelBurst(transform.position + Vector3.up * 0.6f, new Color(0.85f, 0.12f, 0.18f), 14, 4f);
        GameFeel.Shake(0.15f, 0.2f);
        if (shieldVisual != null) Destroy(shieldVisual.gameObject);
        return true;
    }

    private void CreateShieldVisual()
    {
        if (shieldVisual != null) return;
        GameObject pivot = new GameObject("AbanicoOrbita");
        pivot.transform.SetParent(transform, false);
        pivot.transform.localPosition = Vector3.up * 0.2f;
        GameObject fan = VoxelLib.Spawn("Abanico", VoxelModels.Abanico(), pivot.transform, transform.position + Vector3.up * 0.4f, Quaternion.identity, false);
        fan.transform.localPosition = new Vector3(0.6f, 0.3f, 0f);
        fan.transform.localScale = Vector3.one * 0.7f;
        shieldVisual = pivot.transform;
    }

    private void StartSiesta()
    {
        WakeUp();
        siestaTimer = SiestaDuration;
        Sfx.Play(SfxType.Snore, 0.8f);

        Freeze(FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None));
        Freeze(FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None));
        Freeze(FindObjectsByType<Guiri>(FindObjectsSortMode.None));
        Freeze(FindObjectsByType<ArrowTrap>(FindObjectsSortMode.None));
        Freeze(FindObjectsByType<SpikeTrap>(FindObjectsSortMode.None));

        if (FindObjectsByType<Pedro>(FindObjectsSortMode.None).Length > 0 && GameHUD.Instance != null)
        {
            GameHUD.Instance.ShowToast("Pedro no duerme la siesta: ¡está en el escaño!", new Color(1f, 0.4f, 0.35f), 1.8f);
        }
        if (GameHUD.Instance != null) GameHUD.Instance.SetSiestaOverlay(true);
        zzzTimer = 0f;
    }

    private void Freeze<T>(T[] list) where T : Behaviour
    {
        foreach (T b in list)
        {
            if (b == null || !b.enabled) continue;
            b.enabled = false;
            sleeping.Add(b);
        }
    }

    private void WakeUp()
    {
        foreach (Behaviour b in sleeping)
        {
            if (b != null) b.enabled = true;
        }
        sleeping.Clear();
        if (GameHUD.Instance != null) GameHUD.Instance.SetSiestaOverlay(false);
    }

    void Update()
    {
        if (speedTimer > 0f)
        {
            speedTimer -= Time.deltaTime;
            trailTimer -= Time.deltaTime;
            if (trailTimer <= 0f)
            {
                trailTimer = 0.08f;
                GameFeel.Dust(transform.position - Vector3.up * 0.3f, new Color(1f, 0.85f, 0.5f), 1);
            }
            if (speedTimer <= 0f && GameHUD.Instance != null)
                GameHUD.Instance.ShowToast("Se acabó el azúcar", new Color(1f, 0.8f, 0.3f), 0.9f);
        }

        if (shieldVisual != null)
        {
            shieldVisual.Rotate(0f, 160f * Time.deltaTime, 0f, Space.Self);
        }

        if (siestaTimer > 0f)
        {
            siestaTimer -= Time.deltaTime;
            zzzTimer -= Time.deltaTime;
            if (zzzTimer <= 0f)
            {
                zzzTimer = 0.9f;
                foreach (Behaviour b in sleeping)
                {
                    if (b != null && !(b is SpikeTrap) && !(b is ArrowTrap))
                        GameFeel.FloatingText(b.transform.position, Random.value < 0.5f ? "Zzz" : "zZz", new Color(0.6f, 0.8f, 1f), 3.5f);
                }
            }
            if (siestaTimer <= 0f)
            {
                WakeUp();
                if (GameHUD.Instance != null) GameHUD.Instance.ShowToast("¡Se acabó la siesta!", new Color(0.6f, 0.8f, 1f), 0.9f);
            }
        }

        if (GameHUD.Instance != null) GameHUD.Instance.SetPowerUpStatus(StatusText());
    }

    private string StatusText()
    {
        string s = "";
        if (speedTimer > 0f) s += "<color=#FFD04A>CHURROS " + Mathf.CeilToInt(speedTimer) + "s</color>   ";
        if (shield) s += "<color=#FF5A64>ABANICO</color>   ";
        if (siestaTimer > 0f) s += "<color=#8CC4FF>SIESTA " + Mathf.CeilToInt(siestaTimer) + "s</color>";
        return s;
    }

    void OnDisable()
    {
        WakeUp();
    }
}
