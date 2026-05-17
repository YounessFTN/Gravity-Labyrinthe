using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartAndSensitivityUI : MonoBehaviour
{
    const string InstanceName = "Start And Sensitivity UI";
    const float GameDuration = 180f;

    static readonly Color BackgroundColor = new Color(0.005f, 0.018f, 0.035f, 0.92f);
    static readonly Color PanelColor      = new Color(0.018f, 0.055f, 0.09f,  0.86f);
    static readonly Color AccentColor     = new Color(0f,    0.86f,  1f,     1f);
    static readonly Color HotColor        = new Color(1f,    0.28f,  0.58f,  1f);
    static readonly Color TextColor       = new Color(0.9f,  0.99f,  1f,     1f);
    static readonly Color MutedTextColor  = new Color(0.46f, 0.73f,  0.8f,   1f);
    static readonly Color DangerColor     = new Color(1f,    0.22f,  0.22f,  1f);
    static readonly Color VictoryColor    = new Color(0.15f, 1f,     0.55f,  1f);

    public static StartAndSensitivityUI Instance { get; private set; }

    PlayerController player;
    CanvasGroup startGroup;
    CanvasGroup sensitivityGroup;
    CanvasGroup timerGroup;
    CanvasGroup victoryGroup;
    CanvasGroup defeatGroup;
    RectTransform timerPanel;
    Image  sensitivityFill;
    Image  sensitivityPulse;
    Text   sensitivityValueText;
    Text   sensitivityStateText;
    Text   timerText;
    Text   elapsedText;
    Text   victoryTimeText;

    bool  started;
    bool  sensitivityOpen;
    float timeRemaining = GameDuration;
    bool  timerRunning;

    // ── Singleton ────────────────────────────────────────────────────────────────

    public static StartAndSensitivityUI EnsureInstance(PlayerController p)
    {
        var existing = FindAnyObjectByType<StartAndSensitivityUI>();
        if (existing != null)
        {
            existing.Bind(p);
            return existing;
        }

        var root = new GameObject(InstanceName);
        DontDestroyOnLoad(root);
        var ui = root.AddComponent<StartAndSensitivityUI>();
        ui.Bind(p);
        return ui;
    }

    public void Bind(PlayerController targetPlayer)
    {
        player = targetPlayer;

        // Réinitialise l'état (utile lors d'un rechargement de scène)
        started       = false;
        timerRunning  = false;
        timeRemaining = GameDuration;

        if (player != null)
        {
            player.SetControlsEnabled(false);
            player.LockCursor(false);
            Time.timeScale = 0f;
        }

        ShowStart();
        RefreshSensitivity();
    }

    // ── Cycle de vie ─────────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
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

        // Décompte du timer
        if (timerRunning && timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
            UpdateTimerDisplay();

            if (timeRemaining <= 0f)
                ShowDefeat();
        }

        bool ctrlPressed = Keyboard.current != null &&
            (Keyboard.current.leftCtrlKey.wasPressedThisFrame ||
             Keyboard.current.rightCtrlKey.wasPressedThisFrame);

        if (ctrlPressed)
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

    // ── Transitions d'état ────────────────────────────────────────────────────────

    void StartGame()
    {
        started       = true;
        timerRunning  = true;
        timeRemaining = GameDuration;
        Time.timeScale = 1f;

        if (player != null)
        {
            player.SetControlsEnabled(true);
            player.LockCursor(true);
        }

        startGroup.alpha          = 0f;
        startGroup.interactable   = false;
        startGroup.blocksRaycasts = false;

        timerGroup.alpha          = 1f;
        timerGroup.interactable   = false;
        timerGroup.blocksRaycasts = false;
        timerPanel.SetAsLastSibling();

        victoryGroup.alpha          = 0f;
        victoryGroup.interactable   = false;
        victoryGroup.blocksRaycasts = false;

        defeatGroup.alpha          = 0f;
        defeatGroup.interactable   = false;
        defeatGroup.blocksRaycasts = false;

        UpdateTimerDisplay();
        SetSensitivityPanelOpen(false);
    }

    void ShowStart()
    {
        started      = false;
        timerRunning = false;

        startGroup.alpha          = 1f;
        startGroup.interactable   = true;
        startGroup.blocksRaycasts = true;

        timerGroup.alpha          = 0f;
        timerGroup.interactable   = false;
        timerGroup.blocksRaycasts = false;

        victoryGroup.alpha          = 0f;
        victoryGroup.interactable   = false;
        victoryGroup.blocksRaycasts = false;

        defeatGroup.alpha          = 0f;
        defeatGroup.interactable   = false;
        defeatGroup.blocksRaycasts = false;

        SetSensitivityPanelOpen(false);
    }

    public void ShowVictory()
    {
        Debug.Log($"[StartAndSensitivityUI] ShowVictory() appelé. victoryGroup null={victoryGroup == null}");
        timerRunning = false;

        if (player != null)
        {
            player.SetControlsEnabled(false);
            player.LockCursor(false);
        }

        int totalSec = Mathf.CeilToInt(Mathf.Max(0f, timeRemaining));
        int min = totalSec / 60;
        int sec = totalSec % 60;
        if (victoryTimeText != null)
            victoryTimeText.text = $"TEMPS RESTANT  —  {min:D2}:{sec:D2}";

        victoryGroup.alpha          = 1f;
        victoryGroup.interactable   = true;
        victoryGroup.blocksRaycasts = true;
        victoryGroup.transform.SetAsLastSibling();
        Debug.Log("[StartAndSensitivityUI] victoryGroup rendu visible (alpha=1).");
    }

    void ShowDefeat()
    {
        timerRunning = false;

        if (player != null)
        {
            player.SetControlsEnabled(false);
            player.LockCursor(false);
        }

        timerGroup.alpha          = 0f;
        timerGroup.interactable   = false;
        timerGroup.blocksRaycasts = false;

        defeatGroup.alpha          = 1f;
        defeatGroup.interactable   = true;
        defeatGroup.blocksRaycasts = true;
        defeatGroup.transform.SetAsLastSibling();
    }

    void RestartGame()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[StartAndSensitivityUI] RestartGame → rechargement de '{sceneName}'");
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ── Timer ─────────────────────────────────────────────────────────────────────

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int totalSec = Mathf.CeilToInt(Mathf.Max(0f, timeRemaining));
        timerText.text = $"{totalSec / 60:D2}:{totalSec % 60:D2}";

        if (elapsedText != null)
        {
            int elapsedSec = Mathf.FloorToInt(Mathf.Max(0f, GameDuration - timeRemaining));
            elapsedText.text = $"ECOULE {elapsedSec / 60:D2}:{elapsedSec % 60:D2}";
        }

        timerText.color = timeRemaining <= 30f
            ? Color.Lerp(DangerColor, new Color(1f, 0.6f, 0.6f), 0.5f + Mathf.Sin(Time.unscaledTime * 6f) * 0.5f)
            : TextColor;
    }

    // ── Sensibilité ───────────────────────────────────────────────────────────────

    void ToggleSensitivityPanel() => SetSensitivityPanelOpen(!sensitivityOpen);

    void SetSensitivityPanelOpen(bool open)
    {
        sensitivityOpen = open;
        sensitivityGroup.alpha          = open ? 1f : 0f;
        sensitivityGroup.interactable   = open;
        sensitivityGroup.blocksRaycasts = open;

        if (player != null && started)
        {
            player.SetControlsEnabled(!open);
            player.LockCursor(!open);
        }

        if (open) UpdateSensitivityPulse();
    }

    void UpdateSensitivityPulse()
    {
        float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 5f) * 0.22f;
        sensitivityPulse.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, pulse);
    }

    void AdjustSensitivity(float amount)
    {
        if (player == null) return;
        player.SetMouseSensitivity(player.mouseSensitivity + amount);
        RefreshSensitivity();
    }

    void RefreshSensitivity()
    {
        if (player == null || sensitivityFill == null) return;

        float norm = Mathf.InverseLerp(player.minMouseSensitivity, player.maxMouseSensitivity, player.mouseSensitivity);
        sensitivityFill.fillAmount = norm;
        sensitivityFill.color = Color.Lerp(HotColor, AccentColor, norm);
        sensitivityValueText.text = player.mouseSensitivity.ToString("0.0");
        sensitivityStateText.text = norm < 0.35f ? "LOW" : norm > 0.72f ? "HIGH" : "BALANCED";
    }

    // ── Construction de l'interface ───────────────────────────────────────────────

    void BuildInterface()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();

        BuildStartUI();
        BuildSensitivityUI();
        BuildTimerUI();
        BuildVictoryUI();
        BuildDefeatUI();
    }

    void BuildStartUI()
    {
        var startOverlay = CreateFullRect("Start Overlay", transform);
        startGroup = startOverlay.gameObject.AddComponent<CanvasGroup>();
        startOverlay.gameObject.AddComponent<Image>().color = BackgroundColor;

        var startPanel = CreateRect("Start Panel", startOverlay, new Vector2(540f, 256f));
        startPanel.anchorMin        = new Vector2(0.5f, 0.5f);
        startPanel.anchorMax        = new Vector2(0.5f, 0.5f);
        startPanel.pivot            = new Vector2(0.5f, 0.5f);
        startPanel.anchoredPosition = Vector2.zero;

        startPanel.gameObject.AddComponent<Image>().color = PanelColor;
        AddOutline(startPanel.gameObject, AccentColor, 0.5f);

        CreateBar("Start Top Line",  startPanel, new Vector2(480f, 3f),   new Vector2(30f, -24f),  AccentColor, new Vector2(0f, 1f));
        CreateBar("Start Side Line", startPanel, new Vector2(4f,   168f), new Vector2(30f, -52f),  AccentColor, new Vector2(0f, 1f));

        var title = CreateText("Start Title", startPanel, "GRAVITY LABYRINTHE", 34, FontStyle.Bold, TextColor);
        title.rectTransform.anchoredPosition = new Vector2(52f, -58f);
        title.rectTransform.sizeDelta        = new Vector2(430f, 42f);

        var subtitle = CreateText("Start Subtitle", startPanel, "SYSTEM ONLINE  |  SHIFT GRAVITY TO ESCAPE", 13, FontStyle.Bold, MutedTextColor);
        subtitle.rectTransform.anchoredPosition = new Vector2(54f, -105f);
        subtitle.rectTransform.sizeDelta        = new Vector2(430f, 22f);

        var startBtn = CreateButton("Start Button", startPanel, "START", new Vector2(182f, 52f), new Vector2(54f, -158f));
        startBtn.onClick.AddListener(StartGame);

        var hint = CreateText("Start Hint", startPanel, "ENTER / SPACE", 12, FontStyle.Bold, MutedTextColor);
        hint.rectTransform.anchoredPosition = new Vector2(260f, -174f);
        hint.rectTransform.sizeDelta        = new Vector2(180f, 20f);
    }

    void BuildSensitivityUI()
    {
        var sensitivityPanel = CreateRect("Sensitivity Panel", transform, new Vector2(360f, 132f));
        sensitivityPanel.anchorMin        = new Vector2(1f, 0f);
        sensitivityPanel.anchorMax        = new Vector2(1f, 0f);
        sensitivityPanel.pivot            = new Vector2(1f, 0f);
        sensitivityPanel.anchoredPosition = new Vector2(-24f, 24f);

        sensitivityGroup = sensitivityPanel.gameObject.AddComponent<CanvasGroup>();
        sensitivityGroup.alpha          = 0f;
        sensitivityGroup.interactable   = false;
        sensitivityGroup.blocksRaycasts = false;

        sensitivityPanel.gameObject.AddComponent<Image>().color = PanelColor;
        AddOutline(sensitivityPanel.gameObject, AccentColor, 0.45f);

        sensitivityPulse = CreateBar("Sensitivity Pulse", sensitivityPanel, new Vector2(4f, 98f), new Vector2(16f, -18f), AccentColor, new Vector2(0f, 1f));

        var sensitivityTitle = CreateText("Sensitivity Title", sensitivityPanel, "MOUSE SENSITIVITY", 13, FontStyle.Bold, MutedTextColor);
        sensitivityTitle.rectTransform.anchoredPosition = new Vector2(32f, -19f);
        sensitivityTitle.rectTransform.sizeDelta        = new Vector2(170f, 20f);

        sensitivityValueText = CreateText("Sensitivity Value", sensitivityPanel, "2.0", 28, FontStyle.Bold, TextColor);
        sensitivityValueText.alignment = TextAnchor.MiddleRight;
        sensitivityValueText.rectTransform.anchoredPosition = new Vector2(254f, -16f);
        sensitivityValueText.rectTransform.sizeDelta        = new Vector2(74f, 34f);

        sensitivityStateText = CreateText("Sensitivity State", sensitivityPanel, "BALANCED", 11, FontStyle.Bold, MutedTextColor);
        sensitivityStateText.rectTransform.anchoredPosition = new Vector2(32f, -48f);
        sensitivityStateText.rectTransform.sizeDelta        = new Vector2(130f, 18f);

        var track = CreateRect("Sensitivity Track", sensitivityPanel, new Vector2(296f, 6f));
        track.anchorMin        = new Vector2(0f, 0f);
        track.anchorMax        = new Vector2(0f, 0f);
        track.pivot            = new Vector2(0f, 0f);
        track.anchoredPosition = new Vector2(32f, 18f);
        track.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.22f, 0.27f, 0.85f);

        var fill = CreateRect("Sensitivity Fill", track, Vector2.zero);
        fill.anchorMin  = Vector2.zero;
        fill.anchorMax  = Vector2.one;
        fill.offsetMin  = Vector2.zero;
        fill.offsetMax  = Vector2.zero;

        sensitivityFill            = fill.gameObject.AddComponent<Image>();
        sensitivityFill.type       = Image.Type.Filled;
        sensitivityFill.fillMethod = Image.FillMethod.Horizontal;
        sensitivityFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        sensitivityFill.fillAmount = 0f;

        var decBtn = CreateButton("Sensitivity Decrease", sensitivityPanel, "-", new Vector2(48f, 34f), new Vector2(32f, -76f));
        decBtn.onClick.AddListener(() => AdjustSensitivity(-0.1f));

        var incBtn = CreateButton("Sensitivity Increase", sensitivityPanel, "+", new Vector2(48f, 34f), new Vector2(92f, -76f));
        incBtn.onClick.AddListener(() => AdjustSensitivity(0.1f));

        var closeHint = CreateText("Sensitivity Close Hint", sensitivityPanel, "CTRL TO CLOSE", 11, FontStyle.Bold, MutedTextColor);
        closeHint.alignment = TextAnchor.MiddleRight;
        closeHint.rectTransform.anchoredPosition = new Vector2(178f, -83f);
        closeHint.rectTransform.sizeDelta        = new Vector2(150f, 18f);
    }

    void BuildTimerUI()
    {
        // Panneau centré en haut pour rendre le temps restant évident pendant le jeu.
        timerPanel = CreateRect("Timer Panel", transform, new Vector2(380f, 148f));
        timerPanel.anchorMin        = new Vector2(0.5f, 1f);
        timerPanel.anchorMax        = new Vector2(0.5f, 1f);
        timerPanel.pivot            = new Vector2(0.5f, 1f);
        timerPanel.anchoredPosition = new Vector2(0f, -18f);

        timerGroup = timerPanel.gameObject.AddComponent<CanvasGroup>();
        timerGroup.alpha          = 0f;
        timerGroup.interactable   = false;
        timerGroup.blocksRaycasts = false;

        timerPanel.gameObject.AddComponent<Image>().color = PanelColor;
        AddOutline(timerPanel.gameObject, AccentColor, 0.5f);

        CreateBar("Timer Side", timerPanel, new Vector2(4f, 120f), new Vector2(18f, -14f), AccentColor, new Vector2(0f, 1f));

        var label = CreateText("Timer Label", timerPanel, "TEMPS DE MISSION", 16, FontStyle.Bold, AccentColor);
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchoredPosition = new Vector2(44f, -18f);
        label.rectTransform.sizeDelta        = new Vector2(296f, 24f);

        timerText = CreateText("Timer Value", timerPanel, "03:00", 52, FontStyle.Bold, TextColor);
        timerText.alignment = TextAnchor.MiddleCenter;
        timerText.rectTransform.anchoredPosition = new Vector2(44f, -50f);
        timerText.rectTransform.sizeDelta        = new Vector2(296f, 62f);

        elapsedText = CreateText("Timer Elapsed", timerPanel, "ECOULE 00:00", 16, FontStyle.Bold, AccentColor);
        elapsedText.alignment = TextAnchor.MiddleCenter;
        elapsedText.rectTransform.anchoredPosition = new Vector2(44f, -118f);
        elapsedText.rectTransform.sizeDelta        = new Vector2(296f, 22f);
    }

    void BuildVictoryUI()
    {
        // Overlay plein écran, caché jusqu'à la victoire
        var victoryOverlay = CreateFullRect("Victory Overlay", transform);
        victoryGroup = victoryOverlay.gameObject.AddComponent<CanvasGroup>();
        victoryGroup.alpha          = 0f;
        victoryGroup.interactable   = false;
        victoryGroup.blocksRaycasts = false;
        victoryOverlay.gameObject.AddComponent<Image>().color = BackgroundColor;

        // Panneau central
        var panel = CreateRect("Victory Panel", victoryOverlay, new Vector2(580f, 360f));
        panel.anchorMin        = new Vector2(0.5f, 0.5f);
        panel.anchorMax        = new Vector2(0.5f, 0.5f);
        panel.pivot            = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;

        panel.gameObject.AddComponent<Image>().color = PanelColor;
        AddOutline(panel.gameObject, VictoryColor, 0.7f);

        CreateBar("Victory Top Line",    panel, new Vector2(520f, 3f),  new Vector2(30f,  -28f),  VictoryColor, new Vector2(0f, 1f));
        CreateBar("Victory Side Line",   panel, new Vector2(4f,   280f), new Vector2(30f,  -56f), VictoryColor, new Vector2(0f, 1f));
        CreateBar("Victory Bottom Line", panel, new Vector2(520f, 3f),  new Vector2(30f, -326f),
            new Color(VictoryColor.r, VictoryColor.g, VictoryColor.b, 0.35f), new Vector2(0f, 1f));

        var missionLabel = CreateText("Mission Label", panel, "MISSION ACCOMPLIE", 13, FontStyle.Bold, MutedTextColor);
        missionLabel.rectTransform.anchoredPosition = new Vector2(52f, -60f);
        missionLabel.rectTransform.sizeDelta        = new Vector2(480f, 20f);

        var title = CreateText("Victory Title", panel, "TU AS GAGNÉ !", 46, FontStyle.Bold, VictoryColor);
        title.rectTransform.anchoredPosition = new Vector2(48f, -100f);
        title.rectTransform.sizeDelta        = new Vector2(480f, 58f);

        var subtitle = CreateText("Victory Subtitle", panel, "Ta famille est sauvée !", 22, FontStyle.Normal, TextColor);
        subtitle.rectTransform.anchoredPosition = new Vector2(52f, -168f);
        subtitle.rectTransform.sizeDelta        = new Vector2(480f, 30f);

        victoryTimeText = CreateText("Victory Time", panel, "", 14, FontStyle.Bold, MutedTextColor);
        victoryTimeText.rectTransform.anchoredPosition = new Vector2(52f, -212f);
        victoryTimeText.rectTransform.sizeDelta        = new Vector2(480f, 22f);

        var replayBtn = CreateButton("Replay Button", panel, "REJOUER", new Vector2(200f, 52f), new Vector2(52f, -270f));
        replayBtn.onClick.AddListener(RestartGame);
    }

    void BuildDefeatUI()
    {
        var defeatOverlay = CreateFullRect("Defeat Overlay", transform);
        defeatGroup = defeatOverlay.gameObject.AddComponent<CanvasGroup>();
        defeatGroup.alpha          = 0f;
        defeatGroup.interactable   = false;
        defeatGroup.blocksRaycasts = false;
        defeatOverlay.gameObject.AddComponent<Image>().color = BackgroundColor;

        var panel = CreateRect("Defeat Panel", defeatOverlay, new Vector2(580f, 340f));
        panel.anchorMin        = new Vector2(0.5f, 0.5f);
        panel.anchorMax        = new Vector2(0.5f, 0.5f);
        panel.pivot            = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;

        panel.gameObject.AddComponent<Image>().color = PanelColor;
        AddOutline(panel.gameObject, DangerColor, 0.7f);

        CreateBar("Defeat Top Line",  panel, new Vector2(520f, 3f),   new Vector2(30f, -28f), DangerColor, new Vector2(0f, 1f));
        CreateBar("Defeat Side Line", panel, new Vector2(4f,   260f), new Vector2(30f, -56f), DangerColor, new Vector2(0f, 1f));

        var missionLabel = CreateText("Defeat Label", panel, "EXPERIENCE TERMINEE", 13, FontStyle.Bold, MutedTextColor);
        missionLabel.rectTransform.anchoredPosition = new Vector2(52f, -60f);
        missionLabel.rectTransform.sizeDelta        = new Vector2(480f, 20f);

        var title = CreateText("Defeat Title", panel, "TEMPS ECOULE", 44, FontStyle.Bold, DangerColor);
        title.rectTransform.anchoredPosition = new Vector2(48f, -100f);
        title.rectTransform.sizeDelta        = new Vector2(480f, 58f);

        var subtitle = CreateText("Defeat Subtitle", panel, "Ashiro a ete capture par le systeme.", 20, FontStyle.Normal, TextColor);
        subtitle.rectTransform.anchoredPosition = new Vector2(52f, -166f);
        subtitle.rectTransform.sizeDelta        = new Vector2(480f, 30f);

        var replayBtn = CreateButton("Defeat Replay Button", panel, "REJOUER", new Vector2(200f, 52f), new Vector2(52f, -250f));
        replayBtn.onClick.AddListener(RestartGame);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    RectTransform CreateFullRect(string objName, Transform parent)
    {
        var rt = CreateRect(objName, parent, Vector2.zero);
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
        return rt;
    }

    RectTransform CreateRect(string objName, Transform parent, Vector2 size)
    {
        var go = new GameObject(objName);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        return rt;
    }

    Image CreateBar(string objName, Transform parent, Vector2 size, Vector2 position, Color color, Vector2 pivot)
    {
        var rt = CreateRect(objName, parent, size);
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = pivot;
        rt.anchoredPosition = position;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    Button CreateButton(string objName, Transform parent, string label, Vector2 size, Vector2 position)
    {
        var rt = CreateRect(objName, parent, size);
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = position;

        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0f, 0.36f, 0.47f, 0.9f);

        var btn    = rt.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = img.color;
        colors.highlightedColor = new Color(0f, 0.62f, 0.78f, 1f);
        colors.pressedColor     = new Color(0f, 0.9f,  1f,    1f);
        colors.selectedColor    = colors.highlightedColor;
        btn.colors = colors;

        var txt = CreateText("Label", rt, label, 18, FontStyle.Bold, TextColor);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.rectTransform.anchorMin  = Vector2.zero;
        txt.rectTransform.anchorMax  = Vector2.one;
        txt.rectTransform.offsetMin  = Vector2.zero;
        txt.rectTransform.offsetMax  = Vector2.zero;

        return btn;
    }

    Text CreateText(string objName, Transform parent, string content, int size, FontStyle style, Color color)
    {
        var rt = CreateRect(objName, parent, Vector2.zero);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);

        var txt = rt.gameObject.AddComponent<Text>();
        txt.font              = GetFont();
        txt.text              = content;
        txt.fontSize          = size;
        txt.fontStyle         = style;
        txt.color             = color;
        txt.alignment         = TextAnchor.MiddleLeft;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow   = VerticalWrapMode.Truncate;
        return txt;
    }

    void AddOutline(GameObject go, Color color, float alpha)
    {
        var outline = go.AddComponent<Outline>();
        outline.effectColor    = new Color(color.r, color.g, color.b, alpha);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    Font GetFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
