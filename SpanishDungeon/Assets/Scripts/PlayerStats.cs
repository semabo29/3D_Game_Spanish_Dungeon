using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para hacer el parpadeo en el tiempo

public class PlayerStats : MonoBehaviour
{
    [Header("Estadísticas")]
    public int maxHealth = 3;
    private int currentHealth;
    private int coins = 0;

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Invulnerabilidad")]
    [SerializeField] private float invincibilityTime = 1.5f; // Cuánto dura el escudo
    [SerializeField] private float blinkInterval = 0.1f;     // Velocidad del parpadeo

    private bool isDead = false;
    private bool isInvincible = false;

    // Referencia al renderizador para apagar/encender el modelo
    private Renderer playerRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        playerRenderer = GetComponent<Renderer>();
        UpdateUI();
    }

    public void TakeDamage()
    {
        // Si ya está muerto o tiene el escudo de invulnerabilidad, cancelamos el daño
        if (isDead || isInvincible) return;

        currentHealth--;
        UpdateUI();

        Debug.Log("¡Ouch! Te quedan " + currentHealth + " corazones.");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Si sobrevivimos, activamos el parpadeo de invulnerabilidad
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // Esta rutina se ejecuta en segundo plano durante 1.5 segundos
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;

        // Bucle que alterna la visibilidad mientras dure el tiempo
        while (timer < invincibilityTime)
        {
            if (playerRenderer != null)
            {
                playerRenderer.enabled = !playerRenderer.enabled; // Apaga/Enciende
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Al terminar, nos aseguramos de que el cubo vuelva a ser visible
        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
        }

        isInvincible = false; // Ya le pueden volver a hacer daño
    }

    public void AddCoin()
    {
        if (isDead) return;
        coins++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthText != null) healthText.text = "Corazones: " + currentHealth;
        if (coinText != null) coinText.text = "Monedas: " + coins;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("¡GAME OVER! Reiniciando el nivel...");

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        Invoke("RestartScene", 1f);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}