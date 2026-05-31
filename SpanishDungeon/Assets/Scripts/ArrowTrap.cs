using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    [SerializeField] private GameObject arrowPrefab; // Aquí va el prefab de la Multa
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private Transform spawnPoint;

    [Header("Sonido del Radar")]
    [SerializeField] private AudioClip sonidoFlash; // El sonido de la cámara
    [SerializeField] private float volumenFlash = 0.8f;

    private float timer;
    private Vector3 shootDirection = Vector3.forward;

    public void SetDirection(Vector3 direction)
    {
        shootDirection = direction;
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

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + (shootDirection * 0.6f);

        GameObject arrowObj = Instantiate(arrowPrefab, pos, transform.rotation);
        ArrowProjectile projectile = arrowObj.GetComponent<ArrowProjectile>();

        if (projectile != null)
        {
            projectile.Setup(shootDirection);
        }

        // --- SONIDO DE FLASH ---
        if (sonidoFlash != null)
        {
            // Reproducimos en las "orejas" del jugador para que suene fuerte
            AudioSource.PlayClipAtPoint(sonidoFlash, Camera.main.transform.position, volumenFlash);
        }
    }
}