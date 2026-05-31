using UnityEngine;

public class Pedro : MonoBehaviour
{
    [Header("Movimiento Inteligente (1x1)")]
    public float moveSpeed = 2.5f;
    public float gridSize = 1f;
    public float waitTimePerTile = 1.2f;

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

    private PedroStats stats;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        stats = GetComponent<PedroStats>();
    }

    void Update()
    {
        if (stats != null && stats.IsDead()) return;

        // Gestión del temporizador de huida
        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.deltaTime;
        }

        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                // Si no está huyendo, comprueba si tiene al jugador en su propia casilla destino para atacarle
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

        // Comprobación reactiva constante en su posición actual si no está huyendo
        if (fleeTimer <= 0f)
        {
            CheckPlayerCollision();
        }
    }

    private void CalculateNextStep()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        // Comparamos usando la posición lógica de cuadrícula del jugador
        Vector3 diff = player.targetPosition - transform.position;
        Vector3 stepDir = Vector3.zero;

        bool isFleeing = fleeTimer > 0f;
        float directionModifier = isFleeing ? -1f : 1f;

        // Elige el eje con mayor distancia para acortar camino (o alargarlo al huir)
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
            waitTimer = waitTimePerTile; // Se bloquea temporalmente si no hay salida válida
        }
    }

    private bool IsPathClear(Vector3 dir)
    {
        if (dir == Vector3.zero) return false;

        Vector3 nextPos = transform.position + (dir * gridSize);

        // COMPROBACIÓN DE OBSTÁCULOS EXACTA 1x1
        Vector3 boxSize = new Vector3(0.4f, 0.4f, 0.4f);
        Collider[] objects = Physics.OverlapBox(nextPos + Vector3.up * 0.5f, boxSize, Quaternion.identity);

        foreach (Collider col in objects)
        {
            // Muros y objetos sólidos bloquean. Ignoramos triggers y al Player
            if (col.gameObject != gameObject && !col.isTrigger && col.GetComponent<PlayerController>() == null)
            {
                return false;
            }
        }

        // COMPROBACIÓN DE SUELO BAJO LA CASILLA
        Vector3 floorPos = nextPos + Vector3.down * 0.5f;
        Collider[] floorObjects = Physics.OverlapBox(floorPos, new Vector3(0.3f, 0.1f, 0.3f), Quaternion.identity);

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

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            // Al ser 1x1, evaluamos si ocupan de forma efectiva la misma casilla lógica
            float distanceX = Mathf.Abs(targetPosition.x - player.targetPosition.x);
            float distanceZ = Mathf.Abs(targetPosition.z - player.targetPosition.z);

            if (distanceX < 0.1f && distanceZ < 0.1f)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null) playerStats.TakeDamage();

                if (audioSource != null && SonidoAtaque != null)
                {
                    audioSource.PlayOneShot(SonidoAtaque, 0.3f);
                }

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
        GatillarHuidaEInmunidad();
    }

    private void GatillarHuidaEInmunidad()
    {
        fleeTimer = fleeDuration;
        if (stats != null) stats.StartInvincibility();
    }
}