using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("References (auto-created if null)")]
    public Text pointsText;
    public Text waveText;
    public Text livesText;
    public Text enemiesText;
    public Button nextWaveButton;
    public Text nextWaveButtonText;

    private Canvas canvas;
    private GameObject hudPanel;
    private Font defaultFont;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
        {
            Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
            if (allFonts != null && allFonts.Length > 0)
                defaultFont = allFonts[0];
        }

        if (pointsText == null)
            CreateHUD();

        RefreshAll();
    }

    void OnEnable()
    {
        GameEconomy.OnPointsChanged += OnPointsChanged;
        GameEconomy.OnLivesChanged += OnLivesChanged;
        WaveManager.OnWaveStarted += OnWaveStarted;
        WaveManager.OnWaveCompleted += OnWaveCompleted;
        WaveManager.OnEnemiesRemainingChanged += OnEnemiesRemainingChanged;
    }

    void OnDisable()
    {
        GameEconomy.OnPointsChanged -= OnPointsChanged;
        GameEconomy.OnLivesChanged -= OnLivesChanged;
        WaveManager.OnWaveStarted -= OnWaveStarted;
        WaveManager.OnWaveCompleted -= OnWaveCompleted;
        WaveManager.OnEnemiesRemainingChanged -= OnEnemiesRemainingChanged;
    }

    void CreateHUD()
    {
        if (canvas == null) return;

        hudPanel = new GameObject("HUDPanel");
        hudPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = hudPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 1);
        panelRT.anchorMax = new Vector2(1, 1);
        panelRT.pivot = new Vector2(0.5f, 1);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(0, 50);

        Image panelBg = hudPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.6f);
        panelBg.raycastTarget = false;

        HorizontalLayoutGroup layout = hudPanel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 5, 5);
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        pointsText = CreateLabel(hudPanel.transform, "PointsText", "$100", new Color(1f, 0.85f, 0.2f, 1f), 200);
        waveText = CreateLabel(hudPanel.transform, "WaveText", "Wave: --", Color.white, 180);
        enemiesText = CreateLabel(hudPanel.transform, "EnemiesText", "Enemies: 0", Color.white, 180);
        livesText = CreateLabel(hudPanel.transform, "LivesText", "Lives: 20", new Color(1f, 0.4f, 0.4f, 1f), 150);

        // Next Wave button (right side)
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(hudPanel.transform, false);
        LayoutElement spacerLayout = spacer.AddComponent<LayoutElement>();
        spacerLayout.flexibleWidth = 1;
        spacer.AddComponent<RectTransform>();

        GameObject btnGO = new GameObject("NextWaveButton");
        btnGO.transform.SetParent(hudPanel.transform, false);

        RectTransform btnRT = btnGO.AddComponent<RectTransform>();
        LayoutElement btnLayout = btnGO.AddComponent<LayoutElement>();
        btnLayout.preferredWidth = 160;
        btnLayout.preferredHeight = 40;

        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        btnImg.raycastTarget = true;

        nextWaveButton = btnGO.AddComponent<Button>();
        nextWaveButton.targetGraphic = btnImg;

        ColorBlock colors = nextWaveButton.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.75f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.5f, 0.15f, 1f);
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        nextWaveButton.colors = colors;

        nextWaveButton.onClick.AddListener(OnNextWaveClicked);

        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(btnGO.transform, false);

        RectTransform btnTextRT = btnTextGO.AddComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.sizeDelta = Vector2.zero;

        nextWaveButtonText = btnTextGO.AddComponent<Text>();
        nextWaveButtonText.text = "Start Wave";
        if (defaultFont != null) nextWaveButtonText.font = defaultFont;
        nextWaveButtonText.fontSize = 18;
        nextWaveButtonText.alignment = TextAnchor.MiddleCenter;
        nextWaveButtonText.color = Color.white;
        nextWaveButtonText.fontStyle = FontStyle.Bold;
        nextWaveButtonText.raycastTarget = false;
    }

    Text CreateLabel(Transform parent, string name, string initialText, Color color, float width)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        go.AddComponent<RectTransform>();

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;

        Text text = go.AddComponent<Text>();
        text.text = initialText;
        if (defaultFont != null) text.font = defaultFont;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = color;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 14;
        text.resizeTextMaxSize = 22;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(1, -1);

        return text;
    }

    void RefreshAll()
    {
        if (GameEconomy.Instance != null)
        {
            OnPointsChanged(GameEconomy.Instance.CurrentPoints);
            OnLivesChanged(GameEconomy.Instance.CurrentLives);
        }

        if (WaveManager.Instance != null)
        {
            UpdateWaveDisplay();
            UpdateNextWaveButton();
        }
    }

    void OnPointsChanged(int points)
    {
        if (pointsText != null)
            pointsText.text = $"${points}";
    }

    void OnLivesChanged(int lives)
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}";
    }

    void OnWaveStarted(int wave)
    {
        UpdateWaveDisplay();
        UpdateNextWaveButton();
    }

    void OnWaveCompleted(int wave)
    {
        UpdateWaveDisplay();
        UpdateNextWaveButton();
    }

    void OnEnemiesRemainingChanged(int remaining)
    {
        if (enemiesText != null)
            enemiesText.text = $"Enemies: {remaining}";
    }

    void UpdateWaveDisplay()
    {
        if (waveText == null || WaveManager.Instance == null) return;

        var wm = WaveManager.Instance;
        switch (wm.CurrentState)
        {
            case WaveManager.WaveState.WaitingToStart:
                waveText.text = wm.CurrentWaveNumber == 0 ? "Ready!" : $"Wave {wm.CurrentWaveNumber} Clear!";
                break;
            case WaveManager.WaveState.Spawning:
            case WaveManager.WaveState.InProgress:
                waveText.text = $"Wave {wm.CurrentWaveNumber}";
                break;
            case WaveManager.WaveState.WaveComplete:
                waveText.text = $"Wave {wm.CurrentWaveNumber} Clear!";
                break;
        }
    }

    void UpdateNextWaveButton()
    {
        if (nextWaveButton == null || WaveManager.Instance == null) return;

        var wm = WaveManager.Instance;
        bool canStart = wm.CurrentState == WaveManager.WaveState.WaitingToStart
                     || wm.CurrentState == WaveManager.WaveState.WaveComplete;

        nextWaveButton.interactable = canStart;

        if (nextWaveButtonText != null)
        {
            nextWaveButtonText.text = canStart
                ? $"Start Wave {wm.CurrentWaveNumber + 1}"
                : "In Progress...";
        }
    }

    void OnNextWaveClicked()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.StartNextWave();
    }
}
