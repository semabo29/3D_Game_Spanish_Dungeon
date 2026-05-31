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
                stats.TakeDamage(); 
                stats.LoseCoins(monedasDeMulta); 

                Debug.Log("¡Te ha pillado el radar! Pierdes 1 vida y " + monedasDeMulta + " monedas.");
            }

            if (sonidoImpacto != null)
            {
                AudioSource.PlayClipAtPoint(sonidoImpacto, Camera.main.transform.position, volumenImpacto);
            }

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