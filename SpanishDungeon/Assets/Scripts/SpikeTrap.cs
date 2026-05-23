using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Configuración del Temporizador")]
    [SerializeField] private float timeActive = 2f;   // Cuánto tiempo pincha (Rojo)
    [SerializeField] private float timeInactive = 2f; // Cuánto tiempo es segura (Verde)

    private bool isDangerous = false;
    private float timer = 0f;

    // Referencia para cambiar el color del cubo provisionalmente
    private Renderer myRenderer;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        // Empezamos desactivados (seguros)
        SetTrapState(false);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Lógica del temporizador para alternar estados
        if (isDangerous && timer >= timeActive)
        {
            SetTrapState(false); // Se apaga
        }
        else if (!isDangerous && timer >= timeInactive)
        {
            SetTrapState(true);  // Se enciende
        }

        // Si la trampa es peligrosa, chequeamos constantemente si el jugador está encima
        if (isDangerous)
        {
            CheckPlayerCollision();
        }
    }

    private void SetTrapState(bool dangerous)
    {
        isDangerous = dangerous;
        timer = 0f; // Reiniciamos el contador

        // Código provisional visual: Cambiamos el color del cubo
        if (myRenderer != null)
        {
            myRenderer.material.color = dangerous ? Color.red : Color.green;
        }
    }

    private void CheckPlayerCollision()
    {
        // Buscamos al jugador en la escena por su componente PlayerController
        // (Esto es supersencillo y limpio para prototipos)
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            // Comparamos la posición X y Z de la trampa con las del jugador
            // Ignoramos la Y porque el jugador está un poco más arriba (0.5f) sobre el suelo
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            // Si ambos están en la misma baldosa (distancia muy cercana a 0)
            if (distanceX < 0.1f && distanceZ < 0.1f)
            {
                TriggerDamage();
            }
        }
    }

    private void TriggerDamage()
    {
        // Por ahora, como no tenemos sistema de vidas, ponemos un aviso en consola
        Debug.LogWarning("¡ZAS! El jugador ha pisado los pinchos y ha recibido daño.");

        // Aquí podrías destruir al jugador, restarle vida, etc.
    }
}