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

        // Mapeo para cámara isométrica a 45º:
        // W (Arriba) -> Salta hacia arriba-derecha (Eje X+)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector3.right;
        }
        // S (Abajo) -> Salta hacia abajo-izquierda (Eje X-)
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector3.left;
        }
        // A (Izquierda) -> Salta hacia arriba-izquierda (Eje Z+)
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector3.forward;
        }
        // D (Derecha) -> Salta hacia abajo-derecha (Eje Z-)
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector3.back;
        }

        if (direction != Vector3.zero)
        {
            targetPosition = transform.position + (direction * gridSize);
            targetRotation = Quaternion.LookRotation(direction);
            isMoving = true;
        }
    }
}