using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Necesario para reiniciar el nivel

public class PlayerStats : MonoBehaviour
{
    [Header("Estadísticas")]
    public int maxHealth = 3;
    private int currentHealth;
    private int coins = 0;

    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI coinText;

    private bool isDead = false; // Control para evitar que muera dos veces a la vez

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage()
    {
        if (isDead) return; // Si ya está muerto, ignoramos el daño

        currentHealth--;
        UpdateUI();

        Debug.Log("¡Ouch! Te quedan " + currentHealth + " corazones.");

        if (currentHealth <= 0)
        {
            Die();
        }
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

        // Desactivamos el script de movimiento para que no pueda escapar estando muerto
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Llamamos a la función RestartScene después de 1 segundo de retraso
        Invoke("RestartScene", 1f);
    }

    private void RestartScene()
    {
        // Vuelve a cargar la escena en la que estamos actualmente
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}