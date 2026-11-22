using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// HUD adicional creado por código (no hace falta tocar la escena):
// cartel de sala, avisos, cronómetro, barra del jefe, flash de daño y estadísticas finales.
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    private static readonly string[] RoomNames =
    {
        "San Fermín",
        "La Plaza de Toros",
        "Las Ramblas",
        "Benidorm",
        "La Mancha",
        "La A-7",
        "Magaluf",
        "La Feria de Abril",
        "Lloret de Mar",
        "El Congreso"
    };

    private RectTransform root;
    private TextMeshProUGUI bannerTitle;
    private TextMeshProUGUI bannerSubtitle;
    private CanvasGroup bannerGroup;
    private TextMeshProUGUI toastText;
    private CanvasGroup toastGroup;
    private TextMeshProUGUI timerText;
    private Image damageFlash;
    private GameObject bossBar;
    private Image bossFill;
    private TextMeshProUGUI bossLabel;
    private TextMeshProUGUI endStatsText;
    private TextMeshProUGUI lotteryText;

    private Coroutine bannerRoutine;
    private Coroutine toastRoutine;
    private Coroutine flashRoutine;
    private PedroStats trackedBoss;
    private bool runEnded;

    public static GameHUD Create()
    {
        if (Instance != null) return Instance;
        GameObject go = new GameObject("GameHUD");
        return go.AddComponent<GameHUD>();
    }

    public static string GetRoomName(int room)
    {
        int i = room - 1;
        return (i >= 0 && i < RoomNames.Length) ? RoomNames[i] : "";
    }

    void Awake()
    {
        Instance = this;
        BuildCanvas();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------- Construcción ----------

    private void BuildCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        // Sin GraphicRaycaster: este HUD nunca bloquea los botones de los paneles

        root = (RectTransform)transform;

        damageFlash = CreateImage("DamageFlash", root, new Color(0.8f, 0f, 0f, 0f));
        Stretch(damageFlash.rectTransform);

        // Cartel de sala
        GameObject banner = new GameObject("RoomBanner", typeof(RectTransform), typeof(CanvasGroup));
        banner.transform.SetParent(root, false);
        bannerGroup = banner.GetComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        RectTransform brt = (RectTransform)banner.transform;
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = new Vector2(0, 260);
        brt.sizeDelta = new Vector2(1400, 220);

        bannerTitle = CreateText("Title", brt, 96, new Color(1f, 0.82f, 0.1f), FontStyles.Bold);
        bannerTitle.rectTransform.anchoredPosition = new Vector2(0, 40);
        bannerSubtitle = CreateText("Subtitle", brt, 48, Color.white, FontStyles.Italic);
        bannerSubtitle.rectTransform.anchoredPosition = new Vector2(0, -50);

        // Avisos
        GameObject toast = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup));
        toast.transform.SetParent(root, false);
        toastGroup = toast.GetComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        RectTransform trt = (RectTransform)toast.transform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, -330);
        trt.sizeDelta = new Vector2(1600, 100);
        toastText = CreateText("ToastText", trt, 56, Color.white, FontStyles.Bold);

        // Cronómetro
        timerText = CreateText("Timer", root, 40, new Color(1f, 1f, 1f, 0.9f), FontStyles.Bold);
        RectTransform tirt = timerText.rectTransform;
        tirt.anchorMin = tirt.anchorMax = new Vector2(0.5f, 1f);
        tirt.pivot = new Vector2(0.5f, 1f);
        tirt.anchoredPosition = new Vector2(0, -10);
        tirt.sizeDelta = new Vector2(400, 60);

        // Barra del jefe
        bossBar = new GameObject("BossBar", typeof(RectTransform));
        bossBar.transform.SetParent(root, false);
        RectTransform bbrt = (RectTransform)bossBar.transform;
        bbrt.anchorMin = bbrt.anchorMax = new Vector2(0.5f, 1f);
        bbrt.pivot = new Vector2(0.5f, 1f);
        bbrt.anchoredPosition = new Vector2(0, -80);
        bbrt.sizeDelta = new Vector2(700, 70);

        bossLabel = CreateText("BossLabel", bbrt, 32, new Color(1f, 0.35f, 0.3f), FontStyles.Bold);
        bossLabel.rectTransform.anchoredPosition = new Vector2(0, 18);
        bossLabel.text = "PEDRO, EL PRESIDENTE";

        Image back = CreateImage("Back", bbrt, new Color(0f, 0f, 0f, 0.7f));
        back.rectTransform.anchorMin = back.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        back.rectTransform.anchoredPosition = new Vector2(0, -18);
        back.rectTransform.sizeDelta = new Vector2(600, 26);

        bossFill = CreateImage("Fill", back.rectTransform, new Color(0.85f, 0.1f, 0.15f));
        RectTransform frt = bossFill.rectTransform;
        frt.anchorMin = new Vector2(0, 0);
        frt.anchorMax = new Vector2(1, 1);
        frt.pivot = new Vector2(0, 0.5f);
        frt.offsetMin = new Vector2(3, 3);
        frt.offsetMax = new Vector2(-3, -3);
        bossBar.SetActive(false);

        // Progreso de la lotería (debajo del contador de monedas)
        lotteryText = CreateText("Lottery", root, 28, new Color(1f, 0.85f, 0.2f), FontStyles.Bold);
        RectTransform lrt = lotteryText.rectTransform;
        lrt.anchorMin = lrt.anchorMax = new Vector2(1f, 1f);
        lrt.pivot = new Vector2(1f, 1f);
        lrt.anchoredPosition = new Vector2(-20, -60);
        lrt.sizeDelta = new Vector2(420, 40);
        lotteryText.alignment = TextAlignmentOptions.Right;

        // Estadísticas de final de partida (sobre los paneles de victoria / derrota)
        endStatsText = CreateText("EndStats", root, 34, Color.white, FontStyles.Normal);
        RectTransform ert = endStatsText.rectTransform;
        ert.anchorMin = ert.anchorMax = new Vector2(0.5f, 0.5f);
        ert.anchoredPosition = new Vector2(0, 250);
        ert.sizeDelta = new Vector2(1200, 340);
        endStatsText.gameObject.SetActive(false);
    }

    private TextMeshProUGUI CreateText(string name, RectTransform parent, float size, Color color, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 0, 0, 255);
        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1400, 120);
        return tmp;
    }

    private Image CreateImage(string name, RectTransform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ---------- Actualización ----------

    void Update()
    {
        if (!runEnded)
        {
            timerText.text = RunStats.FormatTime(RunStats.ElapsedSeconds);
        }

        if (trackedBoss == null)
        {
            if (bossBar.activeSelf) bossBar.SetActive(false);
        }
        else
        {
            if (!bossBar.activeSelf && !runEnded) bossBar.SetActive(true);
            float target = (float)trackedBoss.VidasActuales / Mathf.Max(1, trackedBoss.maxVidas);
            Vector3 s = bossFill.rectTransform.localScale;
            s.x = Mathf.MoveTowards(s.x, target, Time.unscaledDeltaTime * 2f);
            bossFill.rectTransform.localScale = s;
        }
    }

    public void SetLotteryProgress(int current, int needed)
    {
        lotteryText.text = "Lotería " + current + "/" + needed;
    }

    public void RegisterBoss(PedroStats boss)
    {
        trackedBoss = boss;
        bossFill.rectTransform.localScale = Vector3.one;
    }

    // ---------- API pública ----------

    public void ShowRoomBanner(int room)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerTitle.text = room >= RoomNames.Length ? "SALA FINAL" : "SALA " + room;
        bannerSubtitle.text = GetRoomName(room);
        bannerRoutine = StartCoroutine(FadeGroup(bannerGroup, 0.25f, 1.6f, 0.6f));
    }

    public void ShowToast(string message, Color color, float hold = 1.4f)
    {
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastText.text = message;
        toastText.color = color;
        toastRoutine = StartCoroutine(FadeGroup(toastGroup, 0.15f, hold, 0.4f));
    }

    public void FlashDamage()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    public void ShowEndStats(bool victory, int score, int coins, int room, bool newRecord)
    {
        runEnded = true;
        StopAllCoroutines();
        timerText.gameObject.SetActive(false);
        lotteryText.gameObject.SetActive(false);
        bossBar.SetActive(false);
        bannerGroup.alpha = 0f;
        toastGroup.alpha = 0f;

        string record = newRecord
            ? "<color=#FFD21A>¡NUEVO RÉCORD!</color>"
            : "Récord: " + RunStats.HighScore;

        string reached = victory ? "Mazmorra completada" : "Llegaste a la sala " + room + " (" + GetRoomName(room) + ")";

        endStatsText.text =
            "<size=60><b>Puntuación: " + score + "</b></size>\n" +
            record + "\n" +
            "<size=30>" + reached + "  ·  Tiempo " + RunStats.FormatTime(RunStats.ElapsedSeconds) + "\n" +
            "Monedas " + coins + "  ·  Enemigos " + RunStats.Kills + "  ·  Mejor combo x" + RunStats.BestCombo +
            "  ·  Gordos " + RunStats.LotteryWins + "</size>";
        endStatsText.gameObject.SetActive(true);
    }

    private IEnumerator FadeGroup(CanvasGroup group, float fadeIn, float hold, float fadeOut)
    {
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(t / fadeIn);
            group.transform.localScale = Vector3.one * Mathf.Lerp(1.15f, 1f, group.alpha);
            yield return null;
        }
        group.alpha = 1f;
        group.transform.localScale = Vector3.one;
        yield return new WaitForSecondsRealtime(hold);
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.Clamp01(t / fadeOut);
            yield return null;
        }
        group.alpha = 0f;
    }

    private IEnumerator FlashRoutine()
    {
        float t = 0f;
        const float duration = 0.35f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Color c = damageFlash.color;
            c.a = Mathf.Lerp(0.35f, 0f, t / duration);
            damageFlash.color = c;
            yield return null;
        }
        Color end = damageFlash.color;
        end.a = 0f;
        damageFlash.color = end;
    }
}
