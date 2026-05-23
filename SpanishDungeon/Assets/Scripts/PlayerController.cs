using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Tamaño de la casilla en metros (en Unity, 1 unidad = 1 metro)
    [SerializeField] private float gridSize = 1f;

    // Velocidad a la que el personaje se desplazará visualmente hacia la siguiente casilla
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        // Al arrancar, nuestra posición objetivo inicial es donde hayamos colocado el cubo
        targetPosition = transform.position;
    }

    void Update()
    {
        // Si el personaje ya se está desplazando hacia una casilla, no le dejamos pulsar otra tecla
        if (!isMoving)
        {
            CheckInput();
        }
        else
        {
            // Mueve al personaje suavemente desde su posición actual hacia la posición de la casilla objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Si ya ha llegado (o está virtualmente al lado), detenemos el movimiento
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition; // Clavamos la posición exacta
                isMoving = false;
            }
        }
    }

    private void CheckInput()
    {
        Vector3 direction = Vector3.zero;

        // Capturamos la pulsación única de la tecla (GetKeyDown)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector3.forward; // Mover hacia adelante (Z+)
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector3.back;    // Mover hacia atrás (Z-)
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector3.left;    // Mover hacia la izquierda (X-)
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector3.right;   // Mover hacia la derecha (X+)
        }

        // Si se ha pulsado alguna dirección, calculamos la casilla de destino
        if (direction != Vector3.zero)
        {
            targetPosition = transform.position + (direction * gridSize);
            isMoving = true;
        }
    }
}