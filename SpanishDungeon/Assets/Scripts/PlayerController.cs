using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 targetPosition;
    private Quaternion targetRotation; // Guarda la rotación a la que queremos llegar
    private bool isMoving = false;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation; // Empezamos con la rotación inicial
    }

    void Update()
    {
        if (!isMoving)
        {
            CheckInput();
        }
        else
        {
            // Movimiento suave hacia la casilla
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Rotación suave hacia la dirección del movimiento
            transform.rotation = targetRotation;
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
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

            // 1. Siempre giramos para mirar hacia donde hemos pulsado
            transform.rotation = Quaternion.LookRotation(direction);

            // 2. Comprobamos si hay un enemigo en la casilla objetivo
            EnemyPatrol enemyInTarget = GetEnemyAtPosition(proposedPosition);

            if (enemyInTarget != null)
            {
                // ¡ATAQUE! Matamos al enemigo y NO nos movemos de nuestra casilla
                Debug.Log("¡ZAS! Has atacado y eliminado al enemigo.");
                Destroy(enemyInTarget.gameObject);

                // (Más adelante, aquí activaremos la animación de tu espada)
            }
            else
            {
                // 3. Si la casilla está libre, comprobamos si hay suelo para saltar
                if (Physics.Raycast(proposedPosition + Vector3.up, Vector3.down, 2f))
                {
                    targetPosition = proposedPosition;
                    isMoving = true;
                }
            }
        }
    }

    // Nueva función matemática para buscar enemigos en una coordenada exacta
    private EnemyPatrol GetEnemyAtPosition(Vector3 targetPos)
    {
        // Buscamos a todos los enemigos activos en la escena
        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);

        foreach (EnemyPatrol enemy in enemies)
        {
            // Comparamos posiciones ignorando la altura (Y)
            float distanceX = Mathf.Abs(enemy.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(enemy.transform.position.z - targetPos.z);

            // Si la distancia es casi 0, significa que el enemigo está en esa baldosa
            if (distanceX < 0.1f && distanceZ < 0.1f)
            {
                return enemy;
            }
        }

        return null; // No hay nadie en esa casilla
    }
}