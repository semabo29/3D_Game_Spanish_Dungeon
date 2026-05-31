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

    //Variables al tocar cerveza rota
    private bool isDrunk = false;
    private float drunkTimer = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stepSound;
    [SerializeField] private AudioClip SonidoAtaque;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation; // Empezamos con la rotación actual
        rb = GetComponent<Rigidbody>(); // Obtenemos el componente físico
    }

    void Update()
    {
        if (isFalling) return; // Si ya está cayendo, bloqueamos todo lo demás

        if (isDrunk) //Contador del efecto de la cerveza rota
        {
            drunkTimer -= Time.deltaTime;
            if (drunkTimer <= 0f)
            {
                isDrunk = false;
                Debug.Log("¡Ya se te ha pasado el efecto de la cerveza! Controles normales.");
            }
        }

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

    private bool esObstaculoSolido(RaycastHit hit)
    {
        // Un obstáculo sólido es aquel que NO es un trigger y NO es un enemigo
        return !hit.collider.isTrigger &&
               hit.collider.GetComponent<EnemyPatrol>() == null &&
               hit.collider.GetComponent<Enemigo_basico>() == null &&
               hit.collider.GetComponent<Guiri>() == null &&
               hit.collider.GetComponent<Pedro>() == null;
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

            //Si esta bajo el efecto de la cerveza rota, invertimos la dirección del movimiento
            if (isDrunk)
            {
                direction = -direction;
            }

            Vector3 proposedPosition = transform.position + (direction * gridSize);

            // Calculamos la rotación deseada en base a la dirección pulsada
            targetRotation = Quaternion.LookRotation(direction);

            // COMPROBACIÓN DE OBSTÁCULOS (MUROS Y PUERTAS) ---
            // Lanzamos un rayo hacia adelante desde el pecho del jugador. Si choca con algo a 1 casilla de distancia...
            if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), direction, out RaycastHit hit, gridSize))
            {
                // Si el obstáculo NO es un trigger (monedas/salidas) y NO es un enemigo, es un muro o puerta cerrada
                if (esObstaculoSolido(hit))
                {
                    Debug.Log("Movimiento bloqueado por obstáculo sólido: " + hit.collider.gameObject.name);
                    return; // Detenemos la función aquí. El jugador gira pero NO se mueve.
                }
            }

            GameObject enemyInTarget = GetEnemyAtPosition(proposedPosition);

            if (enemyInTarget != null)
            {
                Pedro pedro = enemyInTarget.GetComponent<Pedro>();

                if (pedro != null)//Si elenemigo es el rei, no lo matamos, le quitamos una vida
                {
                    pedro.RecibirDano();
                }

                else//Si el enemigo no es el jefe pedro, lo eliminamos directamente
                {
                    Debug.Log("¡ZAS! Has atacado y eliminado al enemigo.");
                    Destroy(enemyInTarget);
                }

                //Reproducimos el sonido de ataque si el AudioSource y el propio audio estan assignados
                if (audioSource != null && SonidoAtaque != null)
                {
                    audioSource.PlayOneShot(SonidoAtaque);
                }
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
                    //Reproducimos el sonido de paso al iniciar el movimiento si el AudioSource y el propio audio estan assignados
                    if (audioSource != null && stepSound != null)
                    {
                        audioSource.PlayOneShot(stepSound);
                    }
                }
            }
        }
    }

    private GameObject GetEnemyAtPosition(Vector3 targetPos)
    {
        // Busca si hay un EnemyPatrol normal
        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (EnemyPatrol enemy in enemies)
        {
            float distanceX = Mathf.Abs(enemy.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(enemy.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return enemy.gameObject;
        }

        // Busca si hay un Enemigo_basico
        Enemigo_basico[] basicos = FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None);
        foreach (Enemigo_basico basico in basicos)
        {
            float distanceX = Mathf.Abs(basico.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(basico.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return basico.gameObject;
        }

        Guiri[] guiris = FindObjectsByType<Guiri>(FindObjectsSortMode.None);
        foreach (Guiri guiri in guiris)
        {
            float distanceX = Mathf.Abs(guiri.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(guiri.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return guiri.gameObject;
        }

        Pedro[] pedros = FindObjectsByType<Pedro>(FindObjectsSortMode.None);
        foreach (Pedro pedro in pedros)
        {
            float distanceX = Mathf.Abs(pedro.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(pedro.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return pedro.gameObject;
        }

        return null;
    }

    public void Enborrachar(float duracion)
    {
        isDrunk = true;
        drunkTimer = duracion;
        Debug.Log("¡Te has emborrachado! Controles invertidos durante " + duracion + " segundos.");
    }
}