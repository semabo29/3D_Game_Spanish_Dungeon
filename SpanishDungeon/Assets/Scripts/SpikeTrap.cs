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
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            // Si el jugador pisa la trampa
            if (distanceX < 0.1f && distanceZ < 0.1f)
            {
                // Le aplicamos el daño (la invulnerabilidad decidirá si entra o no)
                player.GetComponent<PlayerStats>().TakeDamage();
            }
        }
    }
}