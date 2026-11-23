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
    private RectTransform title;
    private TextMeshProUGUI subtitle;
    private float titleTime;

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
            ModalMenu.InjectButton(panelPrincipal, "Récords", () => { ReproducirClic(); ModalMenu.OpenRecords(); }, "Btn_Salir");
            CrearTitulo();
            CrearSplash();

            // En el navegador no se puede cerrar el juego: fuera el botón de salir
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Transform salir = FindDeep(panelPrincipal.transform, "Btn_Salir");
                if (salir != null) salir.gameObject.SetActive(false);
            }
        }
    }

    private static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            Transform r = FindDeep(c, name);
            if (r != null) return r;
        }
        return null;
    }

    // Título del juego: entra cayendo con rebote y luego se balancea un poco
    private void CrearTitulo()
    {
        GameObject go = new GameObject("Titulo", typeof(RectTransform));
        go.transform.SetParent(panelPrincipal.transform, false);
        title = (RectTransform)go.transform;
        title.anchorMin = title.anchorMax = new Vector2(0.5f, 0.5f);
        title.anchoredPosition = new Vector2(0f, 330f);
        title.sizeDelta = new Vector2(1400f, 230f);
        title.SetAsFirstSibling();

        TextMeshProUGUI shadow = NewText("Sombra", title, 150, new Color(0.35f, 0.02f, 0.05f, 0.9f));
        shadow.text = "SPANISH DUNGEON";
        shadow.rectTransform.anchoredPosition = new Vector2(8f, 22f);

        TextMeshProUGUI main = NewText("Texto", title, 150, Color.white);
        main.text = "<color=#E8102E>SPANISH</color> <color=#FFC400>DUNGEON</color>";
        main.outlineWidth = 0.22f;
        main.outlineColor = new Color32(40, 0, 0, 255);
        main.rectTransform.anchoredPosition = new Vector2(0f, 30f);

        subtitle = NewText("Subtitulo", title, 40, new Color(1f, 0.95f, 0.85f));
        subtitle.text = "La mazmorra más castiza de la península";
        subtitle.fontStyle = FontStyles.Italic | FontStyles.Bold;
        subtitle.outlineWidth = 0.25f;
        subtitle.outlineColor = new Color32(0, 0, 0, 255);
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, -78f);
        titleTime = 0f;
    }

    private static TextMeshProUGUI NewText(string name, RectTransform parent, float size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.raycastTarget = false;
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = size;
        t.fontStyle = FontStyles.Bold;
        t.color = color;
        t.enableWordWrapping = false;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1600f, 180f);
        return t;
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
        rt.anchoredPosition = new Vector2(560f, 215f);
        rt.sizeDelta = new Vector2(700f, 60f);
        rt.localRotation = Quaternion.Euler(0f, 0f, 14f);
    }

    void Update()
    {
        if (title != null)
        {
            titleTime += Time.unscaledDeltaTime;
            // Caída con rebote (0,7 s) y luego un balanceo suave
            float k = Mathf.Clamp01(titleTime / 0.7f);
            float bounce = 1f - Mathf.Pow(1f - k, 3f) * Mathf.Cos(k * 9f);
            float y = Mathf.Lerp(700f, 330f, bounce);
            float sway = GameSettings.ReduceMotion ? 0f : Mathf.Sin(Time.unscaledTime * 1.6f);
            title.anchoredPosition = new Vector2(0f, y + (k >= 1f ? sway * 6f : 0f));
            title.localRotation = Quaternion.Euler(0f, 0f, k >= 1f ? sway * 1.2f : 0f);
        }
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
