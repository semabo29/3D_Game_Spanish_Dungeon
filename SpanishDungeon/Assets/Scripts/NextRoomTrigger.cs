using UnityEngine;

public class NextRoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            Debug.Log("¡Has cruzado la puerta! Generando siguiente sala...");

            PlayerStats stats = player.GetComponent<PlayerStats>();
            stats.AddRoom();

            MapGenerator generator = FindFirstObjectByType<MapGenerator>();
            if (generator != null)
            {
                // Ahora le enviamos también el número de la sala actual
                generator.LoadNextRoom(player, stats.currentRoom);
            }
        }
    }
}