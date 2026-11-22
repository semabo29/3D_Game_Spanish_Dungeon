using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip SonidoAgarrarMoneda;
    void Update()
    {
        // Gira sobre sí misma para destacar
        transform.Rotate(0, 90 * Time.deltaTime, 0);
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
                Destroy(gameObject); 

                if (SonidoAgarrarMoneda != null)
                {
                    AudioSource.PlayClipAtPoint(SonidoAgarrarMoneda, Camera.main.transform.position, 1f);
                }
            }
        }
    }
}