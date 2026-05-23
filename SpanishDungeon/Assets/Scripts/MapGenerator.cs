using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Pasamos el Prefab azul de la baldosa
    [SerializeField] private GameObject tilePrefab;

    // Tamaño de la habitación de pruebas (Filas x Columnas)
    [SerializeField] private int width = 5;
    [SerializeField] private int length = 15; // Más largo para poder avanzar hacia adelante

    void Start()
    {
        GenerateFloor();
    }

    void GenerateFloor()
    {
        // Ahora el ancho se mide en Z y la longitud (el avance del nivel) se extiende en X
        for (int z = -width / 2; z <= width / 2; z++)
        {
            for (int x = 0; x < length; x++)
            {
                // Intercambiamos los ejes: el mapa avanza hacia las X positivas (arriba-derecha en tu pantalla con esta cámara)
                Vector3 spawnPos = new Vector3(x, 0f, z);

                GameObject newTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                newTile.transform.parent = this.transform;
            }
        }
    }
}