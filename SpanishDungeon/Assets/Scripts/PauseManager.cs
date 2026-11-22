using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("UI Pausa")]
    public GameObject panelPausa;

    private bool juegoPausado = false;
    private TextMeshProUGUI tipText;

    private static readonly string[] Tips =
    {
        "Si ves un toro, no le hagas fotos: dale.",
        "Las multas llegan siempre. Como Hacienda.",
        "Los molinos no son gigantes, pero duelen igual.",
        "La siesta no es pereza: es estrategia.",
        "Un bocata de calamares lo cura todo.",
        "Cada 15 monedas te toca la lotería. Hacienda no se entera.",
        "Encadena golpes para hacer combos y ganar monedas extra.",
        "El agua del botijo quita la borrachera.",
        "El abanico flamenco te salva de un golpe. ¡Olé!",
        "Los guiris dejan cerveza por el suelo. No la pises.",
        "Pedro miente más que habla. Pégale igual.",
        "Las palomas de Las Ramblas también roban. Bueno, eso dicen.",
        "El suelo se hunde desde la entrada: no te entretengas.",
        "Mantén la tecla pulsada para avanzar sin parar (se puede quitar en Opciones).",
        "Los churros dan velocidad. Y diabetes, pero eso luego.",
        "Rompe todo lo que veas. Es cultura.",
    };

    void Start()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
            ModalMenu.InjectButton(panelPausa, "OPCIONES", () => ModalMenu.OpenOptions(), "Btn_Menu");
            CreateTip();
        }
    }

    // "Consejo de la abuela" en la parte inferior del menú de pausa
    private void CreateTip()
    {
        GameObject go = new GameObject("ConsejoAbuela", typeof(RectTransform));
        go.transform.SetParent(panelPausa.transform, false);
        tipText = go.AddComponent<TextMeshProUGUI>();
        tipText.raycastTarget = false;
        tipText.alignment = TextAlignmentOptions.Center;
        tipText.fontSize = 28;
        tipText.fontStyle = FontStyles.Italic;
        tipText.color = new Color(1f, 0.88f, 0.5f);
        tipText.enableWordWrapping = true;
        tipText.outlineWidth = 0.2f;
        tipText.outlineColor = new Color32(0, 0, 0, 255);
        RectTransform rt = tipText.rectTransform;
        rt.anchorMin = new Vector2(0.1f, 0f);
        rt.anchorMax = new Vector2(0.9f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 30f);
        rt.sizeDelta = new Vector2(0f, 90f);
    }

    void Update()
    {
        // En las pantallas de victoria / derrota no se puede pausar ni reanudar
        if (PlayerStats.GameEnded) return;
        // Las teclas las gestiona el menú de opciones mientras está abierto
        if (ModalMenu.IsOpen || ModalMenu.ClosedFrame == Time.frameCount) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton7))
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
        if (tipText != null) tipText.text = "Consejo de la abuela: " + Tips[Random.Range(0, Tips.Length)];
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
