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

    private enum Kind { Options, Achievements }

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

    private static void Open(Kind kind, Action onClose)
    {
        if (current != null) return;
        EnsureEventSystem();
        GameObject go = new GameObject(kind == Kind.Options ? "OptionsMenu" : "AchievementsMenu");
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
        panel.sizeDelta = new Vector2(1180f, kind == Kind.Options ? 1010f : 900f);
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
        else
        {
            title.text = "LOGROS  " + Achievements.UnlockedCount + "/" + Achievements.All.Count;
            BuildAchievements();
        }

        Button closeBtn = Btn("Volver", panel, "VOLVER", Close);
        Place((RectTransform)closeBtn.transform, new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(300f, 64f), new Vector2(0.5f, 0f));

        TextMeshProUGUI keys = Txt("Keys", panel, 22, new Color(0.7f, 0.7f, 0.75f), FontStyles.Normal);
        keys.text = kind == Kind.Options ? "Arriba/abajo: elegir   Izq./der.: cambiar   Esc: volver" : "Esc volver";
        Place(keys.rectTransform, new Vector2(1f, 0f), new Vector2(-30f, 50f), new Vector2(420f, 30f), new Vector2(1f, 0f));
        keys.alignment = TextAlignmentOptions.Right;
    }

    private void BuildOptions()
    {
        for (int i = 0; i < GameSettings.OptionCount; i++)
        {
            int index = i;
            RectTransform row = Rect("Row" + i, panel);
            Place(row, new Vector2(0.5f, 1f), new Vector2(0f, -140f - i * 52f), new Vector2(1040f, 48f), new Vector2(0.5f, 1f));
            Image rb = Img("Back", row, new Color(1f, 1f, 1f, 0.04f));
            Stretch(rb.rectTransform);
            rb.raycastTarget = true;
            Button rowBtn = rb.gameObject.AddComponent<Button>();
            rowBtn.transition = Selectable.Transition.None;
            rowBtn.onClick.AddListener(() => { selected = index; ChangeSelected(1); });
            rowBacks.Add(rb);

            TextMeshProUGUI label = Txt("Label", row, 32, Color.white, FontStyles.Bold);
            label.text = GameSettings.Label(i);
            label.alignment = TextAlignmentOptions.Left;
            Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(520f, 46f), new Vector2(0f, 0.5f));

            Button left = Btn("Left", row, "<", () => { selected = index; ChangeSelected(-1); });
            Place((RectTransform)left.transform, new Vector2(1f, 0.5f), new Vector2(-420f, 0f), new Vector2(54f, 42f), new Vector2(1f, 0.5f));
            Button right = Btn("Right", row, ">", () => { selected = index; ChangeSelected(1); });
            Place((RectTransform)right.transform, new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(54f, 42f), new Vector2(1f, 0.5f));

            TextMeshProUGUI value = Txt("Value", row, 32, new Color(1f, 0.85f, 0.3f), FontStyles.Bold);
            value.enableAutoSizing = true;
            value.fontSizeMin = 18f;
            value.fontSizeMax = 32f;
            Place(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(-80f, 0f), new Vector2(280f, 46f), new Vector2(1f, 0.5f));
            rowValues.Add(value);
        }

        hintText = Txt("Hint", panel, 26, new Color(0.8f, 0.85f, 0.95f), FontStyles.Italic);
        hintText.enableWordWrapping = true;
        Place(hintText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(1000f, 60f), new Vector2(0.5f, 0f));
        Refresh();
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
            Place(cell, new Vector2(0.5f, 1f), new Vector2(col == 0 ? -285f : 285f, -140f - row * 64f), new Vector2(550f, 58f), new Vector2(0.5f, 1f));
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
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            Close();
            return;
        }
        if (kind != Kind.Options) return;

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
