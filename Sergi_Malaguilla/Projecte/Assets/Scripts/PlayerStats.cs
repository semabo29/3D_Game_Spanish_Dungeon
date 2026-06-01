using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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

    private List<Renderer> activeRenderers = new List<Renderer>();

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        currentHealth = maxHealth;

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

        UpdateUI();
    }

    public void WinGame()
    {
        if (isDead) return; //Si ya morimos en el último segundo, no ganamos

        //Quitamos el control al jugador
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        AudioListener.pause = true; //Silenciamos el juego

        //Mostramos el panel
        if (victoryPanel != null) victoryPanel.SetActive(true);

        // Congelamos el tiempo
        Time.timeScale = 0f;
    }

    public void TakeDamage()
    {
        if (isDead || isInvincible) return;

        currentHealth--;
        UpdateUI();

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine(0.5f));
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
            foreach (Renderer ren in activeRenderers)
            {
                if (ren != null) ren.enabled = !ren.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

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
        if (coins < 0) coins = 0;
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

        StartCoroutine(DieRoutine(1.5f));
    }

    private IEnumerator DieRoutine(float delayBeforeGameOver)
    {
        isDead = true;

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        yield return new WaitForSeconds(delayBeforeGameOver);

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

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
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