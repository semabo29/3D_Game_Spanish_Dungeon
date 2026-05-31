using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Configuración de la Multa")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int monedasDeMulta = 2; // Cuántas monedas roba

    [Header("Sonido de Impacto")]
    [SerializeField] private AudioClip sonidoImpacto; // Un sonido tipo "papel" o "error"
    [SerializeField] private float volumenImpacto = 0.8f;

    private Vector3 moveDirection;

    public void Setup(Vector3 direction)
    {
        moveDirection = direction;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        // Se destruye a los 5 segundos si no choca con nada para no consumir memoria
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignoramos la propia caja del radar para que no choque consigo misma al salir
        if (other.GetComponent<ArrowTrap>() != null) return;

        // 1. Si choca con el Jugador
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // ¡El combo letal de la DGT!
                stats.TakeDamage(); // Quita 1 corazón de vida
                stats.LoseCoins(monedasDeMulta); // Quita las 2 monedas

                Debug.Log("¡Te ha pillado el radar! Pierdes 1 vida y " + monedasDeMulta + " monedas.");
            }

            // Reproducimos el sonido al chocar contra el jugador en la posición de la cámara
            if (sonidoImpacto != null)
            {
                AudioSource.PlayClipAtPoint(sonidoImpacto, Camera.main.transform.position, volumenImpacto);
            }

            Destroy(gameObject); // La multa desaparece al darte
        }
        // 2. Si choca con un muro o una puerta (y no es el suelo)
        else if (!other.isTrigger)
        {
            if (other.transform.position.y > 0.1f)
            {
                Destroy(gameObject); // La multa se destruye contra la pared
            }
        }
    }
}