using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip SonidoAgarrarMoneda;

    private float baseY;
    private float phase;

    void Start()
    {
        baseY = transform.position.y;
        phase = transform.position.x * 0.7f + transform.position.z * 1.3f;
    }

    void Update()
    {
        // Gira sobre sí misma y flota para destacar
        transform.Rotate(0, 120 * Time.deltaTime, 0);
        // Si el suelo se derrumba la moneda cae con él: entonces ya no flota
        if (!TryGetComponent<Rigidbody>(out _))
        {
            Vector3 p = transform.position;
            p.y = baseY + Mathf.Sin(Time.time * 3f + phase) * 0.08f;
            transform.position = p;
        }
        CheckPlayerCollision();
    }

    private void CheckPlayerCollision()
    {
        PlayerController player = PlayerController.Instance;

        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            if (distanceX < 0.3f && distanceZ < 0.3f)
            {
                player.GetComponent<PlayerStats>().AddCoin();
                GameFeel.VoxelBurst(transform.position, new Color(1f, 0.82f, 0.15f), 6, 2.5f);
                GameFeel.FloatingText(transform.position, "+1", new Color(1f, 0.85f, 0.2f), 3f);
                Sfx.Play(SfxType.Pickup, 0.35f, Random.Range(0.95f, 1.1f));
                Destroy(gameObject); 

                if (SonidoAgarrarMoneda != null)
                {
                    AudioSource.PlayClipAtPoint(SonidoAgarrarMoneda, Camera.main.transform.position, 1f);
                }
            }
        }
    }
}