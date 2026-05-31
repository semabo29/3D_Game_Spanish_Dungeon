using UnityEngine;

public class CervezaRota : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    [SerializeField] private float duracionBorrachera = 4f;
    [SerializeField] private AudioClip SonidoBeber;

    void Start()
    {
        Destroy(gameObject, 4f);
    }
    void Update()
    {
        CheckPlayerCollision();
    }

    private void CheckPlayerCollision()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            if (distanceX < 0.3f && distanceZ < 0.3f)
            {
                player.Enborrachar(duracionBorrachera);

                Destroy(gameObject);
                if (SonidoBeber != null)
                {
                    AudioSource.PlayClipAtPoint(SonidoBeber, Camera.main.transform.position, 3f);
                }
            }
        }
    }
}