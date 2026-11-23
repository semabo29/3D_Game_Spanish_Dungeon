using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // true cuando se ha mostrado la pantalla de victoria o de derrota
    public static bool GameEnded { get; private set; }

    [Header("Estadísticas")]
    public int maxHealth = 3;
    [Tooltip("Corazones máximos que se pueden acumular con la lotería")]
    public int healthCap = 5;
    private int currentHealth;
    private int coins = 0;
    public int currentRoom = 1;

    [Header("Lotería")]
    [Tooltip("Cada cuántas monedas recogidas te toca el Gordo (+1 corazón)")]
    [SerializeField] private int coinsPerLottery = 15;
    private int coinsTowardsLottery = 0;

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI roomText;

    [Header("Pantallas UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Invulnerabilidad")]
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Sonidos de Estado")]
    [SerializeField] private AudioClip sonidoDaño;
    [SerializeField] private AudioClip sonidoGameOver;
    [SerializeField] private float volumenSonidos = 0.8f;

    private bool isDead = false;
    private bool isInvincible = false;
    private Coroutine invincibilityRoutine;

    private List<Renderer> activeRenderers = new List<Renderer>();

    public int CurrentHealth => currentHealth;
    public int Coins => coins;
    public bool IsDead => isDead;
    // Tras un golpe, los enemigos te atraviesan sin hacerte nada ni desaparecer
    public bool IsInvincible => isInvincible;

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameEnded = false;
        // La dificultad elegida en Opciones cambia los corazones iniciales
        maxHealth = GameSettings.StartingHealth(maxHealth);
        healthCap = Mathf.Max(healthCap, maxHealth);
        currentHealth = maxHealth;

        RunStats.Reset();
        Estampitas.Reset();
        GameSettings.Apply();

        // El HUD nuevo sustituye a los textos antiguos de la escena
        if (healthText != null) healthText.gameObject.SetActive(false);
        if (coinText != null) coinText.gameObject.SetActive(false);
        if (roomText != null) roomText.gameObject.SetActive(false);

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in allRenderers)
        {
            if (ren != null && ren.enabled)
            {
                activeRenderers.Add(ren);
            }
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        GameHUD.Create().ShowRoomBanner(currentRoom);

        UpdateUI();
    }

    public void WinGame()
    {
        if (isDead || GameEnded) return; // Si ya morimos en el último segundo, no ganamos
        GameEnded = true;

        // Quitamos el control al jugador
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        AudioListener.pause = true; // Silenciamos el juego

        Achievements.OnVictory();
        MusicDirector.PlayVictory();
        FinishRun(true);

        if (victoryPanel != null) victoryPanel.SetActive(true);

        // Congelamos el tiempo
        Time.timeScale = 0f;
    }

    public void TakeDamage()
    {
        TakeDamage(null);
    }

    // cause: texto para la pantalla de Game Over ("Causa de la muerte")
    public void TakeDamage(string cause)
    {
        if (isDead || isInvincible || GameEnded) return;

        // El abanico flamenco para un golpe
        PlayerPowerUps powerUps = GetComponent<PlayerPowerUps>();
        if (powerUps != null && powerUps.ConsumeShield())
        {
            invincibilityRoutine = StartCoroutine(InvincibilityRoutine());
            return;
        }

        if (!string.IsNullOrEmpty(cause)) RunStats.DeathCause = cause;
        currentHealth--;
        RunStats.DamageTaken++;
        UpdateUI();

        GameFeel.Shake(0.25f, 0.25f);
        GameFeel.FloatingText(transform.position, "-1", new Color(1f, 0.25f, 0.25f), 5f);
        if (GameHUD.Instance != null) GameHUD.Instance.FlashDamage();

        if (currentHealth <= 0 && powerUps != null && powerUps.ConsumeInsurance())
        {
            // El seguro del chiringuito te deja con un corazón
            currentHealth = 1;
            UpdateUI();
            GameFeel.FloatingText(transform.position + Vector3.up * 0.6f, "¡EL SEGURO TE CUBRE!", new Color(0.5f, 0.9f, 1f), 5f);
            if (GameHUD.Instance != null) GameHUD.Instance.ShowToast("Parte al seguro: sigues vivo de milagro", new Color(0.5f, 0.9f, 1f), 1.8f);
            Sfx.Play(SfxType.ShieldBreak, 1f, 0.8f);
            GameFeel.Hitstop(0.12f);
            invincibilityRoutine = StartCoroutine(InvincibilityRoutine());
        }
        else if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine(0.5f, false));
        }
        else
        {
            if (sonidoDaño != null)
            {
                AudioSource.PlayClipAtPoint(sonidoDaño, Camera.main.transform.position, volumenSonidos);
            }
            GameFeel.Hitstop(0.08f);
            invincibilityRoutine = StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;

        float invTime = invincibilityTime * Estampitas.InvincibilityMultiplier;
        while (timer < invTime)
        {
            foreach (Renderer ren in activeRenderers)
            {
                if (ren != null) ren.enabled = !ren.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        SetRenderersVisible(true);

        isInvincible = false;
        invincibilityRoutine = null;
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (Renderer ren in activeRenderers)
        {
            if (ren != null) ren.enabled = visible;
        }
    }

    // Una gaviota se lleva una moneda (no cuenta para la lotería)
    public bool StealCoin()
    {
        if (isDead || GameEnded || coins <= 0) return false;
        if (Estampitas.ProtectsCoins)
        {
            GameFeel.FloatingText(transform.position + Vector3.up * 0.8f, "¡CARTERA A SALVO!", new Color(0.85f, 0.65f, 0.4f), 4f);
            return false;
        }
        coins--;
        UpdateUI();
        return true;
    }

    // Pagar en el chiringuito
    public bool SpendCoins(int amount)
    {
        if (isDead || GameEnded || amount < 0 || coins < amount) return false;
        coins -= amount;
        UpdateUI();
        return true;
    }

    // Premio gordo comprado en un décimo
    public void WinLotteryPrize()
    {
        if (isDead || GameEnded) return;
        WinLottery();
        UpdateUI();
    }

    private bool doublingCoin;

    public void AddCoin()
    {
        if (isDead || GameEnded) return;
        // Happy hour: cada moneda cuenta doble
        if (RoomEvents.DoubleCoins && !doublingCoin)
        {
            doublingCoin = true;
            AddCoin();
            doublingCoin = false;
            GameFeel.FloatingText(transform.position + Vector3.up * 1.2f, "x2", new Color(1f, 0.9f, 0.3f), 3.5f);
        }
        coins++;
        RunStats.CoinsCollected++;
        Achievements.OnCoinCollected();

        coinsTowardsLottery++;
        if (coinsTowardsLottery >= Estampitas.LotteryCost(coinsPerLottery))
        {
            coinsTowardsLottery = 0;
            WinLottery();
        }

        UpdateUI();
    }

    // "¡Te ha tocado el Gordo!": +1 corazón (o lo cura si estás tocado)
    private void WinLottery()
    {
        RunStats.LotteryWins++;
        Achievements.OnLottery();
        if (currentHealth < healthCap) currentHealth++;
        Sfx.Play(SfxType.PowerUp, 1f, 1.2f);

        GameFeel.FloatingText(transform.position, "+1 CORAZÓN", new Color(0.3f, 1f, 0.4f), 5f);
        GameFeel.VoxelBurst(transform.position, new Color(1f, 0.85f, 0.15f), 20, 3.5f);
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.ShowToast("¡TE HA TOCADO EL GORDO!", new Color(1f, 0.85f, 0.15f), 1.6f);
        }
    }

    // Cura corazones (hasta el máximo de la lotería). Devuelve false si ya estaba a tope.
    public bool Heal(int amount)
    {
        if (isDead || GameEnded || currentHealth >= healthCap) return false;
        currentHealth = Mathf.Min(healthCap, currentHealth + amount);
        UpdateUI();
        return true;
    }

    public void LoseCoins(int amount)
    {
        // Durante la invulnerabilidad tampoco te quitan monedas
        if (isDead || GameEnded || isInvincible) return;
        if (Estampitas.ProtectsCoins)
        {
            if (coins > 0) GameFeel.FloatingText(transform.position + Vector3.right * 0.5f, "¡CARTERA A SALVO!", new Color(0.85f, 0.65f, 0.4f), 3.5f);
            return;
        }
        int lost = Mathf.Min(amount, coins);
        coins -= lost;
        RunStats.CoinsLost += lost;

        if (lost > 0)
        {
            GameFeel.FloatingText(transform.position + Vector3.right * 0.5f, "-" + lost + " monedas", new Color(1f, 0.8f, 0.2f), 3.5f);
        }
        UpdateUI();
    }

    // Estampita "Corazón de oro": un corazón más de máximo y cura uno
    public void AddMaxHeart()
    {
        if (isDead || GameEnded) return;
        maxHealth++;
        healthCap++;
        currentHealth = Mathf.Min(healthCap, currentHealth + 1);
        UpdateUI();
    }

    public void RefreshUI() => UpdateUI();

    public void AddRoom()
    {
        if (isDead) return;
        currentRoom++;
        UpdateUI();
        if (GameHUD.Instance != null && !GameEnded) GameHUD.Instance.ShowRoomBanner(currentRoom);
    }

    private void UpdateUI()
    {
        GameHUD hud = GameHUD.Instance;
        if (hud != null)
        {
            hud.SetHealth(currentHealth, maxHealth);
            hud.SetCoins(coins);
            hud.SetRoom(currentRoom);
        }

        if (healthText != null) healthText.text = "Corazones: " + currentHealth;
        if (coinText != null) coinText.text = "Monedas: " + coins;
        if (GameHUD.Instance != null) GameHUD.Instance.SetLotteryProgress(coinsTowardsLottery, Estampitas.LotteryCost(coinsPerLottery));
        if (roomText != null) roomText.text = "Sala: " + currentRoom;
    }

    public void FallToDeath()
    {
        if (isDead) return;
        currentHealth = 0;
        RunStats.DeathCause = FallCauses[Random.Range(0, FallCauses.Length)];
        UpdateUI();

        if (GameHUD.Instance != null) GameHUD.Instance.ShowToast("¡AL VACÍO!", new Color(1f, 0.3f, 0.3f), 1.2f);

        StartCoroutine(DieRoutine(1.5f, true));
    }

    private static readonly string[] FallCauses =
    {
        "se lo tragó la tierra (literalmente)",
        "comprobó la ley de la gravedad",
        "buscaba el metro de Madrid",
        "el suelo era de obra pública"
    };

    private IEnumerator DieRoutine(float delayBeforeGameOver, bool fell)
    {
        isDead = true;
        Achievements.OnDeath(fell, currentRoom);

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        if (!fell)
        {
            // Torrento estalla en vóxeles
            if (invincibilityRoutine != null) StopCoroutine(invincibilityRoutine);
            SetRenderersVisible(false);
            GameFeel.VoxelBurst(transform.position, new Color(0.35f, 0.3f, 0.2f), 24, 5f);
            GameFeel.VoxelBurst(transform.position, new Color(0.7f, 0.6f, 0.25f), 10, 4f);
            GameFeel.Shake(0.35f, 0.4f);
        }

        yield return new WaitForSeconds(delayBeforeGameOver);

        GameEnded = true;
        AudioListener.pause = true;

        if (sonidoGameOver != null)
        {
            GameObject audioObj = new GameObject("GameOverAudio");
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = sonidoGameOver;
            source.volume = volumenSonidos;
            source.ignoreListenerPause = true;
            source.Play();
            Destroy(audioObj, sonidoGameOver.length + 0.1f);
        }

        FinishRun(false);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void FinishRun(bool victory)
    {
        // Al ganar, currentRoom ya apunta a la "sala siguiente" a la última
        int room = victory ? Mathf.Max(1, currentRoom - 1) : currentRoom;
        int score = RunStats.CalculateScore(coins, room, victory);
        bool newRecord = RunStats.SaveRun(score, room, victory);
        if (GameHUD.Instance != null) GameHUD.Instance.ShowEndStats(victory, score, coins, room, newRecord);
    }

    public void BotonReiniciar()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("SampleScene");
    }

    public void BotonMenuPrincipal()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MenuPrincipal");
    }
}
