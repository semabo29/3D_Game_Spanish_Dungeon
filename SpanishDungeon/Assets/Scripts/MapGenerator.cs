using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs del Mapa")]
    [SerializeField] private GameObject tilePrefab; // Baldosa normal
    [SerializeField] private GameObject trapPrefab; // Baldosa trampa

    [Header("Dimensiones")]
    // Se mantienen vuestras dimensiones y variables anteriores
    [SerializeField] private int width = 5;
    [SerializeField] private int length = 15;

    [Header("Configuración del Peligro")]
    [Range(0f, 100f)]
    [SerializeField] private float trapChance = 15f; // % de probabilidad de que salga una trampa

    void Start()
    {
        GenerateFloor();
    }

    void GenerateFloor()
    {
        // Respetamos tus bucles exactos: Z para el ancho, X para la longitud/avance
        for (int z = -width / 2; z <= width / 2; z++)
        {
            for (int x = 0; x < length; x++)
            {
                // Protegemos la casilla inicial (0,0,0) para que el jugador no empiece muerto
                bool isStartingPosition = (x == 0 && z == 0);

                // Vuestra posición de spawn corregida para la cámara de diamante
                Vector3 spawnPos = new Vector3(x, 0f, z);
                GameObject objectToSpawn;

                // Si no es el inicio y el número aleatorio cae en el %, elegimos trampa
                if (!isStartingPosition && Random.Range(0f, 100f) < trapChance)
                {
                    objectToSpawn = trapPrefab;
                }
                else
                {
                    objectToSpawn = tilePrefab;
                }

                // Instanciamos el objeto correspondiente manteniendo vuestros ejes
                GameObject newObject = Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
                newObject.transform.parent = this.transform;
            }
        }
    }
}