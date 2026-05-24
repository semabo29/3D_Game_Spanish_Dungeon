using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gridSize = 1f;

    [HideInInspector] public Vector3 targetPosition;
    private bool isMoving = false;
    private bool isFalling = false; // Control de caída
    private Rigidbody rb;

    void Start()
    {
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody>(); // Obtenemos el componente físico
    }

    void Update()
    {
        if (isFalling) return; // Si ya está cayendo, no hacemos nada más

        if (!isMoving)
        {
            CheckFloorBeneath(); // Comprobamos si el suelo ha desaparecido

            if (!isFalling) // Si sigue habiendo suelo, puede moverse
            {
                CheckInput();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    // --- NUEVA LÓGICA DE DETECCIÓN DE PRECIPICIO ---
    private void CheckFloorBeneath()
    {
        // Lanzamos un rayo hacia abajo y guardamos TODOS los objetos que toca
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 2f);
        bool floorFound = false;

        foreach (RaycastHit hit in hits)
        {
            // Si el objeto que tocamos NO es el propio jugador y NO es un trigger (como zonas invisibles)
            if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
            {
                floorFound = true;
                break; // Encontramos suelo real, dejamos de buscar
            }
        }

        if (!floorFound)
        {
            // ¡NO HAY SUELO REAL!
            isFalling = true;

            if (rb != null)
            {
                // Le quitamos el Kinematic y le activamos la gravedad para que caiga
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            // Llamamos a PlayerStats para forzar el Game Over instantáneo
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

            transform.rotation = Quaternion.LookRotation(direction);

            EnemyPatrol enemyInTarget = GetEnemyAtPosition(proposedPosition);

            if (enemyInTarget != null)
            {
                Debug.Log("¡ZAS! Has atacado y eliminado al enemigo.");
                Destroy(enemyInTarget.gameObject);
            }
            else
            {
                if (Physics.Raycast(proposedPosition + Vector3.up, Vector3.down, 2f))
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