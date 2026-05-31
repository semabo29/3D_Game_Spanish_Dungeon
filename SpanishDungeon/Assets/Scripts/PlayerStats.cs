using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // IMPORTANTE: Necesario para usar listas (List<Renderer>)

public class PlayerStats : MonoBehaviour
{
    [Header("Estadísticas")]
    public int maxHealth = 3;
    private int currentHealth;
    private int coins = 0;
    public int currentRoom = 1;

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI roomText;

    [Header("Pantallas UI")]
    [SerializeField] private GameObject gameOverPanel; // <--- Arrastra aquí el panel rojo

    [Header("Invulnerabilidad")]
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Sonidos de Estado")]
    [SerializeField] private AudioClip sonidoDaño;
    [SerializeField] private AudioClip sonidoGameOver;
    [SerializeField] private float volumenSonidos = 0.8f;

    private bool isDead = false;
    private bool isInvincible = false;

    // CORRECCIÓN CLAVE: Usamos una lista para guardar SOLO las mallas que deben verse
    private List<Renderer> activeRenderers = new List<Renderer>();

    void Start()
    {
        // 1. Aseguramos que el tiempo fluya al iniciar (por si venimos de un Game Over)
        Time.timeScale = 1f;

        // 2. Nos aseguramos de que el sonido global esté activo al empezar
        AudioListener.pause = false;

        currentHealth = maxHealth;

        // Buscamos todas las mallas, pero FILTRAMOS las que tienes desactivadas adrede
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in allRenderers)
        {
            if (ren != null && ren.enabled)
            {
                activeRenderers.Add(ren); // Solo se añaden las piezas visibles de tu .vox
            }
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // Ocultamos el panel al inicio

        UpdateUI();
    }

    public void TakeDamage()
    {
        if (isDead || isInvincible) return;

        currentHealth--;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (sonidoDaño != null)
            {
                AudioSource.PlayClipAtPoint(sonidoDaño, Camera.main.transform.position, volumenSonidos);
            }
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < invincibilityTime)
        {
            // Solo hacemos parpadear la lista filtrada pura
            foreach (Renderer ren in activeRenderers)
            {
                if (ren != null) ren.enabled = !ren.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Al terminar, nos aseguramos de que esas mismas piezas vuelvan a verse
        foreach (Renderer ren in activeRenderers)
        {
            if (ren != null) ren.enabled = true;
        }

        isInvincible = false;
    }

    public void AddCoin()
    {
        if (isDead) return;
        coins++;
        UpdateUI();
    }

    public void LoseCoins(int amount)
    {
        if (isDead) return;
        coins -= amount;
        if (coins < 0) coins = 0; // Evita que bajen de cero
        UpdateUI();
    }

    public void AddRoom()
    {
        if (isDead) return;
        currentRoom++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthText != null) healthText.text = "Corazones: " + currentHealth;
        if (coinText != null) coinText.text = "Monedas: " + coins;
        if (roomText != null) roomText.text = "Sala: " + currentRoom;
    }

    public void FallToDeath()
    {
        if (isDead) return;
        currentHealth = 0;
        UpdateUI();
        Die();
    }

    private void Die()
    {
        isDead = true;

        // 1. SILENCIAMOS TODOS LOS SONIDOS DEL JUEGO
        AudioListener.pause = true;

        // 2. REPRODUCIMOS EL GAME OVER IGNORANDO EL SILENCIO
        if (sonidoGameOver != null)
        {
            GameObject audioObj = new GameObject("GameOverAudio");
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = sonidoGameOver;
            source.volume = volumenSonidos;
            source.ignoreListenerPause = true; // ¡LA MAGIA ESTÁ AQUÍ!
            source.Play();
            Destroy(audioObj, sonidoGameOver.length + 0.1f);
        }

        // 3. MOSTRAMOS EL PANEL DE GAME OVER
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        // 4. CONGELAMOS EL TIEMPO EN LUGAR DE REINICIAR AUTOMÁTICAMENTE
        Time.timeScale = 0f;
    }

    // --- NUEVAS FUNCIONES PARA LOS BOTONES DEL GAME OVER ---

    public void BotonReiniciar()
    {
        Time.timeScale = 1f; // Descongelamos el tiempo
        AudioListener.pause = false; // Devolvemos el sonido
        SceneManager.LoadScene("SampleScene");
    }

    public void BotonMenuPrincipal()
    {
        Time.timeScale = 1f; // Descongelamos el tiempo
        AudioListener.pause = false; // Devolvemos el sonido
        SceneManager.LoadScene("MenuPrincipal"); // Vuelve al menú principal (asegúrate de que tu escena se llama "Menu")
    }
}