using UnityEngine;

public class ReiEmerito : MonoBehaviour
{
    [Header("Estadísticas del Jefe")]
    public int vidas = 3;

    [Header("Movimiento Inteligente")]
    public float moveSpeed = 2f;
    public float gridSize = 1f;
    public float waitTimePerTile = 1.5f; //Ritmo al que avanza por las baldosas

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private float waitTimer = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip SonidoAtaque;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                if (CheckPlayerCollision())
                {
                    waitTimer = waitTimePerTile; //Reinicia el tiempo de espera sin moverse ja que ha atacado al jugador
                }
                else
                {
                    //Si el jugador no está cerca, calcula su ruta y se mueve normalmente
                    CalculateNextStep();
                }
            }
        }
        else
        {
            //Movimiento suave hacia la casilla objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                waitTimer = waitTimePerTile;
            }
        }

        CheckPlayerCollision();
    }

    //PERSECUCIÓN CON EVASIÓN DE OBSTÁCULOS
    private void CalculateNextStep()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        Vector3 diff = player.transform.position - transform.position;
        Vector3 stepDir = Vector3.zero;

        //Comparamos qué distancia es mayor para acortar camino primero por ahí
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z))
        {
            stepDir = new Vector3(Mathf.Sign(diff.x), 0, 0); //Intenta avanzar por el eje X
            if (!IsPathClear(stepDir))
            {
                stepDir = new Vector3(0, 0, Mathf.Sign(diff.z)); //Chocó, intenta esquivar por el eje Z
                if (!IsPathClear(stepDir)) stepDir = Vector3.zero; //Todo bloqueado, se queda quieto
            }
        }
        else
        {
            stepDir = new Vector3(0, 0, Mathf.Sign(diff.z)); //Intenta avanzar por el eje Z
            if (!IsPathClear(stepDir))
            {
                stepDir = new Vector3(Mathf.Sign(diff.x), 0, 0); //Chocó, intenta esquivar por el eje X
                if (!IsPathClear(stepDir)) stepDir = Vector3.zero;
            }
        }

       if (stepDir != Vector3.zero)
        {
            targetPosition = transform.position + (stepDir * gridSize);
            targetRotation = Quaternion.LookRotation(stepDir);
            isMoving = true;
        }
        else
        {
            waitTimer = waitTimePerTile; //Pierde el turno si está encerrado
        }
    }

    private bool IsPathClear(Vector3 dir)
    {
        if (dir == Vector3.zero) return false;

        //Calculamos la posición a la que quiere ir
        Vector3 nextPos = transform.position + (dir * gridSize);

        //COMPROBACIÓN DE PAREDES
        //Definimos una caja flotante del tamaño del jefe (2x2).
        //Usamos 0.9f de "radio" (la mitad de casi 2) para que quepa bien sin rozar las paredes laterales.
        Vector3 tamañoCajaBusqueda = new Vector3(0.9f, 0.5f, 0.9f);
        Collider[] objetosDetectados = Physics.OverlapBox(nextPos + Vector3.up * 0.5f, tamañoCajaBusqueda, Quaternion.identity);

        foreach (Collider col in objetosDetectados)
        {
            // gnoramos los triggers, al jugador y a NOSOTROS MISMOS
            if (col.gameObject != gameObject && !col.isTrigger && col.GetComponent<PlayerController>() == null)
            {
                return false; //¡Hemos detectado un muro u obstáculo sólido!
            }
        }

        //COMPROBACIÓN DE SUELO
        //Lanzamos una pequeña caja hacia abajo justo en la grieta/centro de las 4 baldosas
        Vector3 posicionSuelo = nextPos + Vector3.down * 0.5f;
        Vector3 tamañoCajaSuelo = new Vector3(0.4f, 0.1f, 0.4f);
        Collider[] sueloDetectado = Physics.OverlapBox(posicionSuelo, tamañoCajaSuelo, Quaternion.identity);

        bool haySuelo = false;
        foreach (Collider col in sueloDetectado)
        {
            //Buscamos cualquier cosa que no seamos nosotros mismos y que no sea un trigger
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                haySuelo = true;
                break;
            }
        }

        if (!haySuelo) return false; //Si esa intersección no tiene suelo debajo, no salta

        return true; // Los carriles están despejados y hay suelo, puede avanzar
    }

    //ATAQUE AL JUGADOR
    private bool CheckPlayerCollision()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            //Al ser 2x2 y estar en el centro, la distancia al jugador cuando lo pisa es de 0.5f aprox.
            //Usamos < 1.2f para dar un margen seguro.
            if (distanceX < 1.2f && distanceZ < 1.2f)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null) stats.TakeDamage();
                audioSource.PlayOneShot(SonidoAtaque, 0.3f);//Reproducimos el sonido de ataque
                return true; //Sí, ha atacado, por lo que bloqueará el movimiento
            }
        }
        return false; //No ha atacado, puede moverse
    }

    // --- SISTEMA DE VIDAS ---
    public void RecibirDano()
    {
        vidas--;
        Debug.Log("¡El Rey Emérito recibe un golpe! Vidas restantes: " + vidas);

        if (vidas <= 0)
        {
            Debug.Log("¡Jefe derrotado!");
            Destroy(gameObject);
        }
    }
}