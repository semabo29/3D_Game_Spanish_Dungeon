using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Configuración del Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gridSize = 1f;

    [Header("Configuración del Ritmo")]
    [SerializeField] private float waitTimePerTile = 1f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private bool isMoving = false;
    private Vector3 currentDirection;
    private float waitTimer = 0f;

    void Start()
    {
        targetPosition = transform.position;

        // 1. DESINCRONIZACIÓN: 50% de probabilidad de empezar yendo a un lado u otro
        currentDirection = Random.value > 0.5f ? Vector3.forward : Vector3.back;
        targetRotation = Quaternion.LookRotation(currentDirection);
        transform.rotation = targetRotation;

        // 2. DESINCRONIZACIÓN: Esperan un tiempo aleatorio antes del primer salto (entre 0 y 1 seg)
        waitTimer = Random.Range(0f, waitTimePerTile);
    }

    void Update()
    {
        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                PrepareNextStep();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = targetRotation;

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                waitTimer = waitTimePerTile;
            }
        }

        CheckPlayerCollision();
    }

    private void PrepareNextStep()
    {
        Vector3 nextPos = transform.position + (currentDirection * gridSize);

        // Si NO hay suelo en la siguiente baldosa (lanzamos rayo hacia abajo)
        if (!Physics.Raycast(nextPos + Vector3.up, Vector3.down, 2f))
        {
            // Invertimos la dirección (se choca con el límite y da la vuelta)
            currentDirection = -currentDirection;
            targetRotation = Quaternion.LookRotation(currentDirection);

            // Calculamos la nueva posición hacia el lado contrario
            nextPos = transform.position + (currentDirection * gridSize);
        }

        targetPosition = nextPos;
        isMoving = true;
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
                Debug.LogError("¡BOOM! El enemigo ha atropellado al jugador.");
            }
        }
    }
    // TRUCO TEMPORAL PARA PRUEBAS: Matar al enemigo al hacerle clic
    private void OnMouseDown()
    {
        Debug.Log("Enemigo destruido con el ratón.");
        Destroy(gameObject);
    }
}