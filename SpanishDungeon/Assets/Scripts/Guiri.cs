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
    
    private Vector3 dirA;        
    private Vector3 dirB;         
    private int currentSteps = 0; 
    private bool alternateDir = false; 

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;

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

        CheckPlayerCollision();
    }

    private void PrepareNextStep()
    {
        if (currentSteps >= 6)
        {
            ChooseNewDiagonalPath();
        }

        Vector3 chosenStepDir = alternateDir ? dirB : dirA;
        Vector3 proposedPosition = transform.position + (chosenStepDir * gridSize);

        bool hayMuro = false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, chosenStepDir, out hit, gridSize))
        {
            if (!hit.collider.isTrigger && hit.collider.GetComponent<PlayerController>() == null)
            {
                hayMuro = true;
            }
        }

        bool haySuelo = Physics.Raycast(proposedPosition + Vector3.up, Vector3.down, 2f);

        if (!hayMuro && haySuelo)
        {
            targetPosition = proposedPosition;
            targetRotation = Quaternion.LookRotation(chosenStepDir);
            isMoving = true;

            alternateDir = !alternateDir;
            currentSteps++;
        }
        else
        {
            ChooseNewDiagonalPath();
            waitTimer = waitTimePerTile;
        }
    }

    private void ChooseNewDiagonalPath()
    {
        currentSteps = 0;
        alternateDir = false;

        int randomDiagonal = Random.Range(0, 4); 

        switch (randomDiagonal)
        {
            case 0: 
                dirA = Vector3.left;
                dirB = Vector3.forward;
                break;
            case 1: 
                dirA = Vector3.left;
                dirB = Vector3.back;
                break;
            case 2: 
                dirA = Vector3.right;
                dirB = Vector3.forward;
                break;
            case 3: 
                dirA = Vector3.right;
                dirB = Vector3.back;
                break;
        }
    }

    private void CheckPlayerCollision()
    {
        PlayerController player = PlayerController.Instance;
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
                GameFeel.VoxelBurst(transform.position, new Color(1f, 0.55f, 0.7f), 10, 3f);
                Destroy(gameObject); 
            }
        }
    }
}