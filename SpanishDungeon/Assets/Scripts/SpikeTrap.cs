using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Configuración del Temporizador")]
    [SerializeField] private float timeActive = 2f;
    [SerializeField] private float timeInactive = 3f;

    [Header("Animación Visual")]
    [Tooltip("Arrastra aquí tu modelo de pinchos que es hijo del prefab")]
    [SerializeField] private Transform pinchosVisual;

    // Valores sacados de tu Inspector
    [SerializeField] private float alturaEscondidos = -1.2f;
    [SerializeField] private float alturaAviso = -0.7f;
    [SerializeField] private float alturaAtaque = -0.25f;

    [SerializeField] private float tiempoAviso = 1f;

    private bool isDangerous = false;
    private bool isWarning = false;
    private float timer = 0f;

    void Start()
    {
        // Posición inicial escondida
        if (pinchosVisual != null)
        {
            pinchosVisual.localPosition = new Vector3(0, alturaEscondidos, 0);
        }

        SetTrapState(false);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isDangerous && timer >= timeActive)
        {
            SetTrapState(false); // Se apaga
        }
        else if (!isDangerous)
        {
            // 1. EL AVISO: Sube rápido pero interpolado hasta la altura de aviso
            if (!isWarning && timer >= (timeInactive - tiempoAviso))
            {
                isWarning = true;
                StopAllCoroutines(); // Por si estaba bajando, cortamos la animación

                // Le damos 0.1 segundos de duración para que suba rápido pero no se teletransporte
                StartCoroutine(MoverPinchos(alturaAviso, 0.1f));
            }
            // 2. EL ATAQUE: Toca salir a pinchar
            else if (timer >= timeInactive)
            {
                SetTrapState(true);
            }
        }

        // Chequeamos daño constantemente si es peligrosa
        if (isDangerous)
        {
            CheckPlayerCollision();
        }
    }

    private void SetTrapState(bool dangerous)
    {
        isDangerous = dangerous;
        timer = 0f;
        isWarning = false;

        // Paramos cualquier animación anterior
        StopAllCoroutines();

        if (dangerous)
        {
            // ATAQUE: En vez de teletransportar, usamos la corrutina con un tiempo casi nulo (0.05s)
            // Esto da el efecto de latigazo rápido pero manteniendo la transición visual
            StartCoroutine(MoverPinchos(alturaAtaque, 0.05f));
        }
        else
        {
            // SE APAGA: Bajan fluidamente hasta esconderse en medio segundo
            StartCoroutine(MoverPinchos(alturaEscondidos, 0.5f));
        }
    }

    // Corrutina general para interpolar el movimiento
    private IEnumerator MoverPinchos(float alturaDestino, float duracion)
    {
        if (pinchosVisual == null) yield break;

        Vector3 posicionInicial = pinchosVisual.localPosition;
        Vector3 posicionFinal = new Vector3(posicionInicial.x, alturaDestino, posicionInicial.z);
        float tiempoPasado = 0f;

        while (tiempoPasado < duracion)
        {
            tiempoPasado += Time.deltaTime;
            float t = tiempoPasado / duracion; // De 0.0 a 1.0

            // Usamos Lerp para calcular la posición intermedia en cada fotograma
            pinchosVisual.localPosition = Vector3.Lerp(posicionInicial, posicionFinal, t);
            yield return null;
        }

        pinchosVisual.localPosition = posicionFinal; // Aseguramos que termine en la posición exacta
    }

    private void CheckPlayerCollision()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - player.transform.position.x);
            float distanceZ = Mathf.Abs(transform.position.z - player.transform.position.z);

            if (distanceX < 0.1f && distanceZ < 0.1f)
            {
                player.GetComponent<PlayerStats>().TakeDamage();
            }
        }
    }
}