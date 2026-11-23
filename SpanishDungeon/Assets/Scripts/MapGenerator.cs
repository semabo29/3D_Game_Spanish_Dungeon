using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    // Bloquea el control mientras la sala se está montando
    public static bool InputLocked { get; private set; }

    public RoomLayout Layout { get; private set; }

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
    private Coroutine assembleCoroutine;
    private int currentMapLength = 0;

    void Awake()
    {
        Instance = this;
        InputLocked = false;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        InputLocked = false;
    }

    // Modo pruebas para la versión web: añade ?dev=1 a la URL
    private static bool? devMode;
    public static bool DevMode
    {
        get
        {
            if (!devMode.HasValue)
            {
                string url = Application.absoluteURL ?? "";
                devMode = url.Contains("dev=1");
            }
            return devMode.Value;
        }
    }

    void Start()
    {
        LoadMapFromFile(0);
        crumbleCoroutine = StartCoroutine(CrumbleFloorRoutine(1));
    }

    void Update()
    {
        if (!(Application.isEditor || Debug.isDebugBuild || debugRoomKeysInBuild || DevMode)) return;
        if (Time.timeScale == 0f || ModalMenu.IsOpen) return;

        // Modo pruebas: T abre el chiringuito
        if (Input.GetKeyDown(KeyCode.T) && PlayerController.Instance != null)
        {
            PlayerStats ps = PlayerController.Instance.GetComponent<PlayerStats>();
            if (ps != null) ModalMenu.OpenShop(ps, ps.currentRoom, null);
        }
        // Modo pruebas: V gana la partida (para probar la pantalla de victoria)
        if (Input.GetKeyDown(KeyCode.V) && PlayerController.Instance != null)
        {
            PlayerStats ps = PlayerController.Instance.GetComponent<PlayerStats>();
            if (ps != null && !PlayerStats.GameEnded) ps.WinGame();
        }
        // Modo pruebas: E ofrece estampitas
        if (Input.GetKeyDown(KeyCode.E) && PlayerController.Instance != null)
        {
            PlayerStats ps = PlayerController.Instance.GetComponent<PlayerStats>();
            if (ps != null) ModalMenu.OpenBlessing(ps, null);
        }

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
        if (assembleCoroutine != null) StopCoroutine(assembleCoroutine);

        RoomEvents.Clear();
        if (GameHUD.Instance != null) GameHUD.Instance.PlayRoomTransition();
        Sfx.Play(SfxType.Whoosh, 0.7f);

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

        RoomLayout layout = new RoomLayout { room = mapIndex + 1, lines = lines.Length };
        Layout = layout;
        DoorController.AnyOpen = false;

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

                if (tileChar != ' ')
                {
                    layout.minZ = Mathf.Min(layout.minZ, z);
                    layout.maxZ = Mathf.Max(layout.maxZ, z);
                }

                if (tileChar != ' ' && tileChar != 'T')
                {
                    Instantiate(tilePrefab, floorPos, Quaternion.identity, transform);
                }

                switch (tileChar)
                {
                    case '.': layout.floor.Add(objectPos); break;

                    case '#': Instantiate(wallPrefab, wallPos, Quaternion.identity, transform); break;
                    case 'D':
                        Instantiate(doorPrefab, wallPos, Quaternion.identity, transform);
                        layout.doors.Add(objectPos);
                        break;
                    case 'X':
                        Instantiate(exitTriggerPrefab, objectPos, Quaternion.identity, transform);
                        layout.exits.Add(objectPos);
                        break;

                    case 'S':
                        layout.start = objectPos;
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

        SpawnReinforcements(layout);

        List<Transform> spawned = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf) continue;
            spawned.Add(child);
            AddEnemyAnimator(child.gameObject);
        }

        Backdrop.ApplyTheme(layout.room);
        RoomDressing.Dress(layout, transform, coinPrefab);
        RoomClearBonus.Create(transform);
        MusicDirector.PlayRoom(layout.room);

        // Estampita del botijo: cada sala empieza con un abanico
        if (Estampitas.Has(Estampitas.Id.Botijo) && PlayerController.Instance != null)
        {
            PlayerPowerUps pu = PlayerController.Instance.GetComponent<PlayerPowerUps>();
            if (pu != null) pu.GiveShield();
        }

        if (assembleCoroutine != null) StopCoroutine(assembleCoroutine);
        // Con "movimiento reducido" la sala aparece directamente, sin montaje animado
        if (!GameSettings.ReduceMotion) assembleCoroutine = StartCoroutine(AssembleRoom(spawned, layout.start));
        else assembleCoroutine = null;
    }

    // Refuerzos: a partir de la sala 3 puede colarse algún enemigo más (de los que ya hay en la sala),
    // así ninguna partida es igual. En Turista no hay; en Ibérico siempre.
    private void SpawnReinforcements(RoomLayout layout)
    {
        if (layout.room < 3 || layout.room >= mapFiles.Length) return;
        int difficulty = GameSettings.Difficulty;
        if (difficulty == 0) return;
        int extra = difficulty == 2 ? 1 + (Random.value < 0.4f ? 1 : 0) : (Random.value < 0.45f ? 1 : 0);
        if (extra == 0) return;

        List<GameObject> templates = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf) continue;
            if (child.GetComponent<EnemyPatrol>() != null || child.GetComponent<Enemigo_basico>() != null || child.GetComponent<Guiri>() != null)
                templates.Add(child.gameObject);
        }
        if (templates.Count == 0) return;

        List<Vector3> cells = layout.floor.FindAll(p => p.x >= layout.start.x + 3f && p.x < layout.lines - 2.5f && layout.FarFromKeyCells(p, 2f));
        foreach (GameObject t in templates)
            cells.RemoveAll(p => Mathf.Abs(p.x - t.transform.position.x) + Mathf.Abs(p.z - t.transform.position.z) < 2f);

        for (int i = 0; i < extra && cells.Count > 0; i++)
        {
            int k = Random.Range(0, cells.Count);
            Vector3 cell = cells[k];
            cells.RemoveAt(k);
            layout.floor.Remove(cell);
            GameObject template = templates[Random.Range(0, templates.Count)];
            GameObject e = Instantiate(template, cell, template.transform.rotation, transform);
            e.name = template.name;
        }
    }

    private static void AddEnemyAnimator(GameObject go)
    {
        if (go.GetComponent<JuiceAnimator>() != null) return;
        if (go.GetComponent<EnemyPatrol>() != null)
        {
            JuiceAnimator a = go.AddComponent<JuiceAnimator>();
            a.hopHeight = 0.12f;
            a.waddle = 9f;
            a.dustOnLand = true;
            a.dustColor = new Color(0.8f, 0.7f, 0.5f);
        }
        else if (go.GetComponent<Enemigo_basico>() != null || go.GetComponent<Guiri>() != null)
        {
            JuiceAnimator a = go.AddComponent<JuiceAnimator>();
            a.hopHeight = 0.18f;
            a.waddle = 10f;
        }
        else if (go.GetComponent<Pedro>() != null)
        {
            JuiceAnimator a = go.AddComponent<JuiceAnimator>();
            a.hopHeight = 0.1f;
            a.waddle = 4f;
            a.breathe = 0.04f;
        }
    }

    // Flechas verdes sobre la salida cuando la puerta se abre
    public void ShowExitArrows()
    {
        if (Layout == null) return;
        foreach (Vector3 exit in Layout.exits) ExitArrow.Create(transform, exit);
    }

    // La sala se monta sola: el suelo emerge desde el jugador hacia fuera,
    // las paredes caen del cielo y el resto aparece con un saltito.
    private IEnumerator AssembleRoom(List<Transform> items, Vector3 origin)
    {
        InputLocked = true;
        const float pieceTime = 0.3f;

        // Los enemigos y radares esperan a que termine el montaje
        List<Behaviour> paused = new List<Behaviour>();
        foreach (Transform t in items)
        {
            foreach (Behaviour b in t.GetComponents<Behaviour>())
            {
                if ((b is EnemyPatrol || b is Enemigo_basico || b is Guiri || b is ArrowTrap) && b.enabled)
                {
                    b.enabled = false;
                    paused.Add(b);
                }
            }
        }

        int n = items.Count;
        Vector3[] pos = new Vector3[n];
        Vector3[] scale = new Vector3[n];
        float[] delay = new float[n];
        int[] mode = new int[n]; // 0 = suelo, 1 = pared que cae, 2 = aparece
        float total = 0f;
        for (int i = 0; i < n; i++)
        {
            Transform t = items[i];
            pos[i] = t.position;
            scale[i] = t.localScale;
            float d = Vector2.Distance(new Vector2(t.position.x, t.position.z), new Vector2(origin.x, origin.z));
            delay[i] = d * 0.035f + Random.Range(0f, 0.04f);
            mode[i] = t.position.y <= 0.05f ? 0 : t.position.y >= 0.9f ? 1 : 2;
            if (mode[i] == 1) delay[i] += 0.08f;
            if (mode[i] == 2) delay[i] += 0.15f;
            total = Mathf.Max(total, delay[i] + pieceTime);
            Apply(t, pos[i], scale[i], mode[i], 0f);
        }

        float time = 0f;
        while (time < total)
        {
            time += Time.deltaTime;
            if (time > 0.3f) InputLocked = false;
            for (int i = 0; i < n; i++)
            {
                if (items[i] == null) continue;
                float k = Mathf.Clamp01((time - delay[i]) / pieceTime);
                Apply(items[i], pos[i], scale[i], mode[i], k);
            }
            yield return null;
        }

        for (int i = 0; i < n; i++)
        {
            if (items[i] == null) continue;
            if (mode[i] != 2) items[i].position = pos[i];
            items[i].localScale = scale[i];
        }
        foreach (Behaviour b in paused) if (b != null) b.enabled = true;
        InputLocked = false;
        assembleCoroutine = null;
    }

    private static void Apply(Transform t, Vector3 pos, Vector3 scale, int mode, float k)
    {
        float e = EaseOutBack(k);
        switch (mode)
        {
            case 0:
                // Nunca baja más de 0.25 para que el suelo siga detectándose
                t.position = pos + Vector3.down * (1f - Mathf.Clamp01(e)) * 0.25f;
                t.localScale = Vector3.Scale(scale, new Vector3(Mathf.Lerp(0.4f, 1f, e), Mathf.Lerp(0.3f, 1f, e), Mathf.Lerp(0.4f, 1f, e)));
                break;
            case 1:
                float fall = 1f - k * k;
                t.position = pos + Vector3.up * fall * 3f;
                t.localScale = scale;
                break;
            default:
                // Solo escala: los que se mueven solos (Pedro) no deben quedarse enganchados
                t.localScale = scale * Mathf.Max(0.001f, e);
                break;
        }
    }

    private static float EaseOutBack(float x)
    {
        if (x <= 0f) return 0f;
        if (x >= 1f) return 1f;
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }

    private IEnumerator CrumbleFloorRoutine(int roomNumber)
    {
        // Sin derrumbe en las dos primeras salas ni en la del jefe
        if (roomNumber < 3 || roomNumber == mapFiles.Length) yield break;

        yield return new WaitForSeconds(timeBeforeFalling + Estampitas.CrumbleDelayBonus);
        float fallSpeed = this.fallSpeed * Estampitas.CrumbleSlowdown;

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