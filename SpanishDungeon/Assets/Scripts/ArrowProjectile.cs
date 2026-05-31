using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    public int damage = 1;
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
        // 1. Si choca con el Jugador
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(); // Le quitamos vida
                Debug.Log("¡El jugador ha sido golpeado por una flecha!");
            }
            Destroy(gameObject);
        }
        // 2. Si choca con un muro o una puerta (y no es el propio suelo ni otra trampa)
        else if (!other.isTrigger)
        {
            // Ignoramos el suelo comprobando que el objeto con el que chocamos está por encima de Y=0
            if (other.transform.position.y > 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }
}