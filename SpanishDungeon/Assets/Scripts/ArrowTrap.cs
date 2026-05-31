using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    [SerializeField] private GameObject arrowPrefab; 
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private Transform spawnPoint;

    [Header("Sonido del Radar")]
    [SerializeField] private AudioClip sonidoFlash; 
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

        if (sonidoFlash != null)
        {
            AudioSource.PlayClipAtPoint(sonidoFlash, Camera.main.transform.position, volumenFlash);
        }
    }
}