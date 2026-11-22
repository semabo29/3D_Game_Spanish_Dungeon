using UnityEngine;

public class WindmillTrap : MonoBehaviour
{
    [Header("Configuración del Molino")]
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Efectos al Golpear (Rúbrica)")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private ParticleSystem particulasGolpe;

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage("un molino lo confundió con un gigante");
                Debug.Log("¡Ay! El aspa del molino manchego te ha golpeado.");

                if (hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position, 0.7f);
                }

                if (particulasGolpe != null)
                {
                    particulasGolpe.Play();
                }
            }
        }
    }
}