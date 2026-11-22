using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Pausa")]
    public GameObject panelPausa;

    private bool juegoPausado = false;

    void Start()
    {
        if (panelPausa != null) panelPausa.SetActive(false);
    }

    void Update()
    {
        // En las pantallas de victoria / derrota no se puede pausar ni reanudar
        if (PlayerStats.GameEnded) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    public void PausarJuego()
    {
        juegoPausado = true;
        if (panelPausa != null) panelPausa.SetActive(true);
        Time.timeScale = 0f; 
        AudioListener.pause = true; 
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        if (panelPausa != null) panelPausa.SetActive(false);
        Time.timeScale = 1f; 
        AudioListener.pause = false;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("SampleScene");
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MenuPrincipal");
    }
}