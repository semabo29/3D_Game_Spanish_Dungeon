using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Estadísticas")]
    public int maxHealth = 3;
    private int currentHealth;
    private int coins = 0;
    public int currentRoom = 1; // Añadimos la sala actual

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI roomText; // Añadimos el texto de la sala

    [Header("Invulnerabilidad")]
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    private bool isDead = false;
    private bool isInvincible = false;
    private Renderer playerRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        playerRenderer = GetComponent<Renderer>();
        UpdateUI();
    }

    public void TakeDamage()
    {
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
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0f;

        while (timer < invincibilityTime)
        {
            if (playerRenderer != null) playerRenderer.enabled = !playerRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (playerRenderer != null) playerRenderer.enabled = true;
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
        if (coins < 0) coins = 0; //Evita que bajen de cero
        UpdateUI();
    }

    // Nueva función para avanzar de sala
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
        // Actualizamos también el texto de la sala aquí
        if (roomText != null) roomText.text = "Sala: " + currentRoom;
    }
    public void FallToDeath()
    {
        if (isDead) return;

        currentHealth = 0; // Vaciamos los corazones
        UpdateUI();

        Debug.Log("¡Te has caído al vacío!");
        Die();
    }
    private void Die()
    {
        isDead = true;
        Debug.Log("¡GAME OVER! Reiniciando el nivel...");

        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        Invoke("RestartScene", 1f);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}