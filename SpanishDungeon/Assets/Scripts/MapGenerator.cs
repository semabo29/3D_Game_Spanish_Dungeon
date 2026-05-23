using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs del Mapa")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private GameObject enemyPrefab; // <- Nuevo Prefab para el enemigo

    [Header("Dimensiones")]
    [SerializeField] private int width = 5;
    [SerializeField] private int length = 15;

    [Header("Configuración del Peligro")]
    [Range(0f, 100f)][SerializeField] private float trapChance = 15f;
    [Range(0f, 100f)][SerializeField] private float enemyChance = 5f; // <- % de probabilidad de enemigos (más bajo que trampas)

    void Start()
    {
        GenerateFloor();
    }

    void GenerateFloor()
    {
        for (int z = -width / 2; z <= width / 2; z++)
        {
            for (int x = 0; x < length; x++)
            {
                bool isStartingPosition = (x == 0 && z == 0);
                Vector3 spawnPos = new Vector3(x, 0f, z);
                GameObject objectToSpawn = tilePrefab; // Por defecto es baldosa normal

                if (!isStartingPosition)
                {
                    float randomValue = Random.Range(0f, 100f);

                    // Lógica para decidir si spawnea trampa o enemigo sobre una baldosa
                    if (randomValue < enemyChance)
                    {
                        // Spawneamos el enemigo un poco elevado (Y: 0.5) para que no se entierre
                        Vector3 enemyPos = new Vector3(x, 0.5f, z);
                        GameObject newEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                        newEnemy.transform.parent = this.transform;
                    }
                    else if (randomValue < enemyChance + trapChance)
                    {
                        objectToSpawn = trapPrefab;
                    }
                }

                // Siempre instanciamos la baldosa (normal o trampa) debajo
                GameObject newTile = Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
                newTile.transform.parent = this.transform;
            }
        }
    }
}