using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs del Mapa")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject exitTriggerPrefab;

    [Header("Configuración Base de Sala")]
    [SerializeField] private int baseWidth = 5;
    [SerializeField] private int baseLength = 10;

    // Variables actuales que cambian con la dificultad
    private int width;
    private int length;

    [Header("Dificultad Dinámica")]
    [SerializeField] private float baseTrapChance = 10f;
    [SerializeField] private float baseEnemyChance = 15f;
    [SerializeField] private float coinChance = 20f;

    private float trapChance;
    private float enemyChance;

    [Header("Configuración del Suelo (Caída)")]
    [SerializeField] private float timeBeforeFalling = 3f;
    [SerializeField] private float fallSpeed = 1.5f; // Lo hemos cambiado a 1.5s para que sea más justo

    private Coroutine crumbleCoroutine;

    void Start()
    {
        // Inicializamos los valores en la sala 1
        width = baseWidth;
        length = baseLength;
        trapChance = baseTrapChance;
        enemyChance = baseEnemyChance;

        GenerateRoom();
        crumbleCoroutine = StartCoroutine(CrumbleFloorRoutine(1));
    }

    void Update()
    {
        // REQUISITO PDF: Teclas del '0' al '9' para ir directamente al nivel correspondiente
        // Asumimos que '1' es la sala 1, '2' la sala 2... y '0' la sala 10 (Boss)
        if (Input.GetKeyDown(KeyCode.Alpha1)) JumpToRoom(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) JumpToRoom(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) JumpToRoom(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) JumpToRoom(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) JumpToRoom(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) JumpToRoom(6);
        if (Input.GetKeyDown(KeyCode.Alpha7)) JumpToRoom(7);
        if (Input.GetKeyDown(KeyCode.Alpha8)) JumpToRoom(8);
        if (Input.GetKeyDown(KeyCode.Alpha9)) JumpToRoom(9);
        if (Input.GetKeyDown(KeyCode.Alpha0)) JumpToRoom(10);
    }

    // Método para los atajos de teclado
    private void JumpToRoom(int targetRoom)
    {
        Debug.Log("Saltando a la sala: " + targetRoom);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();

            // Forzamos el número de sala en el jugador
            stats.currentRoom = targetRoom - 1;
            stats.AddRoom(); // Esto lo pondrá en targetRoom y actualizará la UI

            // Calculamos la dificultad basada en la sala destino
            length = baseLength + (2 * (targetRoom - 1));
            trapChance = baseTrapChance + (2f * (targetRoom - 1));
            enemyChance = baseEnemyChance + (3f * (targetRoom - 1));

            // Cargamos el mapa
            LoadNextRoom(player, targetRoom);
        }
    }

    public void LoadNextRoom(PlayerController player, int roomNumber)
    {
        if (crumbleCoroutine != null) StopCoroutine(crumbleCoroutine);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Si venimos del trigger normal, subimos la dificultad paso a paso
        // Si venimos del atajo (JumpToRoom), ya lo hemos calculado arriba
        if (roomNumber > 1 && Input.anyKeyDown == false)
        {
            length += 2;
            trapChance += 2f;
            enemyChance += 3f;
        }

        GenerateRoom();

        player.transform.position = new Vector3(0, 0.5f, 0);
        player.targetPosition = player.transform.position;

        crumbleCoroutine = StartCoroutine(CrumbleFloorRoutine(roomNumber));
    }

    void GenerateRoom()
    {
        for (int z = -width / 2; z <= width / 2; z++)
        {
            for (int x = 0; x < length; x++)
            {
                bool isSafeZone = (x < 2);
                Vector3 spawnPos = new Vector3(x, 0f, z);
                GameObject objectToSpawn = tilePrefab;

                if (!isSafeZone)
                {
                    float randomValue = Random.Range(0f, 100f);

                    if (randomValue < enemyChance)
                    {
                        Vector3 enemyPos = new Vector3(x, 0.5f, z);
                        Instantiate(enemyPrefab, enemyPos, Quaternion.identity, transform);
                    }
                    else if (randomValue < enemyChance + trapChance)
                    {
                        objectToSpawn = trapPrefab;
                    }
                    else if (randomValue < enemyChance + trapChance + coinChance)
                    {
                        Vector3 coinPos = new Vector3(x, 0.5f, z);
                        Instantiate(coinPrefab, coinPos, Quaternion.identity, transform);
                    }
                }

                Instantiate(objectToSpawn, spawnPos, Quaternion.identity, transform);
            }
        }

        GenerateWallsAndDoor();
    }

    void GenerateWallsAndDoor()
    {
        for (int x = 0; x <= length + 1; x++)
        {
            Vector3 leftWallPos = new Vector3(x, 1f, (width / 2) + 1);
            Instantiate(wallPrefab, leftWallPos, Quaternion.identity, transform);
        }

        for (int z = -width / 2; z <= width / 2; z++)
        {
            Vector3 frontPos = new Vector3(length, 1f, z);

            if (z == 0)
            {
                Instantiate(doorPrefab, frontPos, Quaternion.identity, transform);
                Instantiate(tilePrefab, new Vector3(length, 0f, 0), Quaternion.identity, transform);

                Vector3 exitPos = new Vector3(length + 1, 0.5f, 0);
                Instantiate(exitTriggerPrefab, exitPos, Quaternion.identity, transform);
                Instantiate(tilePrefab, new Vector3(length + 1, 0f, 0), Quaternion.identity, transform);
            }
            else
            {
                Instantiate(wallPrefab, frontPos, Quaternion.identity, transform);
            }
        }
    }

    private IEnumerator CrumbleFloorRoutine(int roomNumber)
    {
        if (roomNumber < 3 || roomNumber == 10) yield break;

        yield return new WaitForSeconds(timeBeforeFalling);

        for (int x = -1; x <= length; x++)
        {
            foreach (Transform child in transform)
            {
                if (Mathf.RoundToInt(child.position.x) == x)
                {
                    Rigidbody rb = child.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = child.gameObject.AddComponent<Rigidbody>();
                    }

                    rb.isKinematic = false;
                    rb.useGravity = true;

                    Destroy(child.gameObject, 3f);
                }
            }
            yield return new WaitForSeconds(fallSpeed);
        }
    }
}