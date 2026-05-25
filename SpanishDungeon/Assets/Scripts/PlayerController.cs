using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gridSize = 1f;

    [HideInInspector] public Vector3 targetPosition;
    private Quaternion targetRotation; // Guarda la rotación a la que queremos llegar suavemente
    private bool isMoving = false;
    private bool isFalling = false; // Control de caída
    private Rigidbody rb;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation; // Empezamos con la rotación actual
        rb = GetComponent<Rigidbody>(); // Obtenemos el componente físico
    }

    void Update()
    {
        if (isFalling) return; // Si ya está cayendo, bloqueamos todo lo demás

        if (!isMoving)
        {
            CheckFloorBeneath(); // Comprobamos si el suelo bajo nuestros pies ha desaparecido

            if (!isFalling) // Si sigue habiendo suelo, procesamos el teclado
            {
                CheckInput();
            }
        }
        else
        {
            // 1. Movimiento suave hacia la casilla objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 2. Rotación suave hacia la dirección hacia la que miramos
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            // 3. Si estamos ya casi en el ángulo correcto, forzamos el ángulo exacto al 100%
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
            }

            // --- LA CORRECCIÓN CLAVE ---
            // Solo dejamos de movernos si físicamente estamos en la casilla Y ADEMÁS la rotación ya es perfecta
            bool reachedPosition = Vector3.Distance(transform.position, targetPosition) < 0.01f;
            bool reachedRotation = Quaternion.Angle(transform.rotation, targetRotation) < 0.1f;

            if (reachedPosition && reachedRotation)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation; // Aseguramos el ángulo limpio
                isMoving = false;
            }
        }
    }

    // --- LOGICA DE DETECCIÓN DE PRECIPICIO (EVITA LEVITAR) ---
    private void CheckFloorBeneath()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 2f);
        bool floorFound = false;

        foreach (RaycastHit hit in hits)
        {
            // Ignoramos al propio jugador y ignoramos los disparadores/triggers invisibles
            if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
            {
                floorFound = true;
                break;
            }
        }

        if (!floorFound)
        {
            isFalling = true;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true; // El motor de físicas toma el control y cae al vacío
            }

            GetComponent<PlayerStats>().FallToDeath();
        }
    }

    private void CheckInput()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector3.right;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector3.back;

        if (direction != Vector3.zero)
        {
            Vector3 proposedPosition = transform.position + (direction * gridSize);

            // Calculamos la rotación deseada en base a la dirección pulsada
            targetRotation = Quaternion.LookRotation(direction);

            // --- SOLUCIÓN AL BUG: COMPROBACIÓN DE OBSTÁCULOS (MUROS Y PUERTAS) ---
            // Lanzamos un rayo hacia adelante desde el pecho del jugador. Si choca con algo a 1 casilla de distancia...
            if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), direction, out RaycastHit hit, gridSize))
            {
                // Si el obstáculo NO es un trigger (monedas/salidas) y NO es un enemigo, es un muro o puerta cerrada
                if (!hit.collider.isTrigger && hit.collider.GetComponent<EnemyPatrol>() == null)
                {
                    Debug.Log("Movimiento bloqueado por obstáculo sólido: " + hit.collider.gameObject.name);
                    return; // Detenemos la función aquí. El jugador gira pero NO se mueve.
                }
            }

            // 2. Comprobamos si hay un enemigo en la casilla objetivo
            EnemyPatrol enemyInTarget = GetEnemyAtPosition(proposedPosition);

            if (enemyInTarget != null)
            {
                Debug.Log("¡ZAS! Has atacado y eliminado al enemigo.");
                Destroy(enemyInTarget.gameObject);
                // Aquí se girará hacia el enemigo y se quedará quieto atacando
            }
            else
            {
                // 3. Si la casilla está libre, nos aseguramos de lanzar el rayo hacia abajo antes de saltar
                RaycastHit[] floorHits = Physics.RaycastAll(proposedPosition + Vector3.up, Vector3.down, 2f);
                bool targetFloorFound = false;

                foreach (RaycastHit fHit in floorHits)
                {
                    if (fHit.collider.gameObject != gameObject && !fHit.collider.isTrigger)
                    {
                        targetFloorFound = true;
                        break;
                    }
                }

                if (targetFloorFound)
                {
                    targetPosition = proposedPosition;
                    isMoving = true;
                }
            }
        }
    }

    private EnemyPatrol GetEnemyAtPosition(Vector3 targetPos)
    {
        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);

        foreach (EnemyPatrol enemy in enemies)
        {
            float distanceX = Mathf.Abs(enemy.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(enemy.transform.position.z - targetPos.z);

            if (distanceX < 0.1f && distanceZ < 0.1f)
            {
                return enemy;
            }
        }

        return null;
    }
}