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

    [SerializeField] private float alturaEscondidos = -1.2f;
    [SerializeField] private float alturaAviso = -0.7f;
    [SerializeField] private float alturaAtaque = -0.25f;

    [SerializeField] private float tiempoAviso = 1f;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoAtaque;
    [SerializeField] private float volumenSonido = 0.8f;

    private bool isDangerous = false;
    private bool isWarning = false;
    private float timer = 0f;

    void Start()
    {
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
            SetTrapState(false); 
        }
        else if (!isDangerous)
        {
            if (!isWarning && timer >= (timeInactive - tiempoAviso))
            {
                isWarning = true;
                StopAllCoroutines();

                StartCoroutine(MoverPinchos(alturaAviso, 0.1f));
            }
            else if (timer >= timeInactive)
            {
                SetTrapState(true);
            }
        }

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

        StopAllCoroutines();

        if (dangerous)
        {
            StartCoroutine(MoverPinchos(alturaAtaque, 0.05f));

            if (sonidoAtaque != null)
            {
                AudioSource.PlayClipAtPoint(sonidoAtaque, Camera.main.transform.position, volumenSonido);
            }
        }
        else
        {
            StartCoroutine(MoverPinchos(alturaEscondidos, 0.5f));
        }
    }

    private IEnumerator MoverPinchos(float alturaDestino, float duracion)
    {
        if (pinchosVisual == null) yield break;

        Vector3 posicionInicial = pinchosVisual.localPosition;
        Vector3 posicionFinal = new Vector3(posicionInicial.x, alturaDestino, posicionInicial.z);
        float tiempoPasado = 0f;

        while (tiempoPasado < duracion)
        {
            tiempoPasado += Time.deltaTime;
            float t = tiempoPasado / duracion; 

            pinchosVisual.localPosition = Vector3.Lerp(posicionInicial, posicionFinal, t);
            yield return null;
        }

        pinchosVisual.localPosition = posicionFinal;
    }

    private void CheckPlayerCollision()
    {
        PlayerController player = PlayerController.Instance;

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