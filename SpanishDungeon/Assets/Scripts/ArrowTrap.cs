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
    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private Vector3 baseScale;
    private const float WarningTime = 0.45f;
    private Vector3 shootDirection = Vector3.forward;

    public void SetDirection(Vector3 direction)
    {
        shootDirection = direction;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void Start()
    {
        timer = Random.Range(0f, fireRate);
        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Suceso "huelga de radares": no disparan en toda la sala
        if (RoomEvents.RadarsOnStrike)
        {
            transform.localScale = baseScale;
            Tint(new Color(0.6f, 0.6f, 0.65f));
            timer = 0f;
            return;
        }
        timer += Time.deltaTime;

        // Aviso: el radar parpadea en rojo y se hincha justo antes de disparar
        bool warning = GameSettings.DangerMarkers && timer >= fireRate - WarningTime;
        float blink = warning ? Mathf.Abs(Mathf.Sin((timer - (fireRate - WarningTime)) * 20f)) : 0f;
        transform.localScale = baseScale * (1f + blink * 0.12f);
        Tint(Color.Lerp(Color.white, new Color(1f, 0.25f, 0.2f), blink));

        if (timer >= fireRate)
        {
            timer = 0f;
            FireArrow();
        }
    }

    private void Tint(Color c)
    {
        if (renderers == null) return;
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", c);
            r.SetPropertyBlock(mpb);
        }
    }

    void OnDisable()
    {
        if (baseScale != Vector3.zero) transform.localScale = baseScale;
        Tint(Color.white);
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