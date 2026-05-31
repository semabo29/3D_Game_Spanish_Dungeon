using UnityEngine;

public class ReiEmerito : MonoBehaviour
{
    [Header("Movimiento Inteligente")]
    public float moveSpeed = 2f;
    public float gridSize = 1f;
    public float waitTimePerTile = 1.5f; // Ritmo al que avanza por las baldosas

    [Header("Lógica de Huida")]
    public float fleeDuration = 3f; // Segundos que pasará huyendo tras interactuar
    private float fleeTimer = 0f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private float waitTimer = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip SonidoAtaque;

    private ReiStats stats;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        stats = GetComponent<ReiStats>(); // Obtenemos las estadísticas heredadas
    }

    void Update()
    {
        if (stats != null && stats.IsDead()) return; // Si está muerto, bloqueamos todo

        // Reducimos el contador de huida frame a frame
        if (fleeTimer > 0f)
        {
            fleeTimer -= Time.deltaTime;
        }

        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                // Si está huyendo, NO se detiene a atacar, prioriza ganar distancia
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
            // Movimiento suave hacia la casilla objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                waitTimer = waitTimePerTile;
            }
        }

        // Solo comprueba colisiones físicas reactivas si no está en cooldown de huida
        if (fleeTimer <= 0f)
        {
            CheckPlayerCollision();
        }
    }

    // PERSECUCIÓN O HUIDA CON EVASIÓN DE OBSTÁCULOS
    private void CalculateNextStep()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        Vector3 diff = player.transform.position - transform.position;
        Vector3 stepDir = Vector3.zero;

        // Si el temporizador de huida está activo, invertimos la dirección (-1).
        bool isFleeing = fleeTimer > 0f;
        float directionModifier = isFleeing ? -1f : 1f;

        // Comparamos qué distancia es mayor para acortar camino (o alargarlo si huye)
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
        {
            stepDir = new Vector3(Mathf.Sign(diff.x) * directionModifier, 0, 0); // Eje X
            if (!IsPathClear(stepDir))
            {
                stepDir = new Vector3(0, 0, Mathf.Sign(diff.z) * directionModifier); // Esquiva por Eje Z
                if (!IsPathClear(stepDir)) stepDir = Vector3.zero;
            }
        }
        else
        {
            stepDir = new Vector3(0, 0, Mathf.Sign(diff.z) * directionModifier); // Eje Z
            if (!IsPathClear(stepDir))
            {
                stepDir = new Vector3(Mathf.Sign(diff.x) * directionModifier, 0, 0); // Esquiva por Eje X
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
            waitTimer = waitTimePerTile; // Se queda quieto si está completamente acorralado
        }
    }

    private bool IsPathClear(Vector3 dir)
    {
        if (dir == Vector3.zero) return false;

        Vector3 nextPos = transform.position + (dir * gridSize);

        // COMPROBACIÓN DE PAREDES
        Vector3 tamañoCajaBusqueda = new Vector3(0.9f, 0.5f, 0.9f);
        Collider[] objetosDetectados = Physics.OverlapBox(nextPos + Vector3.up * 0.5f, tamañoCajaBusqueda, Quaternion.identity);

        foreach (Collider col in objetosDetectados)
        {
            if (col.gameObject != gameObject && !col.isTrigger && col.GetComponent<PlayerController>() == null)
            {
                return false;
            }
        }

        // COMPROBACIÓN DE SUELO
        Vector3 posicionSuelo = nextPos + Vector3.down * 0.5f;
        Vector3 tamañoCajaSuelo = new Vector3(0.4f, 0.1f, 0.4f);
        Collider[] sueloDetectado = Physics.OverlapBox(posicionSuelo, tamañoCajaSuelo, Quaternion.identity);

        bool haySuelo = false;
        foreach (Collider col in sueloDetectado)
        {
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                haySuelo = true;
                break;
            }
        }

        return haySuelo;
    }

    // ATAQUE AL JUGADOR
    private bool CheckPlayerCollision()
    {
        if (fleeTimer > 0f) return false; // Si está huyendo, el jefe no ataca de vuelta

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            if (distanceX < 1.2f && distanceZ < 1.2f)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null) playerStats.TakeDamage();

                if (audioSource != null && SonidoAtaque != null)
                {
                    audioSource.PlayOneShot(SonidoAtaque, 0.3f);
                }

                fleeTimer = fleeDuration; // ¡Activa la huida tras golpear con éxito!

                // NUEVO: Al golpear al jugador, también se vuelve inmune y empieza a parpadear
                if (stats != null) stats.StartInvincibility();

                return true;
            }
        }
        return false;
    }

    // --- ENLACE CON SISTEMA DE VIDAS ---
    public void RecibirDano()
    {
        if (stats == null) return;
        if (stats.IsInvincible()) return; // Si está en sus frames de parpadeo inmune, ignora el golpe

        stats.TakeDamage(); // Esto le resta vida y dispara el parpadeo de forma interna
        fleeTimer = fleeDuration; // ¡Activa la huida inmediatamente al ser golpeado!
    }
}