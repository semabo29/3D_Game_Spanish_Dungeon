using UnityEngine;
using System.Collections; // Añadido para poder usar Corrutinas

public class DoorController : MonoBehaviour
{
    private bool isOpen = false;

    [Header("Sonidos")]
    [SerializeField] private AudioClip SonidoPuerta;

    [Header("Animación de Ascenso")]
    [SerializeField] private float alturaSubida = 3f; // Cuántos metros sube
    [SerializeField] private float velocidadSubida = 2f; // A qué velocidad

    void Start()
    {
        // Cambiamos el color de la puerta a amarillo/dorado para que destaque
        Renderer myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            myRenderer.material.color = Color.yellow;
        }
    }

    void Update()
    {
        // Si la puerta está cerrada, comprobamos los enemigos
        if (!isOpen)
        {
            CheckEnemies();
        }
    }

    private void CheckEnemies()
    {
        //Busca los enemigos de tipo patrulla
        EnemyPatrol[] activePatrols = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);

        //Busca los enemigos de tipo básico
        Enemigo_basico[] activeBasics = FindObjectsByType<Enemigo_basico>(FindObjectsSortMode.None);

        //Busca si quedan Guiris activos en la sala
        Guiri[] activeGuiris = FindObjectsByType<Guiri>(FindObjectsSortMode.None);

        //Busca si quedan Guiris activos en la sala
        Pedro[] activePedros = FindObjectsByType<Pedro>(FindObjectsSortMode.None);

        //Si no queda ningun enemigo de ningún tipo, abrimos la puerta
        if (activePatrols.Length == 0 && activeBasics.Length == 0 && activeGuiris.Length == 0 && activePedros.Length == 0)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        Debug.Log("¡Sala despejada! La puerta se ha abierto.");

        if (SonidoPuerta != null)//Reproducimos el sonido de ataque si existe
        {
            //Crea un "altavoz" (fuente de audio) temporal en la posición de la camara (oidos del juego) a 70% volumen , ja que borramos la puerta
            AudioSource.PlayClipAtPoint(SonidoPuerta, Camera.main.transform.position, 0.7f);
        }

        // Desactivamos el collider INMEDIATAMENTE para que el jugador pueda pasar sin chocarse mientras la puerta sube
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }

        // Iniciamos la animación visual de subir
        StartCoroutine(AscenderPuerta());
    }

    private IEnumerator AscenderPuerta()
    {
        Vector3 posicionInicial = transform.position;
        // Calculamos la posición final sumándole la altura que queremos que suba en el eje Y
        Vector3 posicionFinal = posicionInicial + new Vector3(0, alturaSubida, 0);
        float progreso = 0;

        // Bucle que mueve la puerta suavemente frame a frame
        while (progreso < 1f)
        {
            progreso += Time.deltaTime * velocidadSubida;
            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, progreso);
            yield return null;
        }

        // Desactivamos la puerta físicamente cuando ya ha terminado de subir (respetando tu código original)
        gameObject.SetActive(false);
    }
}