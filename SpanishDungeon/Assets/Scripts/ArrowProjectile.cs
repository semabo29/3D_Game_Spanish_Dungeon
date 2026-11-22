using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Configuración de la Multa")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int monedasDeMulta = 2; 

    [Header("Sonido de Impacto")]
    [SerializeField] private AudioClip sonidoImpacto; 
    [SerializeField] private float volumenImpacto = 0.8f;

    private Vector3 moveDirection;

    public void Setup(Vector3 direction)
    {
        moveDirection = direction;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ArrowTrap>() != null) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.LoseCoins(monedasDeMulta); 
                stats.TakeDamage(); 
                GameFeel.FloatingText(other.transform.position + Vector3.left * 0.5f, "¡MULTA!", new Color(0.4f, 0.65f, 1f), 4.5f);

                Debug.Log("¡Te ha pillado el radar! Pierdes 1 vida y " + monedasDeMulta + " monedas.");
            }

            if (sonidoImpacto != null)
            {
                AudioSource.PlayClipAtPoint(sonidoImpacto, Camera.main.transform.position, volumenImpacto);
            }

            GameFeel.VoxelBurst(transform.position, new Color(0.9f, 0.9f, 0.95f), 6, 2f);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            if (other.transform.position.y > 0.1f)
            {
                Destroy(gameObject); 
            }
        }
    }
}