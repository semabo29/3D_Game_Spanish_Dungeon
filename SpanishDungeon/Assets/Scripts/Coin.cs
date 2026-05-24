using UnityEngine;

public class Coin : MonoBehaviour
{
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
            }
        }
    }
}