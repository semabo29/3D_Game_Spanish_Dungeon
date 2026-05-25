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
        //Busca los enemigos de tipo patrulla
        EnemyPatrol[] activePatrols = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);

        //Busca los enemigos de tipo básico
        Enemigo_basico[] activeBasics = FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None);

        //Si no queda ninguno de ningún tipo, abrimos la puerta
        if (activePatrols.Length == 0 && activeBasics.Length == 0)
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