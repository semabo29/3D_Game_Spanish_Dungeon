using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
    [Header("Archivos de Mapas (.txt)")]
    [Tooltip("Arrastra aquí tus archivos txt en orden (Sala 1 a 10)")]
    [SerializeField] private TextAsset[] mapFiles;

    [Header("Prefabs de Estructura")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject exitTriggerPrefab;
    [SerializeField] private GameObject coinPrefab;

    [Header("Prefabs de Enemigos")]
    [SerializeField] private GameObject enemy1Prefab; // Patrulla
    [SerializeField] private GameObject enemy2Prefab; // Perseguidor/Toro
    [SerializeField] private GameObject enemy3Prefab; // Trampero/Telarañas
    [SerializeField] private GameObject bossPrefab;   // Jefe Sala 10

    [Header("Prefabs de Trampas")]
    [SerializeField] private GameObject trap1Prefab;
    [SerializeField] private GameObject trap2Prefab;
    [SerializeField] private GameObject trap3Prefab; // Este es el ArrowTrap

    [Header("Prefabs de Decoración")]
    [SerializeField] private GameObject[] decorPrefabs; // Array para meter las 5 decoraciones

    [Header("Configuración del Suelo (Caída)")]
    [SerializeField] private float timeBeforeFalling = 3f;
    [SerializeField] private float fallSpeed = 1.5f;

    private Coroutine crumbleCoroutine;
    private int currentMapLength = 0;

    void Start()
    {
        LoadMapFromFile(0);
        crumbleCoroutine = StartCoroutine(CrumbleFloorRoutine(1));
    }

    void Update()
    {
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

    private void JumpToRoom(int targetRoom)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            stats.currentRoom = targetRoom - 1;
            stats.AddRoom();
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

        LoadMapFromFile(roomNumber - 1);
        crumbleCoroutine = StartCoroutine(CrumbleFloorRoutine(roomNumber));
    }

    private void LoadMapFromFile(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= mapFiles.Length) return;

        string[] lines = mapFiles[mapIndex].text.Trim().Split('\n');
        currentMapLength = lines.Length;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            int x = (lines.Length - 1) - i;

            for (int zIndex = 0; zIndex < line.Length; zIndex++)
            {
                char tileChar = line[zIndex];

                // --- ARREGLO DEL EFECTO ESPEJO ---
                // Invertimos el cálculo para que la izquierda del texto coincida con la izquierda visual (+Z)
                int z = (line.Length / 2) - zIndex;

                Vector3 floorPos = new Vector3(x, 0f, z);
                Vector3 objectPos = new Vector3(x, 0.5f, z);
                Vector3 wallPos = new Vector3(x, 1f, z);

                switch (tileChar)
                {
                    case '.': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); break;
                    case '#': Instantiate(wallPrefab, wallPos, Quaternion.identity, transform); break;
                    case 'D':
                        Instantiate(tilePrefab, floorPos, Quaternion.identity, transform);
                        Instantiate(doorPrefab, wallPos, Quaternion.identity, transform);
                        break;
                    case 'X':
                        Instantiate(tilePrefab, floorPos, Quaternion.identity, transform);
                        Instantiate(exitTriggerPrefab, objectPos, Quaternion.identity, transform);
                        break;
                    case 'S':
                        Instantiate(tilePrefab, floorPos, Quaternion.identity, transform);
                        PlayerController player = FindFirstObjectByType<PlayerController>();
                        if (player != null)
                        {
                            player.transform.position = objectPos;
                            player.targetPosition = objectPos;
                        }
                        break;
                    case 'C':
                        Instantiate(tilePrefab, floorPos, Quaternion.identity, transform);
                        Instantiate(coinPrefab, objectPos, Quaternion.identity, transform);
                        break;

                    case '1': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); Instantiate(enemy1Prefab, objectPos, Quaternion.identity, transform); break;
                    case '2': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); Instantiate(enemy2Prefab, objectPos, Quaternion.identity, transform); break;
                    case '3': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); Instantiate(enemy3Prefab, objectPos, Quaternion.identity, transform); break;
                    case 'B': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); Instantiate(bossPrefab, objectPos, Quaternion.identity, transform); break;

                    case 'T': Instantiate(trap1Prefab, floorPos, Quaternion.identity, transform); break;
                    case 'Y': Instantiate(trap2Prefab, floorPos, Quaternion.identity, transform); break;

                    // --- ARREGLO DE LAS FLECHAS (Adaptadas a tus controles) ---
                    // U: Dispara Adelante (En tu cámara, adelante es Vector3.right)
                    case 'U':
                        GameObject trapU = Instantiate(trap3Prefab, floorPos, Quaternion.identity, transform);
                        trapU.GetComponent<ArrowTrap>().SetDirection(Vector3.right);
                        break;
                    // H: Dispara Atrás (En tu cámara, atrás es Vector3.left)
                    case 'H':
                        GameObject tpH = Instantiate(trap3Prefab, floorPos, Quaternion.identity, transform);
                        tpH.GetComponent<ArrowTrap>().SetDirection(Vector3.left);
                        break;
                    // J: Dispara Izquierda (En tu cámara, izquierda es Vector3.forward)
                    case 'J':
                        GameObject tpJ = Instantiate(trap3Prefab, floorPos, Quaternion.identity, transform);
                        tpJ.GetComponent<ArrowTrap>().SetDirection(Vector3.forward);
                        break;
                    // K: Dispara Derecha (En tu cámara, derecha es Vector3.back)
                    case 'K':
                        GameObject tpK = Instantiate(trap3Prefab, floorPos, Quaternion.identity, transform);
                        tpK.GetComponent<ArrowTrap>().SetDirection(Vector3.back);
                        break;

                    case 'O': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); if (decorPrefabs.Length > 0) Instantiate(decorPrefabs[0], objectPos, Quaternion.identity, transform); break;
                    case 'P': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); if (decorPrefabs.Length > 1) Instantiate(decorPrefabs[1], objectPos, Quaternion.identity, transform); break;
                    case 'Q': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); if (decorPrefabs.Length > 2) Instantiate(decorPrefabs[2], objectPos, Quaternion.identity, transform); break;
                    case 'R': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); if (decorPrefabs.Length > 3) Instantiate(decorPrefabs[3], objectPos, Quaternion.identity, transform); break;
                    case 'V': Instantiate(tilePrefab, floorPos, Quaternion.identity, transform); if (decorPrefabs.Length > 4) Instantiate(decorPrefabs[4], objectPos, Quaternion.identity, transform); break;
                }
            }
        }
    }

    private IEnumerator CrumbleFloorRoutine(int roomNumber)
    {
        if (roomNumber < 3 || roomNumber == 10) yield break;

        yield return new WaitForSeconds(timeBeforeFalling);

        for (int x = -1; x <= currentMapLength; x++)
        {
            foreach (Transform child in transform)
            {
                if (Mathf.RoundToInt(child.position.x) == x)
                {
                    Rigidbody rb = child.GetComponent<Rigidbody>();
                    if (rb == null) rb = child.gameObject.AddComponent<Rigidbody>();

                    rb.isKinematic = false;
                    rb.useGravity = true;

                    Destroy(child.gameObject, 3f);
                }
            }
            yield return new WaitForSeconds(fallSpeed);
        }
    }
}