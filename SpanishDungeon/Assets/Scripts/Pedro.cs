using UnityEngine;

public class Pedro : MonoBehaviour
{
    [Header("Movimiento Inteligente (1x1)")]
    public float moveSpeed = 5f;
    public float gridSize = 1f;
    public float waitTimePerTile = 0.5f;

    [Header("Lógica de Huida")]
    public float fleeDuration = 3f;
    private float fleeTimer = 0f;

    [HideInInspector] public Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private float waitTimer = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip SonidoAtaque;
    [SerializeField] private AudioClip SonidoRisa;

    private PedroStats stats;

    [Header("Fase 2 (última vida)")]
    public float phase2SpeedMultiplier = 1.4f;
    public float phase2WaitMultiplier = 0.55f;
    private bool phase2 = false;
    private float baseMoveSpeed;
    private float baseWaitTime;

    private static readonly string[] HitTaunts = { "¡No me consta!", "¡Es un bulo!", "¡Fake news!", "¡Convoco elecciones!", "¡Eso es de la oposición!" };
    private static readonly string[] AttackTaunts = { "¡Subida de impuestos!", "¡Recorte para ti!", "¡Decreto ley!", "¡A la cola del paro!" };

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        stats = GetComponent<PedroStats>();
        baseMoveSpeed = moveSpeed;
        baseWaitTime = waitTimePerTile;
        if (audioSource != null && SonidoRisa != null)
        {
            audioSource.PlayOneShot(SonidoRisa, 2.0f);
        }
    }

    void Update()
    {
        if (stats != null && stats.IsDead()) return;

        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.deltaTime;
        }

        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                if (fleeTimer <= 0f && CheckPlayerCollision())
                {
                    waitTimer = waitTimePerTile;
                }
                else
                {
                    CalculateNextStep();
                }
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                waitTimer = waitTimePerTile;
            }
        }

        if (fleeTimer <= 0f)
        {
            CheckPlayerCollision();
        }
    }

    private void CalculateNextStep()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        Vector3 diff = player.targetPosition - transform.position;
        Vector3 stepDir = Vector3.zero;

        bool isFleeing = fleeTimer > 0f;
        float directionModifier = isFleeing ? -1f : 1f;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
        {
            stepDir = new Vector3(Mathf.Sign(diff.x) * directionModifier, 0, 0);
            if (!IsPathClear(stepDir))
            {
                stepDir = new Vector3(0, 0, Mathf.Sign(diff.z) * directionModifier);
                if (!IsPathClear(stepDir)) stepDir = Vector3.zero;
            }
        }
        else
        {
            stepDir = new Vector3(0, 0, Mathf.Sign(diff.z) * directionModifier);
            if (!IsPathClear(stepDir))
            {
                stepDir = new Vector3(Mathf.Sign(diff.x) * directionModifier, 0, 0);
                if (!IsPathClear(stepDir)) stepDir = Vector3.zero;
            }
        }

        if (stepDir != Vector3.zero)
        {
            targetPosition = transform.position + (stepDir * gridSize);
            targetRotation = Quaternion.LookRotation(stepDir);
            isMoving = true;
        }
        else
        {
            waitTimer = waitTimePerTile; 
        }
    }

    private bool IsPathClear(Vector3 dir)
    {
        if (dir == Vector3.zero) return false;

        Vector3 nextPos = transform.position + (dir * gridSize);

        Vector3 boxSize = new Vector3(0.4f, 0.4f, 0.4f);
        Collider[] objects = Physics.OverlapBox(nextPos + Vector3.up * 0.5f, boxSize, Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        foreach (Collider col in objects)
        {
            if (col.gameObject != gameObject && !col.isTrigger && col.GetComponent<PlayerController>() == null)
            {
                return false;
            }
        }

        Vector3 floorPos = nextPos + Vector3.down * 0.5f;
        Collider[] floorObjects = Physics.OverlapBox(floorPos, new Vector3(0.3f, 0.1f, 0.3f), Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        bool hasFloor = false;
        foreach (Collider col in floorObjects)
        {
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                hasFloor = true;
                break;
            }
        }

        return hasFloor;
    }

    private bool CheckPlayerCollision()
    {
        if (fleeTimer > 0f) return false;

        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            float distanceX = Mathf.Abs(targetPosition.x - player.targetPosition.x);
            float distanceZ = Mathf.Abs(targetPosition.z - player.targetPosition.z);

            if (distanceX < 0.1f && distanceZ < 0.1f)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null) playerStats.TakeDamage("víctima de un decreto ley");

                if (audioSource != null && SonidoAtaque != null)
                {
                    audioSource.PlayOneShot(SonidoAtaque, 2.0f);
                }
                GameFeel.FloatingText(transform.position + Vector3.right * 0.6f, AttackTaunts[Random.Range(0, AttackTaunts.Length)], new Color(1f, 0.4f, 0.35f), 4f);

                GatillarHuidaEInmunidad();
                return true;
            }
        }
        return false;
    }

    public void RecibirDano()
    {
        if (stats == null || stats.IsInvincible()) return;

        stats.TakeDamage();

        GameFeel.VoxelBurst(transform.position, new Color(0.12f, 0.14f, 0.25f), 10, 3.5f);
        GameFeel.Shake(0.2f, 0.2f);
        GameFeel.Hitstop(0.08f);

        if (stats.IsDead()) return;

        GameFeel.FloatingText(transform.position, HitTaunts[Random.Range(0, HitTaunts.Length)], Color.white, 4.5f);

        if (!phase2 && stats.IsLastLife)
        {
            EnterPhase2();
        }

        GatillarHuidaEInmunidad();
    }

    private void EnterPhase2()
    {
        phase2 = true;
        MusicDirector.SetIntensity(true);
        moveSpeed = baseMoveSpeed * phase2SpeedMultiplier;
        waitTimePerTile = baseWaitTime * phase2WaitMultiplier;
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.ShowToast("¡Pedro se atrinchera en el escaño! (más rápido)", new Color(1f, 0.3f, 0.25f), 1.8f);
        }
        if (audioSource != null && SonidoRisa != null)
        {
            audioSource.PlayOneShot(SonidoRisa, 2.0f);
        }
    }

    private void GatillarHuidaEInmunidad()
    {
        fleeTimer = fleeDuration;
        if (stats != null) stats.StartInvincibility();
    }
}