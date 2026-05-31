using UnityEngine;

public class CervezaRota : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    [SerializeField] private float duracionBorrachera = 4f; // Cuántos segundos dura el efecto

    void Start()
    {
        //Se autodestruirá a los 4 segundos de nacer si nadie la toca
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

            //Si el jugador pisa exactamente la casilla de la botella
            if (distanceX < 0.3f && distanceZ < 0.3f)
            {
                //Le aplicamos la borrachera al jugador
                player.Enborrachar(duracionBorrachera);

                //La botella rota desaparece
                Destroy(gameObject);
            }
        }
    }
}