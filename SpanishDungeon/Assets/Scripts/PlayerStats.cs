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

    [Header("Invulnerabilidad")]
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    private bool isDead = false;
    private bool isInvincible = false;

    // CORRECCIÓN CLAVE: Usamos una lista para guardar SOLO las mallas que deben verse
    private List<Renderer> activeRenderers = new List<Renderer>();

    void Start()
    {
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