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
    private bool isMoving = false;
    private float waitTimer = 0f;

    void Start()
    {
        targetPosition = transform.position;
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
            // Movimiento hacia la siguiente casilla
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
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
            float diffX = player.transform.position.x - transform.position.x;
            float diffZ = player.transform.position.z - transform.position.z;

            float gridDistX = Mathf.Abs(diffX) / gridSize;
            float gridDistZ = Mathf.Abs(diffZ) / gridSize;

            // Si está a 2 casillas o menos en ambos ejes
            if (gridDistX <= chaseRange && gridDistZ <= chaseRange)
            {
                Vector3 dir = Vector3.zero;

                if (gridDistX >= gridDistZ && gridDistX > 0.1f) dir = new Vector3(Mathf.Sign(diffX), 0, 0);
                else if (gridDistZ > 0.1f) dir = new Vector3(0, 0, Mathf.Sign(diffZ));

                if (dir != Vector3.zero)
                {
                    Vector3 nextPos = transform.position + dir * gridSize;

                    // Comprueba si hay suelo antes de dar el paso
                    if (Physics.Raycast(nextPos + Vector3.up, Vector3.down, 2f))
                    {
                        targetPosition = nextPos;
                        transform.rotation = Quaternion.LookRotation(dir);
                        isMoving = true;
                        return; // Sale de la función, ya está en movimiento
                    }
                }
            }
        }
        // Si no detecta nada o no puede moverse, se queda quieto
        isMoving = false;
        waitTimer = waitTimePerTile;
    }

    private void CheckPlayerCollision()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            // Si el enemigo toca al jugador (le ataca él)
            if (distanceX < 0.3f && distanceZ < 0.3f)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.LoseCoins(coinsToSteal);
                    stats.TakeDamage();
                }
                Destroy(gameObject); // Desaparece tras atacar
            }
        }
    }
}