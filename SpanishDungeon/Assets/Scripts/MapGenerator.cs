using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs del Mapa")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject wallPrefab; // Muros
    [SerializeField] private GameObject doorPrefab; // Puerta

    [Header("Dimensiones de la Sala")]
    [SerializeField] private int width = 5;
    [SerializeField] private int length = 10; // Hacemos la sala más corta para probar rápido

    [Header("Configuración del Peligro")]
    [Range(0f, 100f)][SerializeField] private float trapChance = 10f;
    [Range(0f, 100f)][SerializeField] private float enemyChance = 15f;

    void Start()
    {
        GenerateRoom();
    }

    void GenerateRoom()
    {
        // 1. Generamos el SUELO, TRAMPAS y ENEMIGOS
        for (int z = -width / 2; z <= width / 2; z++)
        {
            for (int x = 0; x < length; x++)
            {
                bool isStartingPosition = (x == 0 && z == 0);
                Vector3 spawnPos = new Vector3(x, 0f, z);
                GameObject objectToSpawn = tilePrefab;

                if (!isStartingPosition)
                {
                    float randomValue = Random.Range(0f, 100f);
                    if (randomValue < enemyChance)
                    {
                        Vector3 enemyPos = new Vector3(x, 0.5f, z);
                        GameObject newEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                        newEnemy.transform.parent = this.transform;
                    }
                    else if (randomValue < enemyChance + trapChance)
                    {
                        objectToSpawn = trapPrefab;
                    }
                }

                GameObject newTile = Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
                newTile.transform.parent = this.transform;
            }
        }

        // 2. Generamos los MUROS y la PUERTA (Estilo Diorama Isométrico)
        GenerateWallsAndDoor();
    }

    void GenerateWallsAndDoor()
    {
        // Muro Izquierdo (Visualmente arriba a la izquierda en tu pantalla, eje Z positivo)
        for (int x = 0; x <= length; x++)
        {
            Vector3 leftWallPos = new Vector3(x, 1f, (width / 2) + 1);
            Instantiate(wallPrefab, leftWallPos, Quaternion.identity, transform);
        }

        // Muro Frontal y Puerta (Visualmente arriba a la derecha en tu pantalla, eje X positivo)
        for (int z = -width / 2; z <= width / 2; z++)
        {
            Vector3 frontPos = new Vector3(length, 1f, z);

            if (z == 0) // Justo en el centro del muro frontal (z=0), ponemos la puerta
            {
                Instantiate(doorPrefab, frontPos, Quaternion.identity, transform);

                // Ponemos una baldosa normal debajo de la puerta por si acaso
                Instantiate(tilePrefab, new Vector3(length, 0f, 0), Quaternion.identity, transform);
            }
            else // El resto son muros
            {
                Instantiate(wallPrefab, frontPos, Quaternion.identity, transform);
            }
        }
    }
}