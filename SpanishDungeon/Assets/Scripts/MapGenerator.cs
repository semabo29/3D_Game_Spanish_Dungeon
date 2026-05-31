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
    [SerializeField] private GameObject trap1Prefab; // Letra T (Pinchos)
    [SerializeField] private GameObject trap2Prefab; // Letra Y (El Molino)
    [SerializeField] private GameObject trap3Prefab; // Letras U,H,J,K (Radar)

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

        //Buscar todas las cervezas sueltas en el mapa y destruirlas de golpe
        CervezaRota[] cervezasActivas = FindObjectsByType<CervezaRota>(FindObjectsSortMode.None);
        foreach (CervezaRota cerveza in cervezasActivas)
        {
            Destroy(cerveza.gameObject);
        }

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
                int z = (line.Length / 2) - zIndex;

                Vector3 floorPos = new Vector3(x, 0f, z);
                Vector3 objectPos = new Vector3(x, 0.5f, z);
                Vector3 wallPos = new Vector3(x, 1f, z);

                // --- PINTAR EL SUELO ---
                // Solo evitamos pintar suelo si es un hueco vacío ' ', o la trampa de pinchos 'T'
                // Hemos quitado la 'Y' de aquí, así que el molino SÍ tendrá baldosa debajo.
                if (tileChar != ' ' && tileChar != 'T')
                {
                    Instantiate(tilePrefab, floorPos, Quaternion.identity, transform);
                }

                // 2. Colocamos lo que va ENCIMA
                switch (tileChar)
                {
                    case '.': break; // Pasillo vacío. Ya hay suelo, no hacemos nada.

                    case '#': Instantiate(wallPrefab, wallPos, Quaternion.identity, transform); break;
                    case 'D': Instantiate(doorPrefab, wallPos, Quaternion.identity, transform); break;
                    case 'X': Instantiate(exitTriggerPrefab, objectPos, Quaternion.identity, transform); break;

                    case 'S':
                        PlayerController player = FindFirstObjectByType<PlayerController>();
                        if (player != null)
                        {
                            player.transform.position = objectPos;
                            player.targetPosition = objectPos;
                        }
                        break;

                    case 'C': Instantiate(coinPrefab, objectPos, Quaternion.identity, transform); break;

                    case '1': Instantiate(enemy1Prefab, objectPos, Quaternion.identity, transform); break;
                    case '2': Instantiate(enemy2Prefab, objectPos, Quaternion.identity, transform); break;
                    case '3': Instantiate(enemy3Prefab, objectPos, Quaternion.identity, transform); break;
                    case 'B': Instantiate(bossPrefab, objectPos, Quaternion.identity, transform); break;

                    // Las trampas
                    case 'T': Instantiate(trap1Prefab, floorPos, Quaternion.identity, transform); break;

                    // --- EL MOLINO (Trampa 2 / Letra Y) ---
                    // Lo instanciamos en objectPos para que descanse SOBRE la baldosa, no enterrado.
                    case 'Y': Instantiate(trap2Prefab, objectPos + new Vector3(0, 0.5f, 0), Quaternion.identity, transform); break;

                    case 'U':
                        GameObject trapU = Instantiate(trap3Prefab, objectPos, Quaternion.identity, transform);
                        trapU.GetComponent<ArrowTrap>().SetDirection(Vector3.right);
                        break;
                    case 'H':
                        GameObject tpH = Instantiate(trap3Prefab, objectPos, Quaternion.identity, transform);
                        tpH.GetComponent<ArrowTrap>().SetDirection(Vector3.left);
                        break;
                    case 'J':
                        GameObject tpJ = Instantiate(trap3Prefab, objectPos, Quaternion.identity, transform);
                        tpJ.GetComponent<ArrowTrap>().SetDirection(Vector3.forward);
                        break;
                    case 'K':
                        GameObject tpK = Instantiate(trap3Prefab, objectPos, Quaternion.identity, transform);
                        tpK.GetComponent<ArrowTrap>().SetDirection(Vector3.back);
                        break;

                    case 'O': if (decorPrefabs.Length > 0) Instantiate(decorPrefabs[0], objectPos, Quaternion.identity, transform); break;
                    case 'P': if (decorPrefabs.Length > 1) Instantiate(decorPrefabs[1], objectPos, Quaternion.identity, transform); break;
                    case 'Q': if (decorPrefabs.Length > 2) Instantiate(decorPrefabs[2], objectPos, Quaternion.identity, transform); break;
                    case 'R': if (decorPrefabs.Length > 3) Instantiate(decorPrefabs[3], objectPos, Quaternion.identity, transform); break;
                    case 'V': if (decorPrefabs.Length > 4) Instantiate(decorPrefabs[4], objectPos, Quaternion.identity, transform); break;
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