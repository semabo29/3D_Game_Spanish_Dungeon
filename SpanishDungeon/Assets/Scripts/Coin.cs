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
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            if (distanceX < 0.3f && distanceZ < 0.3f)
            {
                // Buscamos el script de stats y le sumamos la moneda
                player.GetComponent<PlayerStats>().AddCoin();
                Destroy(gameObject); // La moneda desaparece

                if (SonidoAgarrarMoneda != null)//Reproducimos el sonido de coger una moneda si existe
                {
                    //Crea un "altavoz" (fuente de audio) temporal en la posición de la camara (oidos del juego) a 70% volumen , ja que borramos la moneda
                    AudioSource.PlayClipAtPoint(SonidoAgarrarMoneda, Camera.main.transform.position, 1f);
                }
            }
        }
    }
}