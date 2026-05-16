using UnityEngine;
using UnityEngine.UI;

public class GravityStatusUI : MonoBehaviour
{
    const string InstanceName = "Gravity Status UI";

    static readonly Color PanelColor = new Color(0.015f, 0.045f, 0.08f, 0.82f);
    static readonly Color AccentColor = new Color(0.0f, 0.85f, 1f, 1f);
    static readonly Color HotColor = new Color(1f, 0.26f, 0.55f, 1f);
    static readonly Color TextColor = new Color(0.86f, 0.98f, 1f, 1f);
    static readonly Color MutedTextColor = new Color(0.45f, 0.72f, 0.78f, 1f);

    GravityController controller;
    RectTransform panel;
    Text titleText;
    Text stateText;
    RectTransform characterRig;
    Image cooldownFill;
    Image pulseLine;
    Image characterBoxImage;
    CanvasGroup canvasGroup;
    Quaternion targetCharacterRotation = Quaternion.identity;

    public static GravityStatusUI EnsureInstance()
    {
        GravityStatusUI existing = FindFirstObjectByType<GravityStatusUI>();
        if (existing != null)
            return existing;

        GameObject root = new GameObject(InstanceName);
        DontDestroyOnLoad(root);
        return root.AddComponent<GravityStatusUI>();
    }

    public void Bind(GravityController gravityController)
    {
        controller = gravityController;
    }

    void Awake()
    {
        BuildInterface();
    }

    void Update()
    {
        if (panel == null)
            return;

        float pulse = 0.55f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.18f;
        pulseLine.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, pulse);
        canvasGroup.alpha = controller == null ? 0.92f : 1f;

        if (characterRig != null)
            characterRig.localRotation = Quaternion.Lerp(characterRig.localRotation, targetCharacterRotation, Time.unscaledDeltaTime * 9f);
    }

    public void SetGravity(Vector3 direction, float strength)
    {
        Vector3 axis = GetNearestAxis(direction);
        targetCharacterRotation = Quaternion.Euler(0f, 0f, GetCharacterRotation(axis));
        characterBoxImage.color = new Color(0f, 0.16f, 0.22f, 0.65f);
        cooldownFill.fillAmount = 1f;
    }

    public void SetTransitionProgress(float progress)
    {
        stateText.text = "GRAVITY SHIFT";
        stateText.color = HotColor;
        cooldownFill.color = HotColor;
        cooldownFill.fillAmount = Mathf.Clamp01(progress);
        characterBoxImage.color = Color.Lerp(
            new Color(0.18f, 0.04f, 0.11f, 0.72f),
            new Color(0f, 0.16f, 0.22f, 0.65f),
            Mathf.PingPong(Time.unscaledTime * 4f, 1f));
    }

    public void SetCooldown(float remaining, float duration)
    {
        float readyAmount = duration <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / duration);
        cooldownFill.color = Color.Lerp(HotColor, AccentColor, readyAmount);
        cooldownFill.fillAmount = readyAmount;
        stateText.text = remaining > 0f ? $"RECHARGE {remaining:0.0}S" : "READY";
        stateText.color = remaining > 0f ? MutedTextColor : AccentColor;
    }

    public void SetReady()
    {
        cooldownFill.color = AccentColor;
        cooldownFill.fillAmount = 1f;
        stateText.text = "READY";
        stateText.color = AccentColor;
        characterBoxImage.color = new Color(0f, 0.16f, 0.22f, 0.65f);
    }

    void BuildInterface()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        panel = CreateRect("Panel", transform, new Vector2(390f, 164f));
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.zero;
        panel.pivot = Vector2.zero;
        panel.anchoredPosition = new Vector2(24f, 24f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = PanelColor;

        Outline panelOutline = panel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.5f);
        panelOutline.effectDistance = new Vector2(1.5f, -1.5f);

        CreateBar("Top Accent", panel, new Vector2(360f, 2f), new Vector2(14f, -12f), AccentColor, new Vector2(0f, 1f));
        pulseLine = CreateBar("Pulse Line", panel, new Vector2(4f, 132f), new Vector2(14f, -24f), AccentColor, new Vector2(0f, 1f));

        titleText = CreateText("Title", panel, "GRAVITY CORE", 13, FontStyle.Bold, MutedTextColor);
        titleText.rectTransform.anchoredPosition = new Vector2(30f, -24f);
        titleText.rectTransform.sizeDelta = new Vector2(170f, 20f);

        stateText = CreateText("State", panel, "READY", 12, FontStyle.Bold, AccentColor);
        stateText.alignment = TextAnchor.MiddleRight;
        stateText.rectTransform.anchoredPosition = new Vector2(262f, -24f);
        stateText.rectTransform.sizeDelta = new Vector2(98f, 20f);

        CreateKeyActionRow(panel, "Z", "GRAVITE GAUCHE", new Vector2(30f, -52f));
        CreateKeyActionRow(panel, "C", "GRAVITE DROITE", new Vector2(30f, -84f));
        CreateKeyActionRow(panel, "X", "GRAVITE AVANT", new Vector2(30f, -116f));

        RectTransform characterBox = CreateRect("Character Gravity Box", panel, new Vector2(92f, 86f));
        characterBox.anchorMin = new Vector2(1f, 1f);
        characterBox.anchorMax = new Vector2(1f, 1f);
        characterBox.pivot = new Vector2(1f, 1f);
        characterBox.anchoredPosition = new Vector2(-28f, -52f);

        characterBoxImage = characterBox.gameObject.AddComponent<Image>();
        characterBoxImage.color = new Color(0f, 0.16f, 0.22f, 0.65f);

        Outline characterOutline = characterBox.gameObject.AddComponent<Outline>();
        characterOutline.effectColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.35f);
        characterOutline.effectDistance = new Vector2(1f, -1f);

        CreateMiniCharacter(characterBox);

        RectTransform cooldownTrack = CreateRect("Cooldown Track", panel, new Vector2(330f, 5f));
        cooldownTrack.anchorMin = new Vector2(0f, 0f);
        cooldownTrack.anchorMax = new Vector2(0f, 0f);
        cooldownTrack.pivot = new Vector2(0f, 0f);
        cooldownTrack.anchoredPosition = new Vector2(30f, 16f);

        Image trackImage = cooldownTrack.gameObject.AddComponent<Image>();
        trackImage.color = new Color(0.08f, 0.22f, 0.27f, 0.85f);

        RectTransform fillRect = CreateRect("Cooldown Fill", cooldownTrack, Vector2.zero);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        cooldownFill = fillRect.gameObject.AddComponent<Image>();
        cooldownFill.color = AccentColor;
        cooldownFill.type = Image.Type.Filled;
        cooldownFill.fillMethod = Image.FillMethod.Horizontal;
        cooldownFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        cooldownFill.fillAmount = 1f;
    }

    void CreateKeyActionRow(Transform parent, string key, string action, Vector2 position)
    {
        RectTransform keyBox = CreateRect($"Key {key}", parent, new Vector2(34f, 24f));
        keyBox.anchorMin = new Vector2(0f, 1f);
        keyBox.anchorMax = new Vector2(0f, 1f);
        keyBox.pivot = new Vector2(0f, 1f);
        keyBox.anchoredPosition = position;

        Image keyImage = keyBox.gameObject.AddComponent<Image>();
        keyImage.color = new Color(0f, 0.26f, 0.34f, 0.9f);

        Outline keyOutline = keyBox.gameObject.AddComponent<Outline>();
        keyOutline.effectColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.38f);
        keyOutline.effectDistance = new Vector2(1f, -1f);

        Text keyText = CreateText($"Key {key} Text", keyBox, key, 13, FontStyle.Bold, TextColor);
        keyText.alignment = TextAnchor.MiddleCenter;
        keyText.rectTransform.anchorMin = Vector2.zero;
        keyText.rectTransform.anchorMax = Vector2.one;
        keyText.rectTransform.offsetMin = Vector2.zero;
        keyText.rectTransform.offsetMax = Vector2.zero;

        Text actionText = CreateText($"{action} Text", parent, action, 12, FontStyle.Bold, MutedTextColor);
        actionText.rectTransform.anchoredPosition = position + new Vector2(44f, -2f);
        actionText.rectTransform.sizeDelta = new Vector2(172f, 22f);
    }

    void CreateMiniCharacter(RectTransform parent)
    {
        characterRig = CreateRect("Mini Character", parent, new Vector2(58f, 64f));
        characterRig.anchorMin = new Vector2(0.5f, 0.5f);
        characterRig.anchorMax = new Vector2(0.5f, 0.5f);
        characterRig.pivot = new Vector2(0.5f, 0.5f);
        characterRig.anchoredPosition = new Vector2(0f, -1f);

        CreateBodyPart("Head", characterRig, new Vector2(16f, 16f), new Vector2(0f, 20f), AccentColor);
        CreateBodyPart("Body", characterRig, new Vector2(18f, 28f), new Vector2(0f, -1f), TextColor);
        CreateBodyPart("Left Arm", characterRig, new Vector2(8f, 24f), new Vector2(-17f, 0f), MutedTextColor);
        CreateBodyPart("Right Arm", characterRig, new Vector2(8f, 24f), new Vector2(17f, 0f), MutedTextColor);
        CreateBodyPart("Left Leg", characterRig, new Vector2(8f, 22f), new Vector2(-7f, -27f), AccentColor);
        CreateBodyPart("Right Leg", characterRig, new Vector2(8f, 22f), new Vector2(7f, -27f), AccentColor);

        RectTransform gravityMark = CreateRect("Gravity Mark", parent, new Vector2(6f, 16f));
        gravityMark.anchorMin = new Vector2(0.5f, 0.5f);
        gravityMark.anchorMax = new Vector2(0.5f, 0.5f);
        gravityMark.pivot = new Vector2(0.5f, 0.5f);
        gravityMark.anchoredPosition = new Vector2(0f, -32f);

        Image markImage = gravityMark.gameObject.AddComponent<Image>();
        markImage.color = HotColor;
    }

    void CreateBodyPart(string objectName, Transform parent, Vector2 size, Vector2 position, Color color)
    {
        RectTransform part = CreateRect(objectName, parent, size);
        part.anchorMin = new Vector2(0.5f, 0.5f);
        part.anchorMax = new Vector2(0.5f, 0.5f);
        part.pivot = new Vector2(0.5f, 0.5f);
        part.anchoredPosition = position;

        Image image = part.gameObject.AddComponent<Image>();
        image.color = color;
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

    Vector3 GetNearestAxis(Vector3 direction)
    {
        direction.Normalize();

        Vector3[] axes =
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        Vector3 bestAxis = axes[0];
        float bestDot = Vector3.Dot(direction, bestAxis);

        foreach (Vector3 axis in axes)
        {
            float dot = Vector3.Dot(direction, axis);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestAxis = axis;
            }
        }

        return bestAxis;
    }

    float GetCharacterRotation(Vector3 axis)
    {
        if (axis == Vector3.right) return 90f;
        if (axis == Vector3.left) return -90f;
        if (axis == Vector3.up) return 180f;
        if (axis == Vector3.forward) return 45f;
        if (axis == Vector3.back) return -45f;
        return 0f;
    }
}
