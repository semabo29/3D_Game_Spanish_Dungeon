using UnityEngine;

public class Guiri : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float waitTimePerTile = 0.4f;

    [Header("Rastro")]
    [SerializeField] private GameObject brokenBeerPrefab;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private float waitTimer = 0f;
    
    //Variables para la ruta del guiri
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
                PrepareNextStep();
            }
        }
        else
        {
            //Movimiento fluido hacia la baldosa objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                waitTimer = waitTimePerTile;
                if (brokenBeerPrefab != null)
                {
                    Instantiate(brokenBeerPrefab, transform.position, Quaternion.identity);
                }
            }
        }

        //Mantiene la lógica de dañar al jugador si lo toca
        CheckPlayerCollision();
    }

    private void PrepareNextStep()
    {
        // Si ya ha avanzado los 6 pasos máximos de la "S", cambia de idea de forma aleatoria
        if (currentSteps >= 6)
        {
            ChooseNewDiagonalPath();
        }

        // Alterna entre la dirección A y la dirección B en cada paso para formar la "S"
        Vector3 chosenStepDir = alternateDir ? dirB : dirA;
        Vector3 proposedPosition = transform.position + (chosenStepDir * gridSize);

        bool hayMuro = false;
        // Lanzamos un Raycast desde el centro del Guiri hacia la dirección del paso para ver si choca con un muro/bloque
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, chosenStepDir, out hit, gridSize))
        {
            //Si lo que hay delante NO es un trigger y NO es el jugador, lo tratamos como un muro
            if (!hit.collider.isTrigger && hit.collider.GetComponent<PlayerController>() == null)
            {
                hayMuro = true;
            }
        }

        //Comprobamos si hay suelo firme antes de saltar (para no caerse por el fin del mapa)
        bool haySuelo = Physics.Raycast(proposedPosition + Vector3.up, Vector3.down, 2f);

        //Si NO hay muro y SÍ hay suelo firme, el Guiri puede avanzar de forma segura
        if (!hayMuro && haySuelo)
        {
            //Si el paso es válido, ejecuta el paso
            targetPosition = proposedPosition;
            targetRotation = Quaternion.LookRotation(chosenStepDir);
            isMoving = true;

            //Registramos el paso dado y alternamos para el siguiente frame de movimiento
            alternateDir = !alternateDir;
            currentSteps++;
        }
        else
        {
            // Como está borracho, si se topa con un muro, un bloque cambiado o un precipicio, 
            // cambia de rumbo instantáneamente a otra diagonal aleatoria para no quedarse atascado o salirse.
            ChooseNewDiagonalPath();
            waitTimer = waitTimePerTile;
        }
    }

    private void ChooseNewDiagonalPath()
    {
        currentSteps = 0;
        alternateDir = false;

        //Elegimos una de las 4 combinaciones macro-diagonales de forma aleatoria (0 a 3)
        int randomDiagonal = Random.Range(0, 4); //devuelve 0,1,2 o 3, el 4 no esta incluido

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
                    stats.TakeDamage();
                }
                Destroy(gameObject); //Desaparece tras golpear al jugador
            }
        }
    }
}