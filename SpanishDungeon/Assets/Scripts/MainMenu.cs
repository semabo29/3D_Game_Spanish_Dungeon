using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelCreditos;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoClic;

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        MostrarRecords();
    }

    // Muestra los récords guardados en la parte inferior del menú principal
    private void MostrarRecords()
    {
        if (panelPrincipal == null || RunStats.HighScore <= 0) return;

        GameObject go = new GameObject("Records", typeof(RectTransform));
        go.transform.SetParent(panelPrincipal.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 30;
        tmp.color = new Color(1f, 0.85f, 0.2f);
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 0, 0, 255);

        string text = "Récord: " + RunStats.HighScore + " puntos  ·  Mejor sala: " + RunStats.BestRoom;
        if (RunStats.Wins > 0)
        {
            text += "  ·  Victorias: " + RunStats.Wins + "  ·  Mejor tiempo: " + RunStats.FormatTime(RunStats.BestTime);
        }
        tmp.text = text;

        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 20f);
        rt.sizeDelta = new Vector2(0f, 50f);
    }

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