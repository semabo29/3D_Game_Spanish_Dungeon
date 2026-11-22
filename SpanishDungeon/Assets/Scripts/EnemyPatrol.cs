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

        currentDirection = Random.value > 0.5f ? Vector3.forward : Vector3.back;
        targetRotation = Quaternion.LookRotation(currentDirection);
        transform.rotation = targetRotation;

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

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
            }

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

    private void PrepareNextStep()
    {
        bool hitWall = false;

        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), currentDirection, out RaycastHit hit, gridSize))
        {
            if (!hit.collider.isTrigger && hit.collider.GetComponent<PlayerController>() == null)
            {
                hitWall = true; 
            }
        }

        Vector3 nextPos = transform.position + (currentDirection * gridSize);

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
        PlayerController player = PlayerController.Instance;

        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            if (distanceX < 0.3f && distanceZ < 0.3f)
            {
                player.GetComponent<PlayerStats>().TakeDamage();
                GameFeel.FloatingText(transform.position + Vector3.left * 0.6f, "¡Cornada!", new Color(1f, 0.5f, 0.3f), 4f);
                GameFeel.VoxelBurst(transform.position, new Color(0.45f, 0.25f, 0.12f), 10, 3f);

                Destroy(gameObject);
            }
        }
    }
}