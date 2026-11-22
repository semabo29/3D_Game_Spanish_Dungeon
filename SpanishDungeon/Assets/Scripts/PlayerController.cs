using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float moveSpeed = 5f;
    public float gridSize = 1f;

    [HideInInspector] public Vector3 targetPosition;
    private Quaternion targetRotation; 
    private bool isMoving = false;
    private bool isFalling = false; 
    private Rigidbody rb;

    private bool isDrunk = false;
    private float drunkTimer = 0f;

    [Header("Combo")]
    [SerializeField] private float comboWindow = 2f;
    private int comboCount = 0;
    private float lastKillTime = -10f;

    private static readonly string[] KillShouts = { "¡ZAS!", "¡PUM!", "¡OLÉ!", "¡TOMA!", "¡HALA!", "¡ZASCA!" };

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stepSound;
    [SerializeField] private AudioClip SonidoAtaque;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool IsDrunk => isDrunk;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        rb = GetComponent<Rigidbody>(); 
    }

    void Update()
    {
        if (isFalling) return; 

        if (isDrunk) 
        {
            drunkTimer -= Time.deltaTime;
            if (drunkTimer <= 0f)
            {
                isDrunk = false;
                Debug.Log("¡Ya se te ha pasado el efecto de la cerveza! Controles normales.");
            }
        }

        if (!isMoving)
        {
            CheckFloorBeneath();

            if (!isFalling) 
            {
                CheckInput();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
            }

            bool reachedPosition = Vector3.Distance(transform.position, targetPosition) < 0.01f;
            bool reachedRotation = Quaternion.Angle(transform.rotation, targetRotation) < 0.1f;

            if (reachedPosition && reachedRotation)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation; 
                isMoving = false;
            }
        }
    }

    private void CheckFloorBeneath()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 2f);
        bool floorFound = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
            {
                floorFound = true;
                break;
            }
        }

        if (!floorFound)
        {
            isFalling = true;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true; 
            }

            GetComponent<PlayerStats>().FallToDeath();
        }
    }

    private bool esObstaculoSolido(RaycastHit hit)
    {
        return !hit.collider.isTrigger &&
               hit.collider.GetComponent<EnemyPatrol>() == null &&
               hit.collider.GetComponent<Enemigo_basico>() == null &&
               hit.collider.GetComponent<Guiri>() == null &&
               hit.collider.GetComponent<Pedro>() == null;
    }

    private void CheckInput()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector3.back;

        if (direction != Vector3.zero)
        {

            if (isDrunk)
            {
                direction = -direction;
            }

            Vector3 proposedPosition = transform.position + (direction * gridSize);

            targetRotation = Quaternion.LookRotation(direction);

            if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), direction, out RaycastHit hit, gridSize))
            {
                if (esObstaculoSolido(hit))
                {
                    return; 
                }
            }

            GameObject enemyInTarget = GetEnemyAtPosition(proposedPosition);

            if (enemyInTarget != null)
            {
                Pedro pedro = enemyInTarget.GetComponent<Pedro>();

                // Nos giramos hacia el enemigo aunque no nos movamos
                transform.rotation = targetRotation;

                if (pedro != null)
                {
                    pedro.RecibirDano();
                }
                else
                {
                    KillEnemy(enemyInTarget);
                }

                if (audioSource != null && SonidoAtaque != null)
                {
                    audioSource.PlayOneShot(SonidoAtaque);
                }
            }
            else
            {
                RaycastHit[] floorHits = Physics.RaycastAll(proposedPosition + Vector3.up, Vector3.down, 2f);
                bool targetFloorFound = false;

                foreach (RaycastHit fHit in floorHits)
                {
                    if (fHit.collider.gameObject != gameObject && !fHit.collider.isTrigger)
                    {
                        targetFloorFound = true;
                        break;
                    }
                }

                if (targetFloorFound)
                {
                    targetPosition = proposedPosition;
                    isMoving = true;
                    if (audioSource != null && stepSound != null)
                    {
                        audioSource.PlayOneShot(stepSound);
                    }
                }
            }
        }
    }

    private GameObject GetEnemyAtPosition(Vector3 targetPos)
    {
        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (EnemyPatrol enemy in enemies)
        {
            float distanceX = Mathf.Abs(enemy.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(enemy.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return enemy.gameObject;
        }

        Enemigo_basico[] basicos = FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None);
        foreach (Enemigo_basico basico in basicos)
        {
            float distanceX = Mathf.Abs(basico.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(basico.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return basico.gameObject;
        }

        Guiri[] guiris = FindObjectsByType<Guiri>(FindObjectsSortMode.None);
        foreach (Guiri guiri in guiris)
        {
            float distanceX = Mathf.Abs(guiri.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(guiri.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return guiri.gameObject;
        }

        Pedro[] pedros = FindObjectsByType<Pedro>(FindObjectsSortMode.None);
        foreach (Pedro pedro in pedros)
        {
            float distanceX = Mathf.Abs(pedro.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(pedro.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return pedro.gameObject;
        }

        return null;
    }

    private void KillEnemy(GameObject enemy)
    {
        Vector3 pos = enemy.transform.position;
        Color color = EnemyColor(enemy);
        Destroy(enemy);

        RunStats.Kills++;

        if (Time.time - lastKillTime <= comboWindow) comboCount++;
        else comboCount = 1;
        lastKillTime = Time.time;
        if (comboCount > RunStats.BestCombo) RunStats.BestCombo = comboCount;

        GameFeel.VoxelBurst(pos, color, 16, 4.5f);
        GameFeel.Shake(0.12f, 0.15f);
        GameFeel.Hitstop(0.05f);

        if (comboCount >= 2)
        {
            string label = comboCount == 2 ? "¡DOBLE!" : comboCount == 3 ? "¡TRIPLE!" : "¡COMBO x" + comboCount + "!";
            GameFeel.FloatingText(pos, label, new Color(1f, 0.55f, 0.1f), 5f);
            // Premio por combo: una moneda extra
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null) stats.AddCoin();
        }
        else
        {
            GameFeel.FloatingText(pos, KillShouts[Random.Range(0, KillShouts.Length)], Color.white, 4.5f);
        }
    }

    private static Color EnemyColor(GameObject enemy)
    {
        if (enemy.GetComponent<EnemyPatrol>() != null) return new Color(0.45f, 0.25f, 0.12f);   // Toro
        if (enemy.GetComponent<Enemigo_basico>() != null) return new Color(0.15f, 0.18f, 0.4f); // Carterista (chándal)
        if (enemy.GetComponent<Guiri>() != null) return new Color(1f, 0.55f, 0.7f);             // Guiri (flotador rosa)
        return Color.gray;
    }

    public void Enborrachar(float duracion)
    {
        if (!isDrunk)
        {
            GameFeel.FloatingText(transform.position, "¡HIP!", new Color(1f, 0.85f, 0.2f), 5f);
            if (GameHUD.Instance != null) GameHUD.Instance.ShowToast("¡Borracho! Controles invertidos", new Color(1f, 0.85f, 0.2f), 1.2f);
        }
        isDrunk = true;
        drunkTimer = duracion;
        Debug.Log("¡Te has emborrachado! Controles invertidos durante " + duracion + " segundos.");
    }
}