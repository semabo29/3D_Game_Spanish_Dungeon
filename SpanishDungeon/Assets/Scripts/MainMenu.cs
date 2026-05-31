using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelCreditos;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoClic;

    // Función para el botón JUGAR
    public void EmpezarJuego()
    {
        ReproducirClic();
        // Carga la escena del juego. Cambia "Nivel_1" por el nombre exacto de tu escena de juego.
        SceneManager.LoadScene("SampleScene");
    }

    // Función para mostrar los CRÉDITOS
    public void AbrirCreditos()
    {
        ReproducirClic();
        panelPrincipal.SetActive(false);
        panelCreditos.SetActive(true);
    }

    // Función para ocultar los CRÉDITOS y volver al menú
    public void CerrarCreditos()
    {
        ReproducirClic();
        panelCreditos.SetActive(false);
        panelPrincipal.SetActive(true);
    }

    // Función para el botón SALIR
    public void SalirDelJuego()
    {
        ReproducirClic();
        Debug.Log("Saliendo de Spanish Dungeon..."); // Esto solo se ve en el editor
        Application.Quit(); // Esto cierra el juego cuando esté exportado
    }

    // Función auxiliar para que suene el clic sin repetir código
    private void ReproducirClic()
    {
        if (audioSource != null && sonidoClic != null)
        {
            audioSource.PlayOneShot(sonidoClic);
        }
    }
}