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
            // 1. Movimiento suave hacia la casilla objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 2. Rotación suave (Slerp) copiada de tu PlayerController
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            // 3. Ajuste fino para evitar vibraciones al final del giro
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
            }

            // Comprobamos si hemos llegado a la casilla
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                // Opcional: Aseguramos la rotación exacta al terminar el movimiento
                transform.rotation = targetRotation;
                isMoving = false;
                waitTimer = waitTimePerTile;
            }
        }

        CheckPlayerCollision();
    }

    private void PrepareNextStep()
    {
        bool hitWall = false;

        // 1. Lanzamos un rayo hacia adelante para ver si hay un muro o puerta
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), currentDirection, out RaycastHit hit, gridSize))
        {
            if (!hit.collider.isTrigger && hit.collider.GetComponent<PlayerController>() == null)
            {
                hitWall = true; // ¡Hemos chocado con algo sólido!
            }
        }

        Vector3 nextPos = transform.position + (currentDirection * gridSize);

        // 2. Si hay muro DELANTE, o NO hay suelo en la siguiente baldosa, damos la vuelta
        if (hitWall || !Physics.Raycast(nextPos + Vector3.up, Vector3.down, 2f))
        {
            currentDirection = -currentDirection;
            targetRotation = Quaternion.LookRotation(currentDirection);
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
                // Quitamos vida al jugador
                player.GetComponent<PlayerStats>().TakeDamage();

                // Destruimos al enemigo para que no te quite los 3 corazones de golpe en un segundo
                Destroy(gameObject);
            }
        }
    }
}