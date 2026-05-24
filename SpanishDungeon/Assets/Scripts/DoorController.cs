using UnityEngine;

public class DoorController : MonoBehaviour
{
    private bool isOpen = false;

    void Start()
    {
        // Cambiamos el color de la puerta a amarillo/dorado para que destaque
        Renderer myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            myRenderer.material.color = Color.yellow;
        }
    }

    void Update()
    {
        // Si la puerta está cerrada, comprobamos los enemigos
        if (!isOpen)
        {
            CheckEnemies();
        }
    }

    private void CheckEnemies()
    {
        // Busca a todos los enemigos activos en la escena
        EnemyPatrol[] activeEnemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);

        // Si ya no queda ninguno, abrimos la puerta
        if (activeEnemies.Length == 0)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        Debug.Log("¡Sala despejada! La puerta se ha abierto.");

        // Desactivamos la puerta físicamente para que el jugador pueda cruzar al siguiente nivel
        gameObject.SetActive(false);
    }
}