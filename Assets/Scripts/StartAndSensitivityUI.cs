using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartAndSensitivityUI : MonoBehaviour
{
    const string InstanceName = "Start And Sensitivity UI";

    static readonly Color BackgroundColor = new Color(0.005f, 0.018f, 0.035f, 0.92f);
    static readonly Color PanelColor = new Color(0.018f, 0.055f, 0.09f, 0.86f);
    static readonly Color AccentColor = new Color(0f, 0.86f, 1f, 1f);
    static readonly Color HotColor = new Color(1f, 0.28f, 0.58f, 1f);
    static readonly Color TextColor = new Color(0.9f, 0.99f, 1f, 1f);
    static readonly Color MutedTextColor = new Color(0.46f, 0.73f, 0.8f, 1f);

    PlayerController player;
    CanvasGroup startGroup;
    CanvasGroup sensitivityGroup;
    Image sensitivityFill;
    Image sensitivityPulse;
    Text sensitivityValueText;
    Text sensitivityStateText;
    bool started;
    bool sensitivityOpen;

    public static StartAndSensitivityUI EnsureInstance(PlayerController player)
    {
        StartAndSensitivityUI existing = FindFirstObjectByType<StartAndSensitivityUI>();
        if (existing != null)
        {
            existing.Bind(player);
            return existing;
        }

        GameObject root = new GameObject(InstanceName);
        DontDestroyOnLoad(root);
        StartAndSensitivityUI ui = root.AddComponent<StartAndSensitivityUI>();
        ui.Bind(player);
        return ui;
    }

    public void Bind(PlayerController targetPlayer)
    {
        player = targetPlayer;

        if (!started && player != null)
        {
            player.SetControlsEnabled(false);
            player.LockCursor(false);
            Time.timeScale = 0f;
        }

        RefreshSensitivity();
    }

    void Awake()
    {
        BuildInterface();
        ShowStart();
    }

    void Update()
    {
        if (!started)
        {
            if (Keyboard.current != null &&
                (Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                 Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                StartGame();
            }

            return;
        }

        bool controlPressed = Keyboard.current != null &&
            (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame);

        if (controlPressed)
            ToggleSensitivityPanel();

        if (sensitivityOpen)
            UpdateSensitivityPulse();

        if (!sensitivityOpen || player == null)
            return;

        float delta = 0f;
        if (Mouse.current != null)
            delta += Mouse.current.scroll.ReadValue().y * 0.0015f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.equalsKey.wasPressedThisFrame || Keyboard.current.numpadPlusKey.wasPressedThisFrame)
                delta += 0.1f;

            if (Keyboard.current.minusKey.wasPressedThisFrame || Keyboard.current.numpadMinusKey.wasPressedThisFrame)
                delta -= 0.1f;
        }

        if (Mathf.Abs(delta) > 0.0001f)
        {
            player.SetMouseSensitivity(player.mouseSensitivity + delta);
            RefreshSensitivity();
        }
    }

    void StartGame()
    {
        started = true;
        Time.timeScale = 1f;

        if (player != null)
        {
            player.SetControlsEnabled(true);
            player.LockCursor(true);
        }

        startGroup.alpha = 0f;
        startGroup.interactable = false;
        startGroup.blocksRaycasts = false;
        SetSensitivityPanelOpen(false);
    }

    void ShowStart()
    {
        started = false;
        startGroup.alpha = 1f;
        startGroup.interactable = true;
        startGroup.blocksRaycasts = true;
        SetSensitivityPanelOpen(false);
    }

    void ToggleSensitivityPanel()
    {
        SetSensitivityPanelOpen(!sensitivityOpen);
    }

    void SetSensitivityPanelOpen(bool open)
    {
        sensitivityOpen = open;
        sensitivityGroup.alpha = open ? 1f : 0f;
        sensitivityGroup.interactable = open;
        sensitivityGroup.blocksRaycasts = open;

        if (player != null && started)
        {
            player.SetControlsEnabled(!open);
            player.LockCursor(!open);
        }

        if (open)
            UpdateSensitivityPulse();
    }

    void UpdateSensitivityPulse()
    {
        float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 5f) * 0.22f;
        sensitivityPulse.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, pulse);
    }

    void AdjustSensitivity(float amount)
    {
        if (player == null)
            return;

        player.SetMouseSensitivity(player.mouseSensitivity + amount);
        RefreshSensitivity();
    }

    void RefreshSensitivity()
    {
        if (player == null || sensitivityFill == null)
            return;

        float normalized = Mathf.InverseLerp(player.minMouseSensitivity, player.maxMouseSensitivity, player.mouseSensitivity);
        sensitivityFill.fillAmount = normalized;
        sensitivityFill.color = Color.Lerp(HotColor, AccentColor, normalized);
        sensitivityValueText.text = player.mouseSensitivity.ToString("0.0");
        sensitivityStateText.text = normalized < 0.35f ? "LOW" : normalized > 0.72f ? "HIGH" : "BALANCED";
    }

    void BuildInterface()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        RectTransform startOverlay = CreateRect("Start Overlay", transform, Vector2.zero);
        startOverlay.anchorMin = Vector2.zero;
        startOverlay.anchorMax = Vector2.one;
        startOverlay.offsetMin = Vector2.zero;
        startOverlay.offsetMax = Vector2.zero;
        startGroup = startOverlay.gameObject.AddComponent<CanvasGroup>();

        Image background = startOverlay.gameObject.AddComponent<Image>();
        background.color = BackgroundColor;

        RectTransform startPanel = CreateRect("Start Panel", startOverlay, new Vector2(540f, 256f));
        startPanel.anchorMin = new Vector2(0.5f, 0.5f);
        startPanel.anchorMax = new Vector2(0.5f, 0.5f);
        startPanel.pivot = new Vector2(0.5f, 0.5f);
        startPanel.anchoredPosition = Vector2.zero;

        Image panelImage = startPanel.gameObject.AddComponent<Image>();
        panelImage.color = PanelColor;

        Outline outline = startPanel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.5f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateBar("Start Top Line", startPanel, new Vector2(480f, 3f), new Vector2(30f, -24f), AccentColor, new Vector2(0f, 1f));
        CreateBar("Start Side Line", startPanel, new Vector2(4f, 168f), new Vector2(30f, -52f), AccentColor, new Vector2(0f, 1f));

        Text title = CreateText("Start Title", startPanel, "GRAVITY LABYRINTHE", 34, FontStyle.Bold, TextColor);
        title.rectTransform.anchoredPosition = new Vector2(52f, -58f);
        title.rectTransform.sizeDelta = new Vector2(430f, 42f);

        Text subtitle = CreateText("Start Subtitle", startPanel, "SYSTEM ONLINE  |  SHIFT GRAVITY TO ESCAPE", 13, FontStyle.Bold, MutedTextColor);
        subtitle.rectTransform.anchoredPosition = new Vector2(54f, -105f);
        subtitle.rectTransform.sizeDelta = new Vector2(430f, 22f);

        Button startButton = CreateButton("Start Button", startPanel, "START", new Vector2(182f, 52f), new Vector2(54f, -158f));
        startButton.onClick.AddListener(StartGame);

        Text hint = CreateText("Start Hint", startPanel, "ENTER / SPACE", 12, FontStyle.Bold, MutedTextColor);
        hint.rectTransform.anchoredPosition = new Vector2(260f, -174f);
        hint.rectTransform.sizeDelta = new Vector2(180f, 20f);

        RectTransform sensitivityPanel = CreateRect("Sensitivity Panel", transform, new Vector2(360f, 132f));
        sensitivityPanel.anchorMin = new Vector2(1f, 0f);
        sensitivityPanel.anchorMax = new Vector2(1f, 0f);
        sensitivityPanel.pivot = new Vector2(1f, 0f);
        sensitivityPanel.anchoredPosition = new Vector2(-24f, 24f);
        sensitivityGroup = sensitivityPanel.gameObject.AddComponent<CanvasGroup>();
        sensitivityGroup.alpha = 0f;
        sensitivityGroup.interactable = false;
        sensitivityGroup.blocksRaycasts = false;

        Image sensitivityImage = sensitivityPanel.gameObject.AddComponent<Image>();
        sensitivityImage.color = PanelColor;

        Outline sensitivityOutline = sensitivityPanel.gameObject.AddComponent<Outline>();
        sensitivityOutline.effectColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.45f);
        sensitivityOutline.effectDistance = new Vector2(1.5f, -1.5f);

        sensitivityPulse = CreateBar("Sensitivity Pulse", sensitivityPanel, new Vector2(4f, 98f), new Vector2(16f, -18f), AccentColor, new Vector2(0f, 1f));

        Text sensitivityTitle = CreateText("Sensitivity Title", sensitivityPanel, "MOUSE SENSITIVITY", 13, FontStyle.Bold, MutedTextColor);
        sensitivityTitle.rectTransform.anchoredPosition = new Vector2(32f, -19f);
        sensitivityTitle.rectTransform.sizeDelta = new Vector2(170f, 20f);

        sensitivityValueText = CreateText("Sensitivity Value", sensitivityPanel, "2.0", 28, FontStyle.Bold, TextColor);
        sensitivityValueText.alignment = TextAnchor.MiddleRight;
        sensitivityValueText.rectTransform.anchoredPosition = new Vector2(254f, -16f);
        sensitivityValueText.rectTransform.sizeDelta = new Vector2(74f, 34f);

        sensitivityStateText = CreateText("Sensitivity State", sensitivityPanel, "BALANCED", 11, FontStyle.Bold, MutedTextColor);
        sensitivityStateText.rectTransform.anchoredPosition = new Vector2(32f, -48f);
        sensitivityStateText.rectTransform.sizeDelta = new Vector2(130f, 18f);

        RectTransform track = CreateRect("Sensitivity Track", sensitivityPanel, new Vector2(296f, 6f));
        track.anchorMin = new Vector2(0f, 0f);
        track.anchorMax = new Vector2(0f, 0f);
        track.pivot = new Vector2(0f, 0f);
        track.anchoredPosition = new Vector2(32f, 18f);

        Image trackImage = track.gameObject.AddComponent<Image>();
        trackImage.color = new Color(0.08f, 0.22f, 0.27f, 0.85f);

        RectTransform fill = CreateRect("Sensitivity Fill", track, Vector2.zero);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        sensitivityFill = fill.gameObject.AddComponent<Image>();
        sensitivityFill.type = Image.Type.Filled;
        sensitivityFill.fillMethod = Image.FillMethod.Horizontal;
        sensitivityFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        sensitivityFill.fillAmount = 0f;

        Button decreaseButton = CreateButton("Sensitivity Decrease", sensitivityPanel, "-", new Vector2(48f, 34f), new Vector2(32f, -76f));
        decreaseButton.onClick.AddListener(() => AdjustSensitivity(-0.1f));

        Button increaseButton = CreateButton("Sensitivity Increase", sensitivityPanel, "+", new Vector2(48f, 34f), new Vector2(92f, -76f));
        increaseButton.onClick.AddListener(() => AdjustSensitivity(0.1f));

        Text closeHint = CreateText("Sensitivity Close Hint", sensitivityPanel, "CTRL TO CLOSE", 11, FontStyle.Bold, MutedTextColor);
        closeHint.alignment = TextAnchor.MiddleRight;
        closeHint.rectTransform.anchoredPosition = new Vector2(178f, -83f);
        closeHint.rectTransform.sizeDelta = new Vector2(150f, 18f);
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    Button CreateButton(string objectName, Transform parent, string label, Vector2 size, Vector2 position)
    {
        RectTransform rect = CreateRect(objectName, parent, size);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0f, 0.36f, 0.47f, 0.9f);

        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0f, 0.62f, 0.78f, 1f);
        colors.pressedColor = new Color(0f, 0.9f, 1f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        Text text = CreateText("Label", rect, label, 18, FontStyle.Bold, TextColor);
        text.alignment = TextAnchor.MiddleCenter;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;

        return button;
    }

    RectTransform CreateRect(string objectName, Transform parent, Vector2 size)
    {
        GameObject child = new GameObject(objectName);
        child.transform.SetParent(parent, false);
        RectTransform rect = child.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        return rect;
    }

    Image CreateBar(string objectName, Transform parent, Vector2 size, Vector2 position, Color color, Vector2 pivot)
    {
        RectTransform rect = CreateRect(objectName, parent, size);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = pivot;
        rect.anchoredPosition = position;

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    Text CreateText(string objectName, Transform parent, string content, int size, FontStyle style, Color color)
    {
        RectTransform rect = CreateRect(objectName, parent, Vector2.zero);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);

        Text text = rect.gameObject.AddComponent<Text>();
        text.font = GetFont();
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    Font GetFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return font;
    }
}
