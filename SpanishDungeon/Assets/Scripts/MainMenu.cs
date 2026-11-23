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

    private TextMeshProUGUI splash;

    private static readonly string[] Splashes =
    {
        "¡Ahora con más siesta!",
        "¡Con un 21% de IVA!",
        "¡Sin subvenciones!",
        "¡Aprobado en el Congreso!",
        "¡Con jamón de bellota!",
        "¡Tortilla CON cebolla!",
        "¡Más largo que una obra pública!",
        "¡Patrocinado por nadie!",
        "¡Olé!",
        "¡Ni un paso atrás!",
        "¡Apto para guiris!",
        "¡Paella los jueves!",
    };

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameSettings.Apply();
        MusicDirector.PlayMenu();
        MostrarRecords();

        if (panelPrincipal != null)
        {
            ModalMenu.InjectButton(panelPrincipal, "Opciones", () => { ReproducirClic(); ModalMenu.OpenOptions(); }, "Btn_Salir");
            ModalMenu.InjectButton(panelPrincipal, "Logros", () => { ReproducirClic(); ModalMenu.OpenAchievements(); }, "Btn_Salir");
            CrearSplash();
        }
    }

    // Frase amarilla que late en una esquina del título
    private void CrearSplash()
    {
        GameObject go = new GameObject("Splash", typeof(RectTransform));
        go.transform.SetParent(panelPrincipal.transform, false);
        splash = go.AddComponent<TextMeshProUGUI>();
        splash.raycastTarget = false;
        splash.alignment = TextAlignmentOptions.Center;
        splash.fontSize = 40;
        splash.fontStyle = FontStyles.Bold;
        splash.color = new Color(1f, 0.9f, 0.15f);
        splash.outlineWidth = 0.25f;
        splash.outlineColor = new Color32(80, 20, 0, 255);
        splash.enableWordWrapping = false;
        splash.text = Splashes[Random.Range(0, Splashes.Length)];
        RectTransform rt = splash.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(330f, 250f);
        rt.sizeDelta = new Vector2(700f, 60f);
        rt.localRotation = Quaternion.Euler(0f, 0f, 14f);
    }

    void Update()
    {
        if (splash != null)
        {
            float s = 1f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 6f)) * 0.08f;
            splash.rectTransform.localScale = Vector3.one * s;
        }
    }

    // Muestra los récords guardados en la parte inferior del menú principal
    private void MostrarRecords()
    {
        if (panelPrincipal == null) return;

        GameObject go = new GameObject("Records", typeof(RectTransform));
        go.transform.SetParent(panelPrincipal.transform, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.raycastTarget = false;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 30;
        tmp.color = new Color(1f, 0.85f, 0.2f);
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 0, 0, 255);

        string text = "Logros: " + Achievements.UnlockedCount + "/" + Achievements.All.Count;
        if (RunStats.HighScore > 0)
        {
            text = "Récord: " + RunStats.HighScore + " puntos  ·  Mejor sala: " + RunStats.BestRoom + "  ·  " + text;
            if (RunStats.Wins > 0)
            {
                text += "  ·  Victorias: " + RunStats.Wins + "  ·  Mejor tiempo: " + RunStats.FormatTime(RunStats.BestTime);
            }
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
        if (ModalMenu.IsOpen) return;
        ReproducirClic();
        SceneManager.LoadScene("SampleScene");
    }

    public void AbrirCreditos()
    {
        if (ModalMenu.IsOpen) return;
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
        if (ModalMenu.IsOpen) return;
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
