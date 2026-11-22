using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private GameObject enemy1Prefab; 
    [SerializeField] private GameObject enemy2Prefab; 
    [SerializeField] private GameObject enemy3Prefab; 
    [SerializeField] private GameObject bossPrefab;   

    [Header("Prefabs de Trampas")]
    [SerializeField] private GameObject trap1Prefab; 
    [SerializeField] private GameObject trap2Prefab; 
    [SerializeField] private GameObject trap3Prefab; 

    [Header("Prefabs de Decoración")]
    [SerializeField] private GameObject[] decorPrefabs; 

    [Header("Configuración del Suelo (Caída)")]
    [SerializeField] private float timeBeforeFalling = 3f;
    [SerializeField] private float fallSpeed = 1.5f;
    [Tooltip("Segundos que tiembla una fila antes de caer (aviso al jugador)")]
    [SerializeField] private float crumbleWarningTime = 0.6f;

    [Header("Depuración")]
    [Tooltip("Permite saltar de sala con las teclas 0-9 (siempre activo en el editor)")]
    [SerializeField] private bool debugRoomKeysInBuild = false;

    private Coroutine crumbleCoroutine;
    private int currentMapLength = 0;

    void Start()
    {
        LoadMapFromFile(0);
        crumbleCoroutine = StartCoroutine(CrumbleFloorRoutine(1));
    }

    void Update()
    {
        if (!(Application.isEditor || Debug.isDebugBuild || debugRoomKeysInBuild)) return;
        if (Time.timeScale == 0f) return;

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
        PlayerController player = PlayerController.Instance;
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
        //Comprobamos si nos hemos pasado del número de mapas (¡Victoria!)
        if (roomNumber > mapFiles.Length)
        {
            player.GetComponent<PlayerStats>().WinGame();
            return; // Detenemos la función aquí para que no destruya el suelo ni intente cargar la sala 11
        }

        if (crumbleCoroutine != null) StopCoroutine(crumbleCoroutine);

        CervezaRota[] cervezasActivas = FindObjectsByType<CervezaRota>(FindObjectsSortMode.None);
        foreach (CervezaRota cerveza in cervezasActivas)
        {
            Destroy(cerveza.gameObject);
        }

        // Las multas en vuelo no son hijas del generador: también hay que limpiarlas
        ArrowProjectile[] multas = FindObjectsByType<ArrowProjectile>(FindObjectsSortMode.None);
        foreach (ArrowProjectile multa in multas)
        {
            Destroy(multa.gameObject);
        }

        foreach (Transform child in transform)
        {
            // Lo sacamos ya de la jerarquía para que la nueva sala no lo vea (Destroy es diferido)
            child.gameObject.SetActive(false);
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
                // Mayúsculas y minúsculas valen lo mismo (p. ej. la 's' de Sala10.txt)
                char tileChar = char.ToUpperInvariant(line[zIndex]);

                int z = (line.Length / 2) - zIndex;

                Vector3 floorPos = new Vector3(x, 0f, z);
                Vector3 objectPos = new Vector3(x, 0.5f, z);
                Vector3 wallPos = new Vector3(x, 1f, z);

                if (tileChar != ' ' && tileChar != 'T')
                {
                    Instantiate(tilePrefab, floorPos, Quaternion.identity, transform);
                }

                switch (tileChar)
                {
                    case '.': break; 

                    case '#': Instantiate(wallPrefab, wallPos, Quaternion.identity, transform); break;
                    case 'D': Instantiate(doorPrefab, wallPos, Quaternion.identity, transform); break;
                    case 'X': Instantiate(exitTriggerPrefab, objectPos, Quaternion.identity, transform); break;

                    case 'S':
                        PlayerController player = PlayerController.Instance;
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

                    case 'T': Instantiate(trap1Prefab, floorPos, Quaternion.identity, transform); break;

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

                    case 'O':
                        if (decorPrefabs.Length > 0 && decorPrefabs[0] != null)
                            Instantiate(decorPrefabs[0], objectPos, Quaternion.identity, transform);
                        break;
                    case 'P':
                        if (decorPrefabs.Length > 1 && decorPrefabs[1] != null)
                            Instantiate(decorPrefabs[1], objectPos, Quaternion.identity, transform);
                        break;
                    case 'Q':
                        if (decorPrefabs.Length > 2 && decorPrefabs[2] != null)
                            Instantiate(decorPrefabs[2], objectPos, Quaternion.identity, transform);
                        break;
                    case 'R':
                        if (decorPrefabs.Length > 3 && decorPrefabs[3] != null)
                            Instantiate(decorPrefabs[3], objectPos, Quaternion.identity, transform);
                        break;
                    case 'V':
                        if (decorPrefabs.Length > 4 && decorPrefabs[4] != null)
                            Instantiate(decorPrefabs[4], objectPos, Quaternion.identity, transform);
                        break;
                }
            }
        }
    }

    private IEnumerator CrumbleFloorRoutine(int roomNumber)
    {
        // Sin derrumbe en las dos primeras salas ni en la del jefe
        if (roomNumber < 3 || roomNumber == mapFiles.Length) yield break;

        yield return new WaitForSeconds(timeBeforeFalling);

        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.ShowToast("¡El suelo se hunde! ¡Corre!", new Color(1f, 0.45f, 0.2f), 1.2f);
        }

        float warning = Mathf.Min(crumbleWarningTime, fallSpeed);

        for (int x = -1; x <= currentMapLength; x++)
        {
            List<Transform> row = new List<Transform>();
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf && Mathf.RoundToInt(child.position.x) == x) row.Add(child);
            }

            // Aviso: el suelo (y lo que está a ras de suelo) tiembla antes de caer
            if (row.Count > 0 && warning > 0f)
            {
                yield return StartCoroutine(ShakeRow(row, warning));
            }

            foreach (Transform child in row)
            {
                if (child == null) continue;

                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb == null) rb = child.gameObject.AddComponent<Rigidbody>();

                rb.isKinematic = false;
                rb.useGravity = true;
                rb.angularVelocity = Random.insideUnitSphere * 1.5f;

                Destroy(child.gameObject, 3f);
            }

            if (row.Count > 0) GameFeel.Shake(0.05f, 0.12f);

            yield return new WaitForSeconds(fallSpeed - warning);
        }
    }

    private IEnumerator ShakeRow(List<Transform> row, float duration)
    {
        List<Transform> floorPieces = new List<Transform>();
        List<Vector3> origins = new List<Vector3>();
        foreach (Transform t in row)
        {
            // Solo baldosas y trampas de suelo: los enemigos se mueven solos y no deben bloquearse
            if (t != null && t.position.y < 0.3f)
            {
                floorPieces.Add(t);
                origins.Add(t.position);
            }
        }

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float strength = Mathf.Lerp(0.02f, 0.07f, timer / duration);
            for (int i = 0; i < floorPieces.Count; i++)
            {
                if (floorPieces[i] == null) continue;
                floorPieces[i].position = origins[i] + new Vector3(Random.Range(-strength, strength), Random.Range(-strength, 0f), Random.Range(-strength, strength));
            }
            yield return null;
        }

        for (int i = 0; i < floorPieces.Count; i++)
        {
            if (floorPieces[i] != null) floorPieces[i].position = origins[i];
        }
    }
}