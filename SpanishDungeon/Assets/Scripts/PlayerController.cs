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

            // Lanzamos un rayo desde arriba hacia abajo en la casilla objetivo. 
            // Si choca con algo (el suelo), nos movemos.
            if (Physics.Raycast(proposedPosition + Vector3.up, Vector3.down, 2f))
            {
                targetPosition = proposedPosition;
                targetRotation = Quaternion.LookRotation(direction);
                isMoving = true;
            }
            else
            {
                // Si no hay suelo (borde del mapa), giramos para mirar hacia allí pero NO saltamos
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}