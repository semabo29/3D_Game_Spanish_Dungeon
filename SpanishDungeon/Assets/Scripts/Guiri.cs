using UnityEngine;

public class EnemigoBorracho : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float waitTimePerTile = 0.4f; // Un poco más rápido para que se note el zigzag

    [Header("Configuración de Castigo")]
    [SerializeField] private int coinsToSteal = 3;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private float waitTimer = 0f;

    // Variables para la ruta del borracho
    private Vector3 dirA;         // Primera dirección del zigzag (ej. Derecha)
    private Vector3 dirB;         // Segunda dirección del zigzag (ej. Abajo)
    private int currentSteps = 0; // Contador de pasos en la diagonal actual (Max 6)
    private bool alternateDir = false; // Alternador para hacer la "S"

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        // Elige su primera dirección borracha al aparecer
        ChooseNewDiagonalPath();
    }

    void Update()
    {
        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                PrepareNextDrunkStep();
            }
        }
        else
        {
            // Movimiento fluido hacia la baldosa objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                waitTimer = waitTimePerTile;
            }
        }

        // Mantiene la lógica de dañar al jugador si lo toca
        CheckPlayerCollision();
    }

    private void PrepareNextDrunkStep()
    {
        // Si ya ha avanzado los 6 pasos máximos de la "S", cambia de idea de forma aleatoria
        if (currentSteps >= 6)
        {
            ChooseNewDiagonalPath();
        }

        // Alterna entre la dirección A y la dirección B en cada paso para formar la "S"
        Vector3 chosenStepDir = alternateDir ? dirB : dirA;
        Vector3 proposedPosition = transform.position + (chosenStepDir * gridSize);

        // Comprobamos si hay suelo firme antes de saltar (para no caerse por los mapas dinámicos)
        if (Physics.Raycast(proposedPosition + Vector3.up, Vector3.down, 2f))
        {
            // Si el suelo es válido, ejecuta el paso
            targetPosition = proposedPosition;
            targetRotation = Quaternion.LookRotation(chosenStepDir);
            isMoving = true;

            // Registramos el paso dado y alternamos para el siguiente frame de movimiento
            alternateDir = !alternateDir;
            currentSteps++;
        }
        else
        {
            // Como está borracho, si se topa con un muro o un precipicio, cambia de rumbo
            // instantáneamente a otra diagonal aleatoria para no quedarse atascado.
            ChooseNewDiagonalPath();
            waitTimer = waitTimePerTile;
        }
    }

    private void ChooseNewDiagonalPath()
    {
        currentSteps = 0;
        alternateDir = false;

        // Elegimos una de las 4 combinaciones macro-diagonales de forma aleatoria (0 a 3)
        int randomDiagonal = Random.Range(0, 4);

        switch (randomDiagonal)
        {
            case 0: // Diagonal: Izquierda + Adelante
                dirA = Vector3.left;
                dirB = Vector3.forward;
                break;
            case 1: // Diagonal: Izquierda + Atrás
                dirA = Vector3.left;
                dirB = Vector3.back;
                break;
            case 2: // Diagonal: Derecha + Adelante
                dirA = Vector3.right;
                dirB = Vector3.forward;
                break;
            case 3: // Diagonal: Derecha + Atrás
                dirA = Vector3.right;
                dirB = Vector3.back;
                break;
        }
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
                Destroy(gameObject); // Desaparece tras golpear al jugador
            }
        }
    }
}