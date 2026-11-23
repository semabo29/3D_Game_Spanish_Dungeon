using UnityEngine;

public class NextRoomTrigger : MonoBehaviour
{
    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null || stats.IsDead || PlayerStats.GameEnded) return;
        used = true;
        RoomEvents.Clear();

        // Al salir de las salas 3, 6 y 9 paramos en el chiringuito de Paco
        if (Chiringuito.OpensAfterRoom(stats.currentRoom))
        {
            ModalMenu.OpenShop(stats, stats.currentRoom, () => Advance(player, stats));
        }
        else
        {
            Advance(player, stats);
        }
    }

    private static void Advance(PlayerController player, PlayerStats stats)
    {
        if (player == null || stats == null || stats.IsDead) return;
        stats.AddRoom();
        MapGenerator generator = MapGenerator.Instance != null ? MapGenerator.Instance : FindFirstObjectByType<MapGenerator>();
        if (generator != null) generator.LoadNextRoom(player, stats.currentRoom);
    }
}
