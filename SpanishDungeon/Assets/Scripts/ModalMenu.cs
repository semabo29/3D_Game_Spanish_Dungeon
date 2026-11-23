using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using TMPro;

// Menús modales creados por código: Opciones (accesibilidad) y Logros.
// Se manejan con teclado (flechas/WASD, Enter, Esc), mando o ratón, y funcionan con el juego en pausa.
public class ModalMenu : MonoBehaviour
{
    public static bool IsOpen => current != null;
    public static int ClosedFrame = -1;
    private static ModalMenu current;

    private enum Kind { Options, Achievements, Shop, Records, Blessing }

    private Kind kind;
    private Action onClose;
    private RectTransform panel;
    private readonly List<Image> rowBacks = new List<Image>();
    private readonly List<TextMeshProUGUI> rowValues = new List<TextMeshProUGUI>();
    private TextMeshProUGUI hintText;
    private int selected;
    private float repeatTimer;

    public static void OpenOptions(Action onClose = null) => Open(Kind.Options, onClose);
    public static void OpenAchievements(Action onClose = null) => Open(Kind.Achievements, onClose);
    public static void OpenRecords(Action onClose = null) => Open(Kind.Records, onClose);

    private PlayerStats shopper;
    private int shopRoom;
    private TextMeshProUGUI coinsText;
    private TextMeshProUGUI pacoText;
    private readonly List<TextMeshProUGUI> rowPrices = new List<TextMeshProUGUI>();

    // Chiringuito de Paco: el juego se congela mientras compras
    public static void OpenShop(PlayerStats stats, int room, Action onClose)
    {
        if (current != null) { onClose?.Invoke(); return; }
        Time.timeScale = 0f;
        EnsureEventSystem();
        GameObject go = new GameObject("ShopMenu");
        ModalMenu m = go.AddComponent<ModalMenu>();
        m.kind = Kind.Shop;
        m.shopper = stats;
        m.shopRoom = room;
        m.onClose = () => { Time.timeScale = 1f; onClose?.Invoke(); };
        current = m;
        m.Build();
        Sfx.Play(SfxType.PowerUp, 0.6f, 1.2f, true);
    }

    private List<Estampitas.Def> offer;
    private PlayerStats blessed;
    private readonly List<RectTransform> cards = new List<RectTransform>();
    private readonly List<Image> cardBorders = new List<Image>();

    // Las estampitas de la abuela: eliges una de tres (no se puede saltar)
    public static void OpenBlessing(PlayerStats stats, Action onClose)
    {
        List<Estampitas.Def> offer = Estampitas.Offer();
        if (current != null || offer.Count == 0) { onClose?.Invoke(); return; }
        Time.timeScale = 0f;
        EnsureEventSystem();
        GameObject go = new GameObject("BlessingMenu");
        ModalMenu m = go.AddComponent<ModalMenu>();
        m.kind = Kind.Blessing;
        m.offer = offer;
        m.blessed = stats;
        m.selected = offer.Count / 2;
        m.onClose = () => { Time.timeScale = 1f; onClose?.Invoke(); };
        current = m;
        m.Build();
        Sfx.Play(SfxType.Achievement, 0.6f, 1.1f, true);
    }

    private static void Open(Kind kind, Action onClose)
    {
        if (current != null) return;
        EnsureEventSystem();
        GameObject go = new GameObject(kind + "Menu");
        ModalMenu m = go.AddComponent<ModalMenu>();
        m.kind = kind;
        m.onClose = onClose;
        current = m;
        m.Build();
        Sfx.Play(SfxType.Select, 0.7f, 1f, true);
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    public void Close()
    {
        if (current == this) current = null;
        ClosedFrame = Time.frameCount;
        Sfx.Play(SfxType.Blip, 0.7f, 0.8f, true);
        onClose?.Invoke();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (current == this) current = null;
    }

    // ---------- Construcción ----------

    private void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        RectTransform root = (RectTransform)transform;

        Image dim = Img("Dim", root, new Color(0f, 0f, 0f, 0.72f));
        Stretch(dim.rectTransform);
        dim.raycastTarget = true; // bloquea los clics a lo que hay debajo

        panel = Rect("Panel", root);
        panel.sizeDelta = new Vector2(1180f, 1010f);
        Image border = Img("Border", panel, new Color(1f, 0.78f, 0.15f));
        Stretch(border.rectTransform);
        Image back = Img("Back", panel, new Color(0.1f, 0.07f, 0.12f, 0.98f));
        Stretch(back.rectTransform);
        back.rectTransform.offsetMin = new Vector2(5, 5);
        back.rectTransform.offsetMax = new Vector2(-5, -5);

        // Franja roja y amarilla arriba, por si había dudas de dónde estamos
        Image stripeA = Img("StripeA", panel, new Color(0.78f, 0.06f, 0.18f));
        Place(stripeA.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -5f), new Vector2(1170f, 14f), new Vector2(0.5f, 1f), true);
        Image stripeB = Img("StripeB", panel, new Color(1f, 0.77f, 0f));
        Place(stripeB.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -19f), new Vector2(1170f, 14f), new Vector2(0.5f, 1f), true);

        TextMeshProUGUI title = Txt("Title", panel, 64, new Color(1f, 0.82f, 0.1f), FontStyles.Bold);
        Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1000f, 80f), new Vector2(0.5f, 1f));

        if (kind == Kind.Options)
        {
            title.text = "OPCIONES";
            BuildOptions();
        }
        else if (kind == Kind.Shop)
        {
            title.text = "EL CHIRINGUITO DE PACO";
            BuildShop();
        }
        else if (kind == Kind.Blessing)
        {
            title.text = "LAS ESTAMPITAS DE LA ABUELA";
            BuildBlessing();
        }
        else if (kind == Kind.Records)
        {
            title.text = "TUS MEJORES PARTIDAS";
            BuildRecords();
        }
        else
        {
            title.text = "LOGROS  " + Achievements.UnlockedCount + "/" + Achievements.All.Count;
            BuildAchievements();
        }

        Button closeBtn = kind == Kind.Blessing
            ? Btn("Elegir", panel, "QUEDÁRMELA", TakeBlessing)
            : Btn("Volver", panel, kind == Kind.Shop ? "SEGUIR" : "VOLVER", Close);
        Place((RectTransform)closeBtn.transform, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(300f, 64f), new Vector2(0.5f, 0f));

        TextMeshProUGUI keys = Txt("Keys", panel, 22, new Color(0.7f, 0.7f, 0.75f), FontStyles.Normal);
        keys.text = kind == Kind.Options ? "Arriba/abajo: elegir   Izq./der.: cambiar   Esc: volver"
                  : kind == Kind.Shop ? "Arriba/abajo: elegir   Enter: comprar   Esc: seguir"
                  : kind == Kind.Blessing ? "Izq./der.: elegir   Enter: quedártela" : "Esc volver";
        Place(keys.rectTransform, new Vector2(1f, 0f), new Vector2(-24f, 8f), new Vector2(560f, 28f), new Vector2(1f, 0f));
        keys.alignment = TextAlignmentOptions.Right;
    }

    private void BuildOptions()
    {
        for (int i = 0; i < GameSettings.OptionCount; i++)
        {
            int index = i;
            RectTransform row = Rect("Row" + i, panel);
            Place(row, new Vector2(0.5f, 1f), new Vector2(0f, -136f - i * 45f), new Vector2(1040f, 41f), new Vector2(0.5f, 1f));
            Image rb = Img("Back", row, new Color(1f, 1f, 1f, 0.04f));
            Stretch(rb.rectTransform);
            rb.raycastTarget = true;
            Button rowBtn = rb.gameObject.AddComponent<Button>();
            rowBtn.transition = Selectable.Transition.None;
            rowBtn.onClick.AddListener(() => { selected = index; ChangeSelected(1); });
            rowBacks.Add(rb);

            TextMeshProUGUI label = Txt("Label", row, 28, Color.white, FontStyles.Bold);
            label.text = GameSettings.Label(i);
            label.alignment = TextAlignmentOptions.Left;
            Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(520f, 40f), new Vector2(0f, 0.5f));

            Button left = Btn("Left", row, "<", () => { selected = index; ChangeSelected(-1); });
            Place((RectTransform)left.transform, new Vector2(1f, 0.5f), new Vector2(-420f, 0f), new Vector2(54f, 36f), new Vector2(1f, 0.5f));
            Button right = Btn("Right", row, ">", () => { selected = index; ChangeSelected(1); });
            Place((RectTransform)right.transform, new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(54f, 36f), new Vector2(1f, 0.5f));

            TextMeshProUGUI value = Txt("Value", row, 32, new Color(1f, 0.85f, 0.3f), FontStyles.Bold);
            value.enableAutoSizing = true;
            value.fontSizeMin = 18f;
            value.fontSizeMax = 28f;
            Place(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(-80f, 0f), new Vector2(280f, 40f), new Vector2(1f, 0.5f));
            rowValues.Add(value);
        }

        hintText = Txt("Hint", panel, 26, new Color(0.8f, 0.85f, 0.95f), FontStyles.Italic);
        hintText.enableWordWrapping = true;
        Place(hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(1000f, 60f), new Vector2(0.5f, 0f));
        Refresh();
    }

    private void BuildShop()
    {
        pacoText = Txt("Paco", panel, 30, new Color(1f, 0.93f, 0.75f), FontStyles.Italic);
        pacoText.enableWordWrapping = true;
        pacoText.text = "Paco: «" + Chiringuito.Greeting(shopRoom) + "»";
        Place(pacoText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(1040f, 50f), new Vector2(0.5f, 1f));

        coinsText = Txt("Coins", panel, 34, new Color(1f, 0.85f, 0.2f), FontStyles.Bold);
        Place(coinsText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -186f), new Vector2(1040f, 46f), new Vector2(0.5f, 1f));

        for (int i = 0; i < Chiringuito.Items.Count; i++)
        {
            int index = i;
            Chiringuito.Item it = Chiringuito.Items[i];
            RectTransform row = Rect("Item" + i, panel);
            Place(row, new Vector2(0.5f, 1f), new Vector2(0f, -250f - i * 84f), new Vector2(1040f, 76f), new Vector2(0.5f, 1f));
            Image rb = Img("Back", row, new Color(1f, 1f, 1f, 0.04f));
            Stretch(rb.rectTransform);
            rb.raycastTarget = true;
            Button rowBtn = rb.gameObject.AddComponent<Button>();
            rowBtn.transition = Selectable.Transition.None;
            rowBtn.onClick.AddListener(() => { selected = index; BuySelected(); });
            rowBacks.Add(rb);

            TextMeshProUGUI name = Txt("Name", row, 32, Color.white, FontStyles.Bold);
            name.text = it.name;
            name.alignment = TextAlignmentOptions.Left;
            Place(name.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -4f), new Vector2(760f, 40f), new Vector2(0f, 1f));

            TextMeshProUGUI desc = Txt("Desc", row, 22, new Color(0.85f, 0.85f, 0.9f), FontStyles.Normal);
            desc.text = it.desc;
            desc.alignment = TextAlignmentOptions.Left;
            Place(desc.rectTransform, new Vector2(0f, 0f), new Vector2(40f, 6f), new Vector2(820f, 30f), new Vector2(0f, 0f));

            TextMeshProUGUI price = Txt("Price", row, 34, new Color(1f, 0.85f, 0.2f), FontStyles.Bold);
            price.alignment = TextAlignmentOptions.Right;
            Place(price.rectTransform, new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(220f, 50f), new Vector2(1f, 0.5f));
            rowPrices.Add(price);
        }

        hintText = Txt("Hint", panel, 28, new Color(0.8f, 0.95f, 0.85f), FontStyles.Bold);
        hintText.enableWordWrapping = true;
        Place(hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(1000f, 60f), new Vector2(0.5f, 0f));
        hintText.text = "";
        RefreshShop();
    }

    private void RefreshShop()
    {
        if (shopper == null) return;
        coinsText.text = "Tus monedas: " + shopper.Coins + "     Corazones: " + shopper.CurrentHealth;
        for (int i = 0; i < Chiringuito.Items.Count; i++)
        {
            Chiringuito.Item it = Chiringuito.Items[i];
            bool canPay = shopper.Coins >= it.price && it.blocked(shopper) == null;
            rowPrices[i].text = it.price + (it.price == 1 ? " moneda" : " monedas");
            rowPrices[i].color = canPay ? new Color(1f, 0.85f, 0.2f) : new Color(0.55f, 0.5f, 0.45f);
            rowBacks[i].color = i == selected ? new Color(1f, 0.78f, 0.15f, 0.22f) : new Color(1f, 1f, 1f, 0.04f);
        }
    }

    private void BuySelected()
    {
        if (shopper == null || selected < 0 || selected >= Chiringuito.Items.Count) return;
        string msg = Chiringuito.TryBuy(Chiringuito.Items[selected], shopper);
        hintText.text = msg;
        RefreshShop();
    }

    private void BuildBlessing()
    {
        TextMeshProUGUI intro = Txt("Intro", panel, 30, new Color(1f, 0.93f, 0.75f), FontStyles.Italic);
        intro.enableWordWrapping = true;
        intro.text = "La abuela: «Toma, hijo, para el camino. Coge la que quieras, que te protege toda la partida.»";
        Place(intro.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(1040f, 80f), new Vector2(0.5f, 1f));

        for (int i = 0; i < offer.Count; i++)
        {
            int index = i;
            Estampitas.Def d = offer[i];
            float x = (i - (offer.Count - 1) / 2f) * 360f;
            RectTransform card = Rect("Card" + i, panel);
            Place(card, new Vector2(0.5f, 1f), new Vector2(x, -240f), new Vector2(330f, 560f), new Vector2(0.5f, 1f));

            Image border = Img("Border", card, new Color(0.85f, 0.65f, 0.2f));
            Stretch(border.rectTransform);
            border.raycastTarget = true;
            Button b = border.gameObject.AddComponent<Button>();
            b.transition = Selectable.Transition.None;
            b.onClick.AddListener(() => { selected = index; TakeBlessing(); });
            cardBorders.Add(border);

            Image paper = Img("Paper", card, new Color(0.98f, 0.94f, 0.84f));
            Stretch(paper.rectTransform);
            paper.rectTransform.offsetMin = new Vector2(8, 8);
            paper.rectTransform.offsetMax = new Vector2(-8, -8);

            // "Ilustración" de la estampita: aureola y un medallón de color
            Image art = Img("Art", card, d.color);
            Place(art.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(282f, 210f), new Vector2(0.5f, 1f));
            Image halo = Img("Halo", art.rectTransform, new Color(1f, 0.92f, 0.45f, 0.9f));
            Place(halo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(150f, 150f), new Vector2(0.5f, 0.5f));
            Image medal = Img("Medal", art.rectTransform, Color.Lerp(d.color, Color.white, 0.25f));
            Place(medal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(118f, 118f), new Vector2(0.5f, 0.5f));
            TextMeshProUGUI initial = Txt("Initial", medal.rectTransform, 76, new Color(0.25f, 0.12f, 0.08f), FontStyles.Bold);
            initial.text = d.name.Substring(0, 1);
            Stretch(initial.rectTransform);

            TextMeshProUGUI name = Txt("Name", card, 34, new Color(0.45f, 0.08f, 0.1f), FontStyles.Bold);
            name.enableWordWrapping = true;
            name.text = d.name;
            Place(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -248f), new Vector2(296f, 90f), new Vector2(0.5f, 1f));

            TextMeshProUGUI saint = Txt("Saint", card, 21, new Color(0.45f, 0.35f, 0.25f), FontStyles.Italic);
            saint.enableWordWrapping = true;
            saint.text = d.saint;
            Place(saint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -338f), new Vector2(290f, 56f), new Vector2(0.5f, 1f));

            TextMeshProUGUI desc = Txt("Desc", card, 26, new Color(0.15f, 0.12f, 0.12f), FontStyles.Bold);
            desc.enableWordWrapping = true;
            desc.text = d.desc;
            Place(desc.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(290f, 140f), new Vector2(0.5f, 0f));

            cards.Add(card);
        }

        hintText = Txt("Hint", panel, 24, new Color(0.8f, 0.85f, 0.95f), FontStyles.Italic);
        Place(hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 104f), new Vector2(1000f, 36f), new Vector2(0.5f, 0f));
        string owned = Estampitas.Count > 0 ? "Ya llevas: " + Estampitas.OwnedNames() + "   ·   " : "";
        hintText.text = owned + "Álbum: " + Estampitas.CollectedEver + "/" + Estampitas.All.Count;
        RefreshBlessing();
    }

    private void RefreshBlessing()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            bool sel = i == selected;
            cards[i].localScale = Vector3.one * (sel ? 1.06f : 0.96f);
            cardBorders[i].color = sel ? new Color(1f, 0.85f, 0.2f) : new Color(0.55f, 0.42f, 0.2f);
        }
    }

    private void TakeBlessing()
    {
        if (offer == null || selected < 0 || selected >= offer.Count) return;
        Estampitas.Def d = offer[selected];
        Estampitas.Take(d.id, blessed);
        if (GameHUD.Instance != null) GameHUD.Instance.ShowToast("Estampita: " + d.name, d.color, 1.8f);
        Sfx.Play(SfxType.PowerUp, 0.9f, 1.1f, true);
        offer = null;
        Close();
    }

    private void BuildRecords()
    {
        List<RunStats.TopRun> runs = RunStats.TopRuns();
        string[] medals = { "1º", "2º", "3º", "4º", "5º" };
        TextMeshProUGUI header = Txt("Header", panel, 26, new Color(0.75f, 0.75f, 0.8f), FontStyles.Bold);
        header.text = "PUESTO        PUNTOS            SALA            TIEMPO          DIFICULTAD";
        Place(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1040f, 40f), new Vector2(0.5f, 1f));
        for (int i = 0; i < 5; i++)
        {
            RectTransform row = Rect("Run" + i, panel);
            Place(row, new Vector2(0.5f, 1f), new Vector2(0f, -200f - i * 90f), new Vector2(1040f, 80f), new Vector2(0.5f, 1f));
            Image rb = Img("Back", row, i == 0 ? new Color(1f, 0.78f, 0.15f, 0.18f) : new Color(1f, 1f, 1f, 0.05f));
            Stretch(rb.rectTransform);
            TextMeshProUGUI t = Txt("Text", row, 34, i == 0 ? new Color(1f, 0.85f, 0.3f) : Color.white, FontStyles.Bold);
            Stretch(t.rectTransform);
            if (i < runs.Count)
            {
                RunStats.TopRun r = runs[i];
                string room = r.victory ? "<color=#7CF08A>¡Ganada!</color>" : "Sala " + r.room;
                t.text = medals[i] + "          " + r.score + "          " + room + "          " + RunStats.FormatTime(r.time) + "          " + GameSettings.DifficultyName(r.difficulty);
            }
            else
            {
                t.text = "<color=#777777>" + medals[i] + "          ---</color>";
            }
        }
        TextMeshProUGUI foot = Txt("Foot", panel, 26, new Color(0.8f, 0.85f, 0.95f), FontStyles.Italic);
        foot.text = runs.Count == 0 ? "Todavía no hay partidas. ¡A la mazmorra!" : "Victorias: " + RunStats.Wins + "   ·   Logros: " + Achievements.UnlockedCount + "/" + Achievements.All.Count;
        Place(foot.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(1000f, 40f), new Vector2(0.5f, 0f));
    }

    private void BuildAchievements()
    {
        List<Achievements.Def> all = Achievements.All;
        int perColumn = Mathf.CeilToInt(all.Count / 2f);
        for (int i = 0; i < all.Count; i++)
        {
            Achievements.Def d = all[i];
            bool unlocked = Achievements.IsUnlocked(d.id);
            int col = i / perColumn, row = i % perColumn;

            RectTransform cell = Rect("Ach" + i, panel);
            Place(cell, new Vector2(0.5f, 1f), new Vector2(col == 0 ? -285f : 285f, -140f - row * 58f), new Vector2(550f, 54f), new Vector2(0.5f, 1f));
            Image cb = Img("Back", cell, unlocked ? new Color(1f, 0.78f, 0.15f, 0.16f) : new Color(1f, 1f, 1f, 0.04f));
            Stretch(cb.rectTransform);

            TextMeshProUGUI t = Txt("Title", cell, 26, unlocked ? new Color(1f, 0.85f, 0.3f) : new Color(0.55f, 0.55f, 0.6f), FontStyles.Bold);
            t.text = unlocked ? d.title : d.title + "  <size=18>(bloqueado)</size>";
            t.alignment = TextAlignmentOptions.Left;
            Place(t.rectTransform, new Vector2(0f, 1f), new Vector2(14f, -2f), new Vector2(530f, 30f), new Vector2(0f, 1f));

            TextMeshProUGUI ds = Txt("Desc", cell, 19, unlocked ? new Color(0.9f, 0.9f, 0.92f) : new Color(0.5f, 0.5f, 0.55f), FontStyles.Normal);
            ds.text = d.desc;
            ds.alignment = TextAlignmentOptions.Left;
            Place(ds.rectTransform, new Vector2(0f, 0f), new Vector2(30f, 3f), new Vector2(515f, 26f), new Vector2(0f, 0f));
        }
    }

    // ---------- Lógica ----------

    private void ChangeSelected(int delta)
    {
        GameSettings.Change(selected, delta);
        if (Backdrop.Instance != null) Backdrop.Instance.SetAmbientEffects(GameSettings.AmbientEffects);
        Sfx.Play(SfxType.Blip, 0.8f, delta > 0 ? 1.1f : 0.95f, true);
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < rowValues.Count; i++)
        {
            rowValues[i].text = GameSettings.Value(i);
            rowBacks[i].color = i == selected ? new Color(1f, 0.78f, 0.15f, 0.22f) : new Color(1f, 1f, 1f, 0.04f);
        }
        if (hintText != null) hintText.text = GameSettings.Hint(selected);
    }

    void Update()
    {
        if (kind == Kind.Blessing)
        {
            // Aquí no se puede salir sin elegir
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                TakeBlessing();
                return;
            }
            int dx = 0;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) dx = -1;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) dx = 1;
            repeatTimer -= Time.unscaledDeltaTime;
            float bx = Input.GetAxisRaw("Horizontal");
            if (dx == 0 && repeatTimer <= 0f && Mathf.Abs(bx) > 0.6f) { dx = bx > 0 ? 1 : -1; repeatTimer = 0.25f; }
            if (Mathf.Abs(bx) < 0.3f) repeatTimer = 0f;
            if (dx != 0 && offer != null)
            {
                selected = (selected + dx + offer.Count) % offer.Count;
                Sfx.Play(SfxType.Blip, 0.6f, 1f, true);
                RefreshBlessing();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            Close();
            return;
        }
        if (kind == Kind.Achievements || kind == Kind.Records) return;

        int v = 0, h = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) v = -1;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) v = 1;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) h = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) h = 1;

        // Mando: stick con repetición
        repeatTimer -= Time.unscaledDeltaTime;
        float ax = Input.GetAxisRaw("Horizontal"), ay = Input.GetAxisRaw("Vertical");
        if (repeatTimer <= 0f && v == 0 && h == 0)
        {
            if (ay > 0.6f) v = -1; else if (ay < -0.6f) v = 1;
            else if (ax > 0.6f) h = 1; else if (ax < -0.6f) h = -1;
            if (v != 0 || h != 0) repeatTimer = 0.25f;
        }
        if (Mathf.Abs(ax) < 0.3f && Mathf.Abs(ay) < 0.3f) repeatTimer = 0f;

        if (kind == Kind.Shop)
        {
            int count = Chiringuito.Items.Count;
            if (v != 0)
            {
                selected = (selected + v + count) % count;
                Sfx.Play(SfxType.Blip, 0.6f, 1f, true);
                RefreshShop();
            }
            bool buy = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton0);
            if (buy) BuySelected();
            return;
        }

        if (v != 0)
        {
            selected = (selected + v + GameSettings.OptionCount) % GameSettings.OptionCount;
            Sfx.Play(SfxType.Blip, 0.6f, 1f, true);
            Refresh();
        }
        if (h != 0) ChangeSelected(h);
    }

    // ---------- Utilidades de UI ----------

    private static RectTransform Rect(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static Image Img(string name, RectTransform parent, Color c)
    {
        RectTransform rt = Rect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        return img;
    }

    private static TextMeshProUGUI Txt(string name, RectTransform parent, float size, Color c, FontStyles style)
    {
        RectTransform rt = Rect(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.color = c;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        return t;
    }

    private static Button Btn(string name, RectTransform parent, string label, UnityEngine.Events.UnityAction action)
    {
        Image img = Img(name, parent, new Color(0.78f, 0.06f, 0.18f));
        img.raycastTarget = true;
        Button b = img.gameObject.AddComponent<Button>();
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1f, 0.85f, 0.4f);
        cb.pressedColor = new Color(1f, 0.7f, 0.2f);
        b.colors = cb;
        b.onClick.AddListener(action);
        TextMeshProUGUI t = Txt("Label", (RectTransform)img.transform, 32, Color.white, FontStyles.Bold);
        t.text = label;
        Stretch(t.rectTransform);
        return b;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot, bool keepAnchor = false)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    // ---------- Inyectar botones en paneles existentes de la escena ----------

    // Clona un botón del panel, le pone otro texto y acción, y lo coloca antes de "beforeName"
    public static Button InjectButton(GameObject panelObj, string label, UnityEngine.Events.UnityAction action, string beforeName)
    {
        if (panelObj == null) return null;
        // Nos quedamos con el grupo de botones hermanos más grande del panel
        Button[] all = panelObj.GetComponentsInChildren<Button>(true);
        if (all.Length == 0) return null;
        Dictionary<Transform, List<Button>> groups = new Dictionary<Transform, List<Button>>();
        foreach (Button b in all)
        {
            if (!groups.TryGetValue(b.transform.parent, out List<Button> g)) groups[b.transform.parent] = g = new List<Button>();
            g.Add(b);
        }
        List<Button> buttons = null;
        foreach (List<Button> g in groups.Values) if (buttons == null || g.Count > buttons.Count) buttons = g;
        Transform container = buttons[0].transform.parent;
        buttons.Sort((a, b) => ((RectTransform)b.transform).anchoredPosition.y.CompareTo(((RectTransform)a.transform).anchoredPosition.y));

        float spacing = 100f;
        if (buttons.Count >= 2)
        {
            spacing = 0f;
            for (int i = 1; i < buttons.Count; i++)
                spacing += ((RectTransform)buttons[i - 1].transform).anchoredPosition.y - ((RectTransform)buttons[i].transform).anchoredPosition.y;
            spacing /= buttons.Count - 1;
            if (spacing < 40f) spacing = 100f;
        }

        Button before = buttons.Find(b => b.name == beforeName);
        // Copiamos el estilo del botón de encima (los de "Salir" o "Menú" suelen ir en rojo)
        int beforeIdx = before != null ? buttons.IndexOf(before) : buttons.Count;
        Button template = beforeIdx > 0 ? buttons[beforeIdx - 1] : (before != null ? before : buttons[buttons.Count - 1]);
        GameObject clone = Instantiate(template.gameObject, container);
        clone.name = "Btn_" + label;
        RectTransform crt = (RectTransform)clone.transform;

        if (before != null)
        {
            float y = ((RectTransform)before.transform).anchoredPosition.y;
            crt.anchoredPosition = new Vector2(crt.anchoredPosition.x, y);
            foreach (Button b in buttons)
            {
                RectTransform rt = (RectTransform)b.transform;
                if (rt.anchoredPosition.y <= y + 0.5f) rt.anchoredPosition -= new Vector2(0f, spacing);
            }
            clone.transform.SetSiblingIndex(before.transform.GetSiblingIndex());
        }
        else
        {
            crt.anchoredPosition -= new Vector2(0f, spacing);
        }

        Button cb = clone.GetComponent<Button>();
        cb.onClick = new Button.ButtonClickedEvent();
        cb.onClick.AddListener(action);

        // Mismo uso de mayúsculas que el resto de botones
        TMP_Text tmp = clone.GetComponentInChildren<TMP_Text>(true);
        Text legacy = tmp == null ? clone.GetComponentInChildren<Text>(true) : null;
        string current = tmp != null ? tmp.text : legacy != null ? legacy.text : "";
        if (!string.IsNullOrEmpty(current) && current == current.ToUpperInvariant()) label = label.ToUpperInvariant();
        if (tmp != null) tmp.text = label;
        else if (legacy != null) legacy.text = label;
        return cb;
    }
}
