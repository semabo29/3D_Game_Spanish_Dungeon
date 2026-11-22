using UnityEngine;
using System.Collections; 

public class DoorController : MonoBehaviour
{
    private bool isOpen = false;
    private float checkTimer = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioClip SonidoPuerta;

    [Header("Animación de Ascenso")]
    [SerializeField] private float alturaSubida = 3f; 
    [SerializeField] private float velocidadSubida = 2f;

    void Start()
    {
        Renderer myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            myRenderer.material.color = Color.yellow;
        }
    }

    void Update()
    {
        if (!isOpen)
        {
            // Comprobar 5 veces por segundo es más que suficiente y mucho más barato
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer = 0.2f;
                CheckEnemies();
            }
        }
    }

    private void CheckEnemies()
    {
        EnemyPatrol[] activePatrols = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);

        Enemigo_basico[] activeBasics = FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None);

        Guiri[] activeGuiris = FindObjectsByType<Guiri>(FindObjectsSortMode.None);

        Pedro[] activePedros = FindObjectsByType<Pedro>(FindObjectsSortMode.None);

        if (activePatrols.Length == 0 && activeBasics.Length == 0 && activeGuiris.Length == 0 && activePedros.Length == 0)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;

        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.ShowToast("¡Sala despejada! La puerta está abierta", new Color(0.4f, 1f, 0.5f), 1.3f);
        }
        GameFeel.Shake(0.08f, 0.3f);
        Debug.Log("¡Sala despejada! La puerta se ha abierto.");

        if (SonidoPuerta != null)
        {
            AudioSource.PlayClipAtPoint(SonidoPuerta, Camera.main.transform.position, 0.7f);
        }

        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }

        StartCoroutine(AscenderPuerta());
    }

    private IEnumerator AscenderPuerta()
    {
        Vector3 posicionInicial = transform.position;
        Vector3 posicionFinal = posicionInicial + new Vector3(0, alturaSubida, 0);
        float progreso = 0;

        while (progreso < 1f)
        {
            progreso += Time.deltaTime * velocidadSubida;
            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, progreso);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}