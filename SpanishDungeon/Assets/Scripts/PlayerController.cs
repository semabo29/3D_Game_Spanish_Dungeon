using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gridSize = 1f;

    [HideInInspector] public Vector3 targetPosition;
    private Quaternion targetRotation; 
    private bool isMoving = false;
    private bool isFalling = false; 
    private Rigidbody rb;

    private bool isDrunk = false;
    private float drunkTimer = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stepSound;
    [SerializeField] private AudioClip SonidoAtaque;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        rb = GetComponent<Rigidbody>(); 
    }

    void Update()
    {
        if (isFalling) return; 

        if (isDrunk) 
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
            CheckFloorBeneath();

            if (!isFalling) 
            {
                CheckInput();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
            }

            bool reachedPosition = Vector3.Distance(transform.position, targetPosition) < 0.01f;
            bool reachedRotation = Quaternion.Angle(transform.rotation, targetRotation) < 0.1f;

            if (reachedPosition && reachedRotation)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation; 
                isMoving = false;
            }
        }
    }

    private void CheckFloorBeneath()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up, Vector3.down, 2f);
        bool floorFound = false;

        foreach (RaycastHit hit in hits)
        {
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
                rb.useGravity = true; 
            }

            GetComponent<PlayerStats>().FallToDeath();
        }
    }

    private bool esObstaculoSolido(RaycastHit hit)
    {
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

            if (isDrunk)
            {
                direction = -direction;
            }

            Vector3 proposedPosition = transform.position + (direction * gridSize);

            targetRotation = Quaternion.LookRotation(direction);

            if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), direction, out RaycastHit hit, gridSize))
            {
                if (esObstaculoSolido(hit))
                {
                    Debug.Log("Movimiento bloqueado por obstáculo sólido: " + hit.collider.gameObject.name);
                    return; 
                }
            }

            GameObject enemyInTarget = GetEnemyAtPosition(proposedPosition);

            if (enemyInTarget != null)
            {
                Pedro pedro = enemyInTarget.GetComponent<Pedro>();

                if (pedro != null)
                {
                    pedro.RecibirDano();
                }

                else
                {
                    Debug.Log("¡ZAS! Has atacado y eliminado al enemigo.");
                    Destroy(enemyInTarget);
                }

                if (audioSource != null && SonidoAtaque != null)
                {
                    audioSource.PlayOneShot(SonidoAtaque);
                }
            }
            else
            {
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
        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (EnemyPatrol enemy in enemies)
        {
            float distanceX = Mathf.Abs(enemy.transform.position.x - targetPos.x);
            float distanceZ = Mathf.Abs(enemy.transform.position.z - targetPos.z);
            if (distanceX < 0.1f && distanceZ < 0.1f) return enemy.gameObject;
        }

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