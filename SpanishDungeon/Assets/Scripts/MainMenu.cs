using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelCreditos;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoClic;

    public void EmpezarJuego()
    {
        ReproducirClic();
        SceneManager.LoadScene("SampleScene");
    }

    public void AbrirCreditos()
    {
        ReproducirClic();
        panelPrincipal.SetActive(false);
        panelCreditos.SetActive(true);
    }

    public void CerrarCreditos()
    {
        ReproducirClic();
        panelCreditos.SetActive(false);
        panelPrincipal.SetActive(true);
    }

    public void SalirDelJuego()
    {
        ReproducirClic();
        Debug.Log("Saliendo de Spanish Dungeon..."); 
        Application.Quit(); 
    }

    private void ReproducirClic()
    {
        if (audioSource != null && sonidoClic != null)
        {
            audioSource.PlayOneShot(sonidoClic);
        }
    }
}