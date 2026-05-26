using UnityEngine;

public class Enemigo_basico : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 5f;
    public float gridSize = 1f;
    public int chaseRange = 2; // Rango para empezar a perseguir
    public float waitTimePerTile = 0.5f;
    public int coinsToSteal = 3;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private float waitTimer = 0f;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                CheckAndChasePlayer();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
                isMoving = false;
                waitTimer = waitTimePerTile;
            }
        }

        CheckPlayerCollision();
    }

    private void CheckAndChasePlayer()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            float gridDistX = Mathf.Abs(player.transform.position.x - transform.position.x) / gridSize;
            float gridDistZ = Mathf.Abs(player.transform.position.z - transform.position.z) / gridSize;

            // Si está dentro del rango de persecución
            if (gridDistX <= chaseRange && gridDistZ <= chaseRange)
            {
                Vector3 bestDir = Vector3.zero;
                float minDistance = float.MaxValue;
                float currentDistance = Vector3.Distance(transform.position, player.transform.position);

                // Comprobamos las 4 direcciones posibles
                Vector3[] possibleDirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

                foreach (Vector3 dir in possibleDirs)
                {
                    if (IsValidMove(dir))
                    {
                        Vector3 nextPos = transform.position + dir * gridSize;
                        float distFromNextPosToPlayer = Vector3.Distance(nextPos, player.transform.position);

                        // Solo aceptamos la casilla si nos ACERCA al jugador (no se aleja) y es la más corta posible
                        if (distFromNextPosToPlayer < currentDistance && distFromNextPosToPlayer < minDistance)
                        {
                            minDistance = distFromNextPosToPlayer;
                            bestDir = dir;
                        }
                    }
                }

                // Si ha encontrado una ruta válida que lo acerque
                if (bestDir != Vector3.zero)
                {
                    targetPosition = transform.position + bestDir * gridSize;
                    targetRotation = Quaternion.LookRotation(bestDir);
                    isMoving = true;
                    return;
                }
            }
        }

        isMoving = false;
        waitTimer = waitTimePerTile;
    }

    // Comprobador limpio de muros y precipicios
    private bool IsValidMove(Vector3 dir)
    {
        Vector3 nextPos = transform.position + dir * gridSize;

        // 1. Muro bloqueando de frente
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), dir, out RaycastHit hit, gridSize))
        {
            if (!hit.collider.isTrigger && hit.collider.GetComponent<PlayerController>() == null)
            {
                return false;
            }
        }

        // 2. Falta de suelo (caída)
        if (!Physics.Raycast(nextPos + Vector3.up, Vector3.down, 2f))
        {
            return false;
        }

        return true;
    }

    private void CheckPlayerCollision()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            if (distanceX < 0.3f && distanceZ < 0.3f)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.LoseCoins(coinsToSteal);
                    stats.TakeDamage();
                }
                Destroy(gameObject);
            }
        }
    }
}