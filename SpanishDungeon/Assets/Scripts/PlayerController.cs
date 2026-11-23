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

    [Header("Control")]
    [Tooltip("Tiempo durante el que se recuerda una tecla pulsada a mitad de un paso")]
    [SerializeField] private float inputBufferTime = 0.2f;
    private Vector3 bufferedDirection;
    private float bufferedAt = -10f;
    private Vector3 lastAxisDirection;

    [Header("Combo")]
    [SerializeField] private float comboWindow = 2f;
    private int comboCount = 0;
    private float lastKillTime = -10f;

    private static readonly string[] KillShouts = { "¡ZAS!", "¡PUM!", "¡OLÉ!", "¡TOMA!", "¡HALA!", "¡ZASCA!", "¡CATAPLÁN!", "¡A LA SACA!", "¡OSTRAS!" };
    private static readonly string[] ToroShouts = { "¡Al corral!", "¡Torero!", "¡Olé, olé!" };
    private static readonly string[] CarteristaShouts = { "¡Devuelve eso!", "¡Al calabozo!", "¡Mi móvil!" };
    private static readonly string[] GuiriShouts = { "¡Bye bye!", "¡Sangría time over!", "¡Go home!" };

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stepSound;
    [SerializeField] private AudioClip SonidoAtaque;

    private JuiceAnimator animator;
    private float lastWallBump = -10f;
    private PlayerPowerUps powerUps;

    public int StepsTaken { get; private set; }
    public bool IsDrunk => isDrunk;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();

        animator = gameObject.AddComponent<JuiceAnimator>();
        animator.dustOnLand = true;
        animator.hopHeight = 0.2f;
        animator.waddle = 5f;

        powerUps = GetComponent<PlayerPowerUps>();
        if (powerUps == null) powerUps = gameObject.AddComponent<PlayerPowerUps>();
        if (GetComponent<HatWearer>() == null) gameObject.AddComponent<HatWearer>();
    }

    private bool InputBlocked => MapGenerator.InputLocked || ModalMenu.IsOpen || Time.timeScale == 0f;

    void Update()
    {
        if (isFalling) return;

        if (isDrunk)
        {
            drunkTimer -= Time.deltaTime;
            if (drunkTimer <= 0f)
            {
                isDrunk = false;
                if (GameHUD.Instance != null) GameHUD.Instance.ShowToast("Se te pasó la borrachera", new Color(0.7f, 0.9f, 1f), 0.9f);
            }
        }

        // Recordamos la tecla pulsada durante un paso para encadenar movimientos fluidos
        Vector3 pressed = ReadPressedDirection();
        if (pressed != Vector3.zero)
        {
            bufferedDirection = pressed;
            bufferedAt = Time.time;
        }

        if (!isMoving)
        {
            if (!MapGenerator.InputLocked) CheckFloorBeneath();

            if (!isFalling && !InputBlocked)
            {
                Vector3 dir = Vector3.zero;
                if (Time.time - bufferedAt <= inputBufferTime) dir = bufferedDirection;
                else if (GameSettings.HoldToMove) dir = ReadHeldDirection();

                if (dir != Vector3.zero)
                {
                    bufferedAt = -10f;
                    TryMove(dir);
                }
            }
        }
        else
        {
            float speed = moveSpeed * (powerUps != null ? powerUps.SpeedMultiplier : 1f) * Estampitas.SpeedMultiplier;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

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

    // ---------- Entrada ----------

    // W/arriba = +X, S/abajo = -X, A/izquierda = +Z, D/derecha = -Z (como en la versión original)
    private Vector3 ReadPressedDirection()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) return Vector3.right;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) return Vector3.left;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) return Vector3.forward;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) return Vector3.back;

        // Mando: detectamos el "flanco" del stick o la cruceta
        Vector3 axis = AxisDirection();
        Vector3 result = (axis != Vector3.zero && axis != lastAxisDirection) ? axis : Vector3.zero;
        lastAxisDirection = axis;
        return result;
    }

    private Vector3 ReadHeldDirection()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) return Vector3.right;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) return Vector3.left;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) return Vector3.forward;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return Vector3.back;
        return AxisDirection();
    }

    private static Vector3 AxisDirection()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(h) < 0.5f && Mathf.Abs(v) < 0.5f) return Vector3.zero;
        if (Mathf.Abs(v) >= Mathf.Abs(h)) return v > 0f ? Vector3.right : Vector3.left;
        return h > 0f ? Vector3.back : Vector3.forward;
    }

    // ---------- Movimiento ----------

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

    private void TryMove(Vector3 direction)
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
                // Choque contra la pared: pequeño empujón visual
                transform.rotation = targetRotation;
                if (animator != null && Time.time - lastWallBump > 0.3f)
                {
                    lastWallBump = Time.time;
                    animator.Lunge(direction, 0.12f);
                }
                return;
            }
        }

        GameObject enemyInTarget = GetEnemyAtPosition(proposedPosition);

        if (enemyInTarget != null)
        {
            Pedro pedro = enemyInTarget.GetComponent<Pedro>();

            // Nos giramos hacia el enemigo aunque no nos movamos
            transform.rotation = targetRotation;
            if (animator != null) animator.Lunge(direction);

            if (pedro != null)
            {
                pedro.RecibirDano();
            }
            else
            {
                KillEnemy(enemyInTarget, direction);
            }

            if (audioSource != null && SonidoAtaque != null)
            {
                audioSource.PlayOneShot(SonidoAtaque);
            }
            return;
        }

        // Macetas, botijos y cajas: se rompen de un golpe
        Breakable breakable = Breakable.At(proposedPosition);
        if (breakable != null)
        {
            transform.rotation = targetRotation;
            if (animator != null) animator.Lunge(direction);
            breakable.Smash(direction, true);
            return;
        }

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
            StepsTaken++;
            if (audioSource != null && stepSound != null)
            {
                audioSource.PlayOneShot(stepSound);
            }
        }
    }

    private GameObject GetEnemyAtPosition(Vector3 targetPos)
    {
        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (EnemyPatrol enemy in enemies)
        {
            if (SameTile(enemy.transform.position, targetPos)) return enemy.gameObject;
        }

        Enemigo_basico[] basicos = FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None);
        foreach (Enemigo_basico basico in basicos)
        {
            if (SameTile(basico.transform.position, targetPos)) return basico.gameObject;
        }

        Guiri[] guiris = FindObjectsByType<Guiri>(FindObjectsSortMode.None);
        foreach (Guiri guiri in guiris)
        {
            if (SameTile(guiri.transform.position, targetPos)) return guiri.gameObject;
        }

        Pedro[] pedros = FindObjectsByType<Pedro>(FindObjectsSortMode.None);
        foreach (Pedro pedro in pedros)
        {
            if (SameTile(pedro.transform.position, targetPos)) return pedro.gameObject;
        }

        return null;
    }

    private static bool SameTile(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) < 0.1f && Mathf.Abs(a.z - b.z) < 0.1f;
    }

    // ---------- Combate ----------

    private void KillEnemy(GameObject enemy, Vector3 direction)
    {
        Vector3 pos = enemy.transform.position;
        Color color = EnemyColor(enemy);
        string shout = ShoutFor(enemy);

        Achievements.OnKill(enemy);

        // El enemigo sale volando entero y además suelta vóxeles
        GameFeel.Ragdoll(enemy, direction, 6.5f);
        Destroy(enemy);

        RunStats.Kills++;

        if (Time.time - lastKillTime <= comboWindow * Estampitas.ComboWindowMultiplier) comboCount++;
        else comboCount = 1;
        lastKillTime = Time.time;
        if (comboCount > RunStats.BestCombo) RunStats.BestCombo = comboCount;
        Achievements.OnCombo(comboCount);
        if (GameHUD.Instance != null) GameHUD.Instance.SetCombo(comboCount, comboWindow * Estampitas.ComboWindowMultiplier);

        GameFeel.VoxelBurst(pos, color, 12, 4.5f);
        GameFeel.Shake(0.12f, 0.15f);
        GameFeel.Hitstop(0.05f);

        if (comboCount >= 2)
        {
            string label = comboCount == 2 ? "¡DOBLE!" : comboCount == 3 ? "¡TRIPLE!" : comboCount == 4 ? "¡CUÁDRUPLE!" : "¡COMBO x" + comboCount + "!";
            GameFeel.FloatingText(pos, label, new Color(1f, 0.55f, 0.1f), 5f + Mathf.Min(comboCount, 6) * 0.3f);
            Sfx.Play(SfxType.Combo, 0.8f, 1f + (comboCount - 2) * 0.08f);
            // Premio por combo: una moneda extra
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddCoin();
                if (Estampitas.Has(Estampitas.Id.Rosario)) stats.AddCoin();
            }
        }
        else
        {
            GameFeel.FloatingText(pos, shout, Color.white, 4.5f);
        }
    }

    private static string ShoutFor(GameObject enemy)
    {
        string[] pool = KillShouts;
        if (Random.value < 0.45f)
        {
            if (enemy.GetComponent<EnemyPatrol>() != null) pool = ToroShouts;
            else if (enemy.GetComponent<Enemigo_basico>() != null) pool = CarteristaShouts;
            else if (enemy.GetComponent<Guiri>() != null) pool = GuiriShouts;
        }
        return pool[Random.Range(0, pool.Length)];
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
            Achievements.OnDrunk();
        }
        isDrunk = true;
        drunkTimer = duracion;
    }

    // El agua del botijo quita la borrachera
    public void Sober()
    {
        isDrunk = false;
        drunkTimer = 0f;
    }
}
