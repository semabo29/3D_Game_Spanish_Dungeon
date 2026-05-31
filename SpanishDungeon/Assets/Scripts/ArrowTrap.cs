using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private Transform spawnPoint;

    private float timer;
    private Vector3 shootDirection = Vector3.forward; // Dirección por defecto

    // El generador llamará a esta función al crear la trampa
    public void SetDirection(Vector3 direction)
    {
        shootDirection = direction;
        // Rotamos físicamente la torreta para que visualmente mire hacia donde dispara
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void Start()
    {
        timer = Random.Range(0f, fireRate);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= fireRate)
        {
            timer = 0f;
            FireArrow();
        }
    }

    void FireArrow()
    {
        if (arrowPrefab == null) return;

        // Modificado: Ahora la flecha nace un poco más adelante (0.6f) para no chocar con la propia trampa
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + (shootDirection * 0.6f);

        GameObject arrowObj = Instantiate(arrowPrefab, pos, transform.rotation);

        ArrowProjectile projectile = arrowObj.GetComponent<ArrowProjectile>();
        if (projectile != null)
        {
            projectile.Setup(shootDirection);
        }
    }
}