using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// HUD creado por código (no hace falta tocar la escena):
// corazones y monedas pixel art, progreso de salas, lotería, cronómetro, cartel de sala,
// avisos, tutorial, logros, barra del jefe, viñeta de poca vida, transiciones y estadísticas finales.
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    private static readonly string[] RoomNames =
    {
        "San Fermín", "La Plaza de Toros", "Las Ramblas", "Benidorm", "La Mancha",
        "La A-7", "Magaluf", "La Feria de Abril", "Lloret de Mar", "El Congreso"
    };

    private static readonly string[] RoomTaglines =
    {
        "Corre, que vienen los toros",
        "Ni capote ni muleta: tú y tus puños",
        "Vigila la cartera... y el móvil",
        "Rascacielos, chanclas y calcetines",
        "Esos no son gigantes, son molinos",
        "Radar a 300 metros. Sonría",
        "Lo que pasa en Magaluf, se queda en Magaluf",
        "Rebujito, farolillos y sevillanas",
        "Sangría tibia y guiris a la plancha",
        "Se abre la sesión. Último debate"
    };

    public const int TotalRooms = 10;

    private CanvasScaler scaler;
    private RectTransform root;

    // Overlays
    private Image damageFlash;
    private Image vignette;
    private Image siestaOverlay;
    private Image transition;

    // Estado del jugador
    private RectTransform heartsRow;
    private readonly List<Image> hearts = new List<Image>();
    private RectTransform coinGroup;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI powerUpText;
    private TextMeshProUGUI timerText;

    // Lotería
    private RectTransform lotteryGroup;
    private Image lotteryFill;
    private TextMeshProUGUI lotteryText;

    // Progreso de salas
    private RectTransform roomGroup;
    private TextMeshProUGUI roomLabel;
    private readonly List<Image> roomDots = new List<Image>();

    // Carteles
    private TextMeshProUGUI bannerTitle;
    private TextMeshProUGUI bannerSubtitle;
    private TextMeshProUGUI bannerTagline;
    private CanvasGroup bannerGroup;
    private TextMeshProUGUI toastText;
    private CanvasGroup toastGroup;
    private TextMeshProUGUI hintText;
    private CanvasGroup hintGroup;

    // Logros
    private RectTransform achievementPanel;
    private TextMeshProUGUI achievementTitle;
    private TextMeshProUGUI achievementDesc;
    private readonly Queue<string[]> achievementQueue = new Queue<string[]>();
    private bool showingAchievement;

    // Jefe
    private GameObject bossBar;
    private Image bossFill;
    private TextMeshProUGUI bossLabel;
    private PedroStats trackedBoss;

    private TextMeshProUGUI endStatsText;

    private Coroutine bannerRoutine;
    private Coroutine toastRoutine;
    private Coroutine flashRoutine;
    private Coroutine transitionRoutine;
    private bool runEnded;

    private int shownHealth = -1;
    private int shownCoins = -1;
    private float coinPop;
    private float heartPop;
    private int lowHealthState;
    private string lastPowerUp = "";
    private int currentRoom = 1;
    private int tutorialStep = -1;

    private Sprite heartFull, heartEmpty, heartGold, coinSprite;

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

    public static string GetRoomTagline(int room)
    {
        int i = room - 1;
        return (i >= 0 && i < RoomTaglines.Length) ? RoomTaglines[i] : "";
    }

    void Awake()
    {
        Instance = this;
        BuildSprites();
        BuildCanvas();
        ApplySettings();
        GameSettings.Changed += ApplySettings;

        // Tutorial: se muestra en las tres primeras partidas
        int runs = PlayerPrefs.GetInt("SD_TutorialRuns", 0);
        if (runs < 3)
        {
            tutorialStep = 0;
            PlayerPrefs.SetInt("SD_TutorialRuns", runs + 1);
        }
    }

    void OnDestroy()
    {
        GameSettings.Changed -= ApplySettings;
        if (Instance == this) Instance = null;
    }

    private void ApplySettings()
    {
        float s = GameSettings.HudScale;
        if (scaler != null) scaler.referenceResolution = new Vector2(1920f / s, 1080f / s);
        if (timerText != null) timerText.gameObject.SetActive(GameSettings.ShowTimer && !runEnded);
    }

    // ---------- Sprites pixel art ----------

    private void BuildSprites()
    {
        string[] heart =
        {
            "..kkk.kkk..",
            ".krrrkrrrk.",
            "krwwrrrrrrk",
            "krwrrrrrrrk",
            "krrrrrrrrrk",
            ".krrrrrrrk.",
            "..krrrrrk..",
            "...krrrk...",
            "....krk....",
            ".....k.....",
        };
        heartFull = PixelArt.Make("heart_full", heart, new Dictionary<char, Color>
        {
            { 'k', new Color(0.15f, 0.02f, 0.05f) }, { 'r', new Color(0.9f, 0.12f, 0.2f) }, { 'w', new Color(1f, 0.75f, 0.75f) }
        });
        heartGold = PixelArt.Make("heart_gold", heart, new Dictionary<char, Color>
        {
            { 'k', new Color(0.25f, 0.15f, 0.02f) }, { 'r', new Color(1f, 0.78f, 0.15f) }, { 'w', new Color(1f, 0.97f, 0.7f) }
        });
        heartEmpty = PixelArt.Make("heart_empty", heart, new Dictionary<char, Color>
        {
            { 'k', new Color(0.1f, 0.1f, 0.12f, 0.9f) }, { 'r', new Color(0.25f, 0.25f, 0.3f, 0.7f) }, { 'w', new Color(0.35f, 0.35f, 0.4f, 0.7f) }
        });
        coinSprite = PixelArt.Make("coin", new[]
        {
            "...kkkk...",
            ".kkyyyykk.",
            ".kywyyyyk.",
            "kywyyoyyyk",
            "kyyyyoyyyk",
            "kyyyyoyyyk",
            "kyyyyoyyyk",
            ".kyyyyyyk.",
            ".kkyyyykk.",
            "...kkkk...",
        }, new Dictionary<char, Color>
        {
            { 'k', new Color(0.35f, 0.2f, 0.02f) }, { 'y', new Color(1f, 0.8f, 0.15f) }, { 'w', new Color(1f, 0.97f, 0.75f) }, { 'o', new Color(0.85f, 0.55f, 0.05f) }
        });
    }

    // ---------- Construcción ----------

    private void BuildCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        // Sin GraphicRaycaster: este HUD nunca bloquea los botones de los paneles

        root = (RectTransform)transform;

        siestaOverlay = CreateImage("Siesta", root, new Color(0.45f, 0.65f, 1f, 0f));
        Stretch(siestaOverlay.rectTransform);

        vignette = CreateImage("Vignette", root, new Color(0.85f, 0f, 0.05f, 0f));
        vignette.sprite = PixelArt.Vignette();
        Stretch(vignette.rectTransform);

        damageFlash = CreateImage("DamageFlash", root, new Color(0.8f, 0f, 0f, 0f));
        Stretch(damageFlash.rectTransform);

        // ---- Corazones (arriba a la izquierda) ----
        heartsRow = NewRect("Hearts", root);
        heartsRow.anchorMin = heartsRow.anchorMax = heartsRow.pivot = new Vector2(0f, 1f);
        heartsRow.anchoredPosition = new Vector2(28f, -24f);
        heartsRow.sizeDelta = new Vector2(400f, 64f);

        // ---- Monedas ----
        coinGroup = NewRect("Coins", root);
        coinGroup.anchorMin = coinGroup.anchorMax = coinGroup.pivot = new Vector2(0f, 1f);
        coinGroup.anchoredPosition = new Vector2(28f, -100f);
        coinGroup.sizeDelta = new Vector2(300f, 56f);
        Image coinIcon = CreateImage("CoinIcon", coinGroup, Color.white);
        coinIcon.sprite = coinSprite;
        RectTransform cirt = coinIcon.rectTransform;
        cirt.anchorMin = cirt.anchorMax = cirt.pivot = new Vector2(0f, 0.5f);
        cirt.anchoredPosition = Vector2.zero;
        cirt.sizeDelta = new Vector2(48f, 48f);
        coinText = CreateText("CoinText", coinGroup, 46, new Color(1f, 0.88f, 0.3f), FontStyles.Bold);
        coinText.alignment = TextAlignmentOptions.Left;
        RectTransform ctrt = coinText.rectTransform;
        ctrt.anchorMin = ctrt.anchorMax = ctrt.pivot = new Vector2(0f, 0.5f);
        ctrt.anchoredPosition = new Vector2(60f, 0f);
        ctrt.sizeDelta = new Vector2(240f, 56f);

        // ---- Power-ups activos ----
        powerUpText = CreateText("PowerUps", root, 30, Color.white, FontStyles.Bold);
        powerUpText.alignment = TextAlignmentOptions.Left;
        RectTransform prt = powerUpText.rectTransform;
        prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0f, 1f);
        prt.anchoredPosition = new Vector2(30f, -160f);
        prt.sizeDelta = new Vector2(700f, 40f);

        // ---- Cronómetro ----
        timerText = CreateText("Timer", root, 40, new Color(1f, 1f, 1f, 0.9f), FontStyles.Bold);
        RectTransform tirt = timerText.rectTransform;
        tirt.anchorMin = tirt.anchorMax = tirt.pivot = new Vector2(0.5f, 1f);
        tirt.anchoredPosition = new Vector2(0, -12);
        tirt.sizeDelta = new Vector2(400, 60);

        // ---- Lotería (arriba a la derecha) ----
        lotteryGroup = NewRect("Lottery", root);
        lotteryGroup.anchorMin = lotteryGroup.anchorMax = lotteryGroup.pivot = new Vector2(1f, 1f);
        lotteryGroup.anchoredPosition = new Vector2(-28f, -24f);
        lotteryGroup.sizeDelta = new Vector2(360f, 80f);
        TextMeshProUGUI lotLabel = CreateText("Label", lotteryGroup, 28, new Color(1f, 0.85f, 0.2f), FontStyles.Bold);
        lotLabel.text = "LOTERÍA DE NAVIDAD";
        lotLabel.alignment = TextAlignmentOptions.Right;
        RectTransform llrt = lotLabel.rectTransform;
        llrt.anchorMin = llrt.anchorMax = llrt.pivot = new Vector2(1f, 1f);
        llrt.anchoredPosition = Vector2.zero;
        llrt.sizeDelta = new Vector2(360f, 36f);
        Image lotBack = CreateImage("Back", lotteryGroup, new Color(0f, 0f, 0f, 0.6f));
        RectTransform lbrt = lotBack.rectTransform;
        lbrt.anchorMin = lbrt.anchorMax = lbrt.pivot = new Vector2(1f, 1f);
        lbrt.anchoredPosition = new Vector2(0f, -42f);
        lbrt.sizeDelta = new Vector2(260f, 26f);
        lotteryFill = CreateImage("Fill", lbrt, new Color(1f, 0.78f, 0.15f));
        RectTransform lfrt = lotteryFill.rectTransform;
        lfrt.anchorMin = Vector2.zero;
        lfrt.anchorMax = Vector2.one;
        lfrt.pivot = new Vector2(0f, 0.5f);
        lfrt.offsetMin = new Vector2(3, 3);
        lfrt.offsetMax = new Vector2(-3, -3);
        lfrt.localScale = new Vector3(0f, 1f, 1f);
        lotteryText = CreateText("Count", lotteryGroup, 28, Color.white, FontStyles.Bold);
        lotteryText.alignment = TextAlignmentOptions.Right;
        RectTransform ltrt = lotteryText.rectTransform;
        ltrt.anchorMin = ltrt.anchorMax = ltrt.pivot = new Vector2(1f, 1f);
        ltrt.anchoredPosition = new Vector2(-270f, -38f);
        ltrt.sizeDelta = new Vector2(90f, 34f);

        // ---- Progreso de salas (abajo a la izquierda) ----
        roomGroup = NewRect("Rooms", root);
        roomGroup.anchorMin = roomGroup.anchorMax = roomGroup.pivot = new Vector2(0f, 0f);
        roomGroup.anchoredPosition = new Vector2(28f, 24f);
        roomGroup.sizeDelta = new Vector2(600f, 80f);
        roomLabel = CreateText("RoomLabel", roomGroup, 32, Color.white, FontStyles.Bold);
        roomLabel.alignment = TextAlignmentOptions.Left;
        RectTransform rlrt = roomLabel.rectTransform;
        rlrt.anchorMin = rlrt.anchorMax = rlrt.pivot = new Vector2(0f, 1f);
        rlrt.anchoredPosition = Vector2.zero;
        rlrt.sizeDelta = new Vector2(600f, 40f);
        for (int i = 0; i < TotalRooms; i++)
        {
            Image dot = CreateImage("Dot" + i, roomGroup, Color.gray);
            RectTransform drt = dot.rectTransform;
            drt.anchorMin = drt.anchorMax = drt.pivot = new Vector2(0f, 0f);
            drt.anchoredPosition = new Vector2(i * 30f, 4f);
            drt.sizeDelta = i == TotalRooms - 1 ? new Vector2(26f, 26f) : new Vector2(20f, 20f);
            roomDots.Add(dot);
        }

        // ---- Cartel de sala ----
        GameObject banner = new GameObject("RoomBanner", typeof(RectTransform), typeof(CanvasGroup));
        banner.transform.SetParent(root, false);
        bannerGroup = banner.GetComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        RectTransform brt = (RectTransform)banner.transform;
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = new Vector2(0, 260);
        brt.sizeDelta = new Vector2(1400, 280);
        Image bannerBack = CreateImage("Back", brt, new Color(0f, 0f, 0f, 0.35f));
        bannerBack.rectTransform.sizeDelta = new Vector2(1100f, 240f);
        bannerTitle = CreateText("Title", brt, 52, new Color(1f, 1f, 1f, 0.85f), FontStyles.Bold);
        bannerTitle.rectTransform.anchoredPosition = new Vector2(0, 75);
        bannerSubtitle = CreateText("Subtitle", brt, 100, new Color(1f, 0.82f, 0.1f), FontStyles.Bold);
        bannerSubtitle.rectTransform.anchoredPosition = new Vector2(0, -5);
        bannerTagline = CreateText("Tagline", brt, 38, Color.white, FontStyles.Italic);
        bannerTagline.rectTransform.anchoredPosition = new Vector2(0, -85);

        // ---- Avisos ----
        GameObject toast = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup));
        toast.transform.SetParent(root, false);
        toastGroup = toast.GetComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        RectTransform trt = (RectTransform)toast.transform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, -300);
        trt.sizeDelta = new Vector2(1600, 100);
        toastText = CreateText("ToastText", trt, 54, Color.white, FontStyles.Bold);

        // ---- Tutorial ----
        GameObject hint = new GameObject("Hint", typeof(RectTransform), typeof(CanvasGroup));
        hint.transform.SetParent(root, false);
        hintGroup = hint.GetComponent<CanvasGroup>();
        hintGroup.alpha = 0f;
        RectTransform hrt = (RectTransform)hint.transform;
        hrt.anchorMin = hrt.anchorMax = hrt.pivot = new Vector2(0.5f, 0f);
        hrt.anchoredPosition = new Vector2(0, 40);
        hrt.sizeDelta = new Vector2(1100, 70);
        Image hintBack = CreateImage("Back", hrt, new Color(0f, 0f, 0f, 0.55f));
        Stretch(hintBack.rectTransform);
        hintText = CreateText("HintText", hrt, 36, Color.white, FontStyles.Bold);
        Stretch(hintText.rectTransform);

        // ---- Logros ----
        achievementPanel = NewRect("Achievement", root);
        achievementPanel.anchorMin = achievementPanel.anchorMax = achievementPanel.pivot = new Vector2(1f, 1f);
        achievementPanel.sizeDelta = new Vector2(560f, 130f);
        achievementPanel.anchoredPosition = new Vector2(600f, -120f);
        Image achBorder = CreateImage("Border", achievementPanel, new Color(1f, 0.78f, 0.15f));
        Stretch(achBorder.rectTransform);
        Image achBack = CreateImage("Back", achievementPanel, new Color(0.08f, 0.06f, 0.1f, 0.95f));
        Stretch(achBack.rectTransform);
        achBack.rectTransform.offsetMin = new Vector2(4, 4);
        achBack.rectTransform.offsetMax = new Vector2(-4, -4);
        Image achIcon = CreateImage("Icon", achievementPanel, Color.white);
        achIcon.sprite = heartGold;
        RectTransform airt = achIcon.rectTransform;
        airt.anchorMin = airt.anchorMax = airt.pivot = new Vector2(0f, 0.5f);
        airt.anchoredPosition = new Vector2(20f, 0f);
        airt.sizeDelta = new Vector2(70f, 64f);
        TextMeshProUGUI achHeader = CreateText("Header", achievementPanel, 22, new Color(1f, 0.78f, 0.15f), FontStyles.Bold);
        achHeader.text = "LOGRO DESBLOQUEADO";
        achHeader.alignment = TextAlignmentOptions.Left;
        SetLeft(achHeader.rectTransform, 110f, 40f, 420f, 30f);
        achievementTitle = CreateText("Title", achievementPanel, 34, Color.white, FontStyles.Bold);
        achievementTitle.alignment = TextAlignmentOptions.Left;
        SetLeft(achievementTitle.rectTransform, 110f, 4f, 430f, 40f);
        achievementDesc = CreateText("Desc", achievementPanel, 22, new Color(0.85f, 0.85f, 0.9f), FontStyles.Normal);
        achievementDesc.alignment = TextAlignmentOptions.Left;
        achievementDesc.enableWordWrapping = true;
        SetLeft(achievementDesc.rectTransform, 110f, -36f, 430f, 40f);
        achievementPanel.gameObject.SetActive(false);

        // ---- Barra del jefe ----
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
        back.rectTransform.anchoredPosition = new Vector2(0, -18);
        back.rectTransform.sizeDelta = new Vector2(600, 26);
        bossFill = CreateImage("Fill", back.rectTransform, new Color(0.85f, 0.1f, 0.15f));
        RectTransform frt = bossFill.rectTransform;
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.pivot = new Vector2(0, 0.5f);
        frt.offsetMin = new Vector2(3, 3);
        frt.offsetMax = new Vector2(-3, -3);
        bossBar.SetActive(false);

        // ---- Estadísticas finales ----
        endStatsText = CreateText("EndStats", root, 34, Color.white, FontStyles.Normal);
        RectTransform ert = endStatsText.rectTransform;
        ert.anchoredPosition = new Vector2(0, 250);
        ert.sizeDelta = new Vector2(1300, 380);
        endStatsText.gameObject.SetActive(false);

        // ---- Transición entre salas (encima de todo) ----
        transition = CreateImage("Transition", root, new Color(0.05f, 0.03f, 0.06f, 0f));
        Stretch(transition.rectTransform);
    }

    private RectTransform NewRect(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
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

    private static void SetLeft(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // ---------- Actualización ----------

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (!runEnded) timerText.text = RunStats.FormatTime(RunStats.ElapsedSeconds);

        // Pequeños "pops" de los contadores
        coinPop = Mathf.MoveTowards(coinPop, 0f, dt * 3f);
        coinGroup.localScale = Vector3.one * (1f + coinPop * 0.3f);
        heartPop = Mathf.MoveTowards(heartPop, 0f, dt * 3f);
        heartsRow.localScale = Vector3.one * (1f + heartPop * 0.2f);

        // Latido de los corazones y viñeta cuando queda uno
        bool low = lowHealthState == 1 && !runEnded;
        float beat = Mathf.Pow(Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3.2f)), 6f);
        for (int i = 0; i < hearts.Count; i++)
        {
            float k = (low && i == 0) ? 1f + beat * 0.25f : 1f;
            hearts[i].rectTransform.localScale = Vector3.one * k;
        }
        Color vc = vignette.color;
        float targetV = low ? (GameSettings.Flashes ? 0.25f + beat * 0.3f : 0.35f) : 0f;
        vc.a = Mathf.MoveTowards(vc.a, targetV, dt * 2f);
        vignette.color = vc;

        // Punto de la sala actual parpadeando
        for (int i = 0; i < roomDots.Count; i++)
        {
            int room = i + 1;
            Color c = room < currentRoom ? new Color(1f, 0.8f, 0.15f) : room == currentRoom ? Color.white : new Color(0.2f, 0.2f, 0.25f, 0.85f);
            if (room == TotalRooms && room > currentRoom) c = new Color(0.6f, 0.1f, 0.15f, 0.9f);
            roomDots[i].color = c;
            roomDots[i].rectTransform.localScale = Vector3.one * (room == currentRoom ? 1.15f + Mathf.Sin(Time.unscaledTime * 5f) * 0.1f : 1f);
        }

        // Barra del jefe
        if (trackedBoss == null)
        {
            if (bossBar.activeSelf) bossBar.SetActive(false);
        }
        else
        {
            if (!bossBar.activeSelf && !runEnded) bossBar.SetActive(true);
            float target = (float)trackedBoss.VidasActuales / Mathf.Max(1, trackedBoss.maxVidas);
            Vector3 s = bossFill.rectTransform.localScale;
            s.x = Mathf.MoveTowards(s.x, target, dt * 2f);
            bossFill.rectTransform.localScale = s;
        }

        UpdateTutorial();

        if (!showingAchievement && achievementQueue.Count > 0) StartCoroutine(AchievementRoutine(achievementQueue.Dequeue()));
    }

    // ---------- Tutorial ----------

    private void UpdateTutorial()
    {
        if (tutorialStep < 0 || runEnded)
        {
            if (hintGroup.alpha > 0f) hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, 0f, Time.unscaledDeltaTime * 3f);
            return;
        }

        PlayerController player = PlayerController.Instance;
        string text = null;
        switch (tutorialStep)
        {
            case 0:
                text = "Muévete con <color=#FFD21A>WASD</color> o las <color=#FFD21A>flechas</color>";
                if (player != null && player.StepsTaken > 0) tutorialStep++;
                break;
            case 1:
                text = "Choca contra un enemigo para <color=#FFD21A>atacarle</color>";
                if (RunStats.Kills > 0) tutorialStep++;
                break;
            case 2:
                text = "Elimina a <color=#FFD21A>todos</color> los enemigos para abrir la puerta";
                if (DoorController.AnyOpen) tutorialStep++;
                break;
            case 3:
                text = "¡Cruza la puerta! Sigue la <color=#3CF06A>flecha verde</color>";
                if (currentRoom > 1) tutorialStep++;
                break;
            case 4:
                text = "Rompe macetas y cajas: a veces esconden monedas";
                if (currentRoom > 2 || RunStats.Smashes > 0) tutorialStep++;
                break;
            default:
                tutorialStep = -1;
                break;
        }

        float target = text != null && Time.timeScale > 0f ? 1f : 0f;
        if (text != null) hintText.text = text;
        hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, target, Time.unscaledDeltaTime * 3f);
    }

    // ---------- API pública ----------

    public void SetHealth(int current, int baseMax)
    {
        int slots = Mathf.Max(baseMax, current);
        while (hearts.Count < slots)
        {
            Image h = CreateImage("Heart" + hearts.Count, heartsRow, Color.white);
            RectTransform rt = h.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(hearts.Count * 62f, 0f);
            rt.sizeDelta = new Vector2(56f, 52f);
            hearts.Add(h);
        }
        for (int i = 0; i < hearts.Count; i++)
        {
            bool visible = i < slots;
            hearts[i].gameObject.SetActive(visible);
            if (!visible) continue;
            hearts[i].sprite = i < current ? (i >= baseMax ? heartGold : heartFull) : heartEmpty;
        }
        if (shownHealth >= 0 && current != shownHealth) heartPop = 1f;
        shownHealth = current;
        lowHealthState = current == 1 ? 1 : 0;
    }

    public void SetCoins(int coins)
    {
        if (shownCoins >= 0 && coins != shownCoins) coinPop = 1f;
        shownCoins = coins;
        coinText.text = "x " + coins;
    }

    public void SetRoom(int room)
    {
        currentRoom = room;
        string name = GetRoomName(Mathf.Min(room, TotalRooms)).ToUpperInvariant();
        roomLabel.text = (room >= TotalRooms ? "SALA FINAL" : "SALA " + room + "/" + TotalRooms) + "  ·  " + name;
    }

    public void SetLotteryProgress(int current, int needed)
    {
        lotteryText.text = current + "/" + needed;
        Vector3 s = lotteryFill.rectTransform.localScale;
        s.x = Mathf.Clamp01(current / (float)Mathf.Max(1, needed));
        lotteryFill.rectTransform.localScale = s;
    }

    public void SetPowerUpStatus(string status)
    {
        if (status == lastPowerUp) return;
        lastPowerUp = status;
        powerUpText.text = status;
    }

    public void SetSiestaOverlay(bool on)
    {
        Color c = siestaOverlay.color;
        c.a = on ? 0.14f : 0f;
        siestaOverlay.color = c;
    }

    public void RegisterBoss(PedroStats boss)
    {
        trackedBoss = boss;
        bossFill.rectTransform.localScale = Vector3.one;
    }

    public void ShowRoomBanner(int room)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        SetRoom(room);
        bannerTitle.text = room >= TotalRooms ? "SALA FINAL" : "SALA " + room;
        bannerSubtitle.text = GetRoomName(room);
        bannerTagline.text = GetRoomTagline(room);
        bannerRoutine = StartCoroutine(FadeGroup(bannerGroup, 0.25f, 1.8f, 0.6f));
    }

    public void ShowToast(string message, Color color, float hold = 1.4f)
    {
        if (runEnded) return;
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastText.text = message;
        toastText.color = color;
        toastRoutine = StartCoroutine(FadeGroup(toastGroup, 0.15f, hold, 0.4f));
    }

    public void ShowAchievement(string title, string desc)
    {
        achievementQueue.Enqueue(new[] { title, desc });
    }

    public void FlashDamage()
    {
        if (!GameSettings.Flashes) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    // Fundido rápido desde negro al cambiar de sala
    public void PlayRoomTransition()
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionRoutine());
    }

    public void ShowEndStats(bool victory, int score, int coins, int room, bool newRecord)
    {
        runEnded = true;
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        timerText.gameObject.SetActive(false);
        lotteryGroup.gameObject.SetActive(false);
        powerUpText.gameObject.SetActive(false);
        hintGroup.alpha = 0f;
        bossBar.SetActive(false);
        bannerGroup.alpha = 0f;
        toastGroup.alpha = 0f;
        Color tc = transition.color;
        tc.a = 0f;
        transition.color = tc;

        string record = newRecord ? "<color=#FFD21A>¡NUEVO RÉCORD!</color>" : "Récord: " + RunStats.HighScore;
        string reached = victory ? "<color=#7CF08A>¡Mazmorra completada!</color>" : "Llegaste a la sala " + room + " (" + GetRoomName(room) + ")";
        string cause = (!victory && !string.IsNullOrEmpty(RunStats.DeathCause))
            ? "<color=#FF8A80>Causa de la muerte: " + RunStats.DeathCause + "</color>\n"
            : "";

        endStatsText.text =
            "<size=26><color=#E8E2D0><b>EL CHISMOSO</b> · <i>" + Headline(victory, room, coins) + "</i></color></size>\n" +
            "<size=60><b>Puntuación: " + score + "</b></size>\n" +
            record + "\n" +
            "<size=30>" + reached + "  ·  Tiempo " + RunStats.FormatTime(RunStats.ElapsedSeconds) + "\n" +
            cause +
            "Monedas " + coins + "  ·  Enemigos " + RunStats.Kills + "  ·  Mejor combo x" + RunStats.BestCombo +
            "  ·  Gordos " + RunStats.LotteryWins + "  ·  Destrozos " + RunStats.Smashes + "  ·  Salas limpias " + RunStats.RoomsCleared + "\n" +
            "Dificultad " + GameSettings.DifficultyName(GameSettings.Difficulty) + "  ·  Logros " + Achievements.UnlockedCount + "/" + Achievements.All.Count + "</size>";
        endStatsText.gameObject.SetActive(true);

        if (victory) GameFeel.Celebrate();
    }

    // Titular de periódico sensacionalista sobre la partida
    private static string Headline(bool victory, int room, int coins)
    {
        string place = GetRoomName(room).ToUpperInvariant();
        if (victory)
        {
            string[] wins =
            {
                "TORRENTO DEROGA A PEDRO Y SE PROCLAMA PRESIDENTE DE SU CASA",
                "UN HOMBRE CRUZA ESPAÑA EN UNA TARDE Y AÚN LE DA TIEMPO A LA SIESTA",
                "EL CONGRESO, DESALOJADO POR UN SEÑOR CON " + coins + " MONEDAS EN EL BOLSILLO",
            };
            return wins[Random.Range(0, wins.Length)];
        }
        if (coins >= 20) return "MUERE CON " + coins + " MONEDAS SIN DECLARAR. HACIENDA INVESTIGA";
        string cause = string.IsNullOrEmpty(RunStats.DeathCause) ? "DESAPARECIDO" : RunStats.DeathCause.ToUpperInvariant();
        string[] lines =
        {
            "TORRENTO, " + cause + " EN " + place,
            "ÚLTIMA HORA: UN VECINO, " + cause + ". LOS TESTIGOS: «NO VIMOS NADA»",
            "SUCESOS: " + cause + " EN " + place + ". EL AYUNTAMIENTO PROMETE UN ESTUDIO",
            "«IBA A POR EL PAN», DECLARA LA FAMILIA DEL HOMBRE " + cause,
        };
        return lines[Random.Range(0, lines.Length)];
    }

    // ---------- Corrutinas ----------

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

    private IEnumerator TransitionRoutine()
    {
        float t = 0f;
        const float duration = 0.45f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Color c = transition.color;
            c.a = Mathf.Lerp(0.9f, 0f, t / duration);
            transition.color = c;
            yield return null;
        }
        Color e = transition.color;
        e.a = 0f;
        transition.color = e;
    }

    private IEnumerator AchievementRoutine(string[] data)
    {
        showingAchievement = true;
        achievementTitle.text = data[0];
        achievementDesc.text = data[1];
        achievementPanel.gameObject.SetActive(true);

        float t = 0f;
        Vector2 hidden = new Vector2(600f, -120f), shown = new Vector2(-28f, -120f);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.35f;
            achievementPanel.anchoredPosition = Vector2.LerpUnclamped(hidden, shown, EaseOutBack(Mathf.Clamp01(t)));
            yield return null;
        }
        yield return new WaitForSecondsRealtime(2.6f);
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.3f;
            achievementPanel.anchoredPosition = Vector2.Lerp(shown, hidden, t * t);
            yield return null;
        }
        achievementPanel.gameObject.SetActive(false);
        showingAchievement = false;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }
}
