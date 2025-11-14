using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class TowerSelectionUI : MonoBehaviour
{
    [Header("References")]
    public TowerPlacementManager placementManager;
    public Transform buttonParent; // Parent transform for buttons (usually a Panel or Grid)
    
    [Header("UI Prefab")]
    public GameObject towerButtonPrefab; // Optional: prefab for tower buttons
    
    [Header("UI Settings")]
    public Vector2 buttonSize = new Vector2(120, 120);
    public float buttonSpacing = 10f;
    public Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color textColor = Color.white;
    
    private Button[] towerButtons;
    private int currentSelectedIndex = -1;
    private bool buttonsCreated = false;
    private GameObject panelInstance;

    void Start()
    {
        // Delay initialization to ensure TowerPlacementManager is ready
        Invoke(nameof(InitializeUI), 0.1f);
    }

    void InitializeUI()
    {
        if (placementManager == null)
        {
            placementManager = FindFirstObjectByType<TowerPlacementManager>();
        }
        
        if (placementManager == null)
        {
            Debug.LogError("TowerSelectionUI: TowerPlacementManager not found! Please assign it in the Inspector or ensure one exists in the scene.");
            return;
        }
        
        if (placementManager.towerPrefabs == null || placementManager.towerPrefabs.Length == 0)
        {
            Debug.LogWarning("TowerSelectionUI: No tower prefabs assigned in TowerPlacementManager!");
            return;
        }
        
        Debug.Log($"TowerSelectionUI: Found TowerPlacementManager with {placementManager.towerPrefabs.Length} tower prefabs");
        
        if (!buttonsCreated)
        {
            CreateTowerButtons();
        }
    }

    void Update()
    {
        // Retry initialization if buttons weren't created yet
        if (!buttonsCreated)
        {
            if (placementManager != null && placementManager.towerPrefabs != null && placementManager.towerPrefabs.Length > 0)
            {
                if (Time.frameCount % 30 == 0) // Check every 30 frames to avoid spam
                {
                    InitializeUI();
                }
            }
            return;
        }

        // Allow keyboard shortcuts (1, 2, 3, etc.)
        if (placementManager == null || placementManager.towerPrefabs == null) return;
        
        for (int i = 0; i < placementManager.towerPrefabs.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectTower(i);
            }
        }
    }

    void CreateTowerButtons()
    {
        if (placementManager == null || placementManager.towerPrefabs == null || placementManager.towerPrefabs.Length == 0)
        {
            Debug.LogWarning("TowerSelectionUI: Cannot create buttons - no tower prefabs!");
            return;
        }

        // Clean up existing panel and buttons
        if (panelInstance != null)
        {
            if (Application.isPlaying)
                Destroy(panelInstance);
            else
                DestroyImmediate(panelInstance);
            panelInstance = null;
            buttonParent = null;
        }

        // Find or create Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("TowerSelectionUI: No Canvas found! Please create a Canvas first (GameObject → UI → Canvas)");
                return;
            }
        }

        // Create panel if buttonParent is not assigned
        if (buttonParent == null)
        {
            panelInstance = new GameObject("TowerSelectionPanel");
            panelInstance.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRT = panelInstance.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0, 0);
            panelRT.anchorMax = new Vector2(0, 0);
            panelRT.pivot = new Vector2(0, 0);
            panelRT.anchoredPosition = new Vector2(10, 10);
            
            // Calculate panel size based on number of buttons
            float panelHeight = (buttonSize.y + buttonSpacing) * placementManager.towerPrefabs.Length - buttonSpacing + 20;
            panelRT.sizeDelta = new Vector2(buttonSize.x + 20, panelHeight);
            
            Image panelImage = panelInstance.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            buttonParent = panelInstance.transform;
        }

        // Clean up any existing buttons in buttonParent
        if (buttonParent != null)
        {
            for (int i = buttonParent.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(buttonParent.GetChild(i).gameObject);
                else
                    DestroyImmediate(buttonParent.GetChild(i).gameObject);
            }
        }

        // Create buttons array
        towerButtons = new Button[placementManager.towerPrefabs.Length];

        // Create buttons from top to bottom
        for (int i = 0; i < placementManager.towerPrefabs.Length; i++)
        {
            GameObject buttonGO;
            
            if (towerButtonPrefab != null)
            {
                buttonGO = Instantiate(towerButtonPrefab, buttonParent);
                buttonGO.name = "TowerButton_" + i;
            }
            else
            {
                // Create button from scratch
                buttonGO = new GameObject("TowerButton_" + i);
                buttonGO.transform.SetParent(buttonParent, false);
                
                // Add RectTransform
                RectTransform rt = buttonGO.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1); // Anchor to top-left
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.sizeDelta = buttonSize;
                
                // Position from top down
                float yOffset = 10 + (buttonSize.y + buttonSpacing) * i;
                rt.anchoredPosition = new Vector2(10, -yOffset);
                
                // Add Image (button background) - this will be the clickable target
                Image bgImg = buttonGO.AddComponent<Image>();
                bgImg.color = normalColor;
                bgImg.raycastTarget = true; // Make sure background is clickable
                
                // Try to get sprite from tower prefab
                Sprite towerSprite = null;
                if (placementManager.towerPrefabs[i] != null)
                {
                    SpriteRenderer spriteRenderer = placementManager.towerPrefabs[i].GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        towerSprite = spriteRenderer.sprite;
                    }
                }
                
                // Add tower sprite image (if found)
                if (towerSprite != null)
                {
                    GameObject spriteGO = new GameObject("TowerSprite");
                    spriteGO.transform.SetParent(buttonGO.transform, false);
                    
                    RectTransform spriteRT = spriteGO.AddComponent<RectTransform>();
                    spriteRT.anchorMin = new Vector2(0.1f, 0.1f);
                    spriteRT.anchorMax = new Vector2(0.9f, 0.9f);
                    spriteRT.sizeDelta = Vector2.zero;
                    spriteRT.anchoredPosition = Vector2.zero;
                    
                    Image spriteImg = spriteGO.AddComponent<Image>();
                    spriteImg.sprite = towerSprite;
                    spriteImg.preserveAspect = true;
                    spriteImg.raycastTarget = false; // Don't block button clicks - critical!
                }
                
                // Add Button component BEFORE adding text
                Button btn = buttonGO.AddComponent<Button>();
                btn.targetGraphic = bgImg; // Background image is the clickable target
                
                // Create colors for button states
                ColorBlock colors = btn.colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = new Color(normalColor.r + 0.2f, normalColor.g + 0.2f, normalColor.b + 0.2f, 1f);
                colors.pressedColor = selectedColor;
                colors.selectedColor = selectedColor;
                colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                btn.colors = colors;
                
                // Add text label (showing key number at top)
                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform, false);
                
                RectTransform textRT = textGO.AddComponent<RectTransform>();
                textRT.anchorMin = new Vector2(0, 0.7f); // Top portion of button
                textRT.anchorMax = new Vector2(1, 1);
                textRT.sizeDelta = Vector2.zero;
                textRT.anchoredPosition = Vector2.zero;
                
                Text text = textGO.AddComponent<Text>();
                text.text = $"[{i + 1}]";
                // Try to get a valid font - use LegacyRuntime.ttf instead of deprecated Arial.ttf
                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont == null)
                {
                    // Fallback: try to find any font in the project
                    Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
                    if (allFonts != null && allFonts.Length > 0)
                    {
                        defaultFont = allFonts[0];
                    }
                }
                if (defaultFont != null)
                {
                    text.font = defaultFont;
                }
                text.fontSize = 16;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = textColor;
                text.fontStyle = FontStyle.Bold;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 12;
                text.resizeTextMaxSize = 18;
                text.raycastTarget = false; // Don't block button clicks - critical!
                
                // Add shadow/outline for better visibility
                Shadow shadow = textGO.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.8f);
                shadow.effectDistance = new Vector2(1, -1);
            }
            
            // Ensure Button component exists
            Button button = buttonGO.GetComponent<Button>();
            if (button == null)
            {
                button = buttonGO.AddComponent<Button>();
            }
            
            // Make sure button has a target graphic
            if (button.targetGraphic == null)
            {
                Image targetImg = buttonGO.GetComponent<Image>();
                if (targetImg != null)
                {
                    button.targetGraphic = targetImg;
                }
            }
            
            // Ensure all child images/text don't block raycasts (except the background)
            Image[] allImages = buttonGO.GetComponentsInChildren<Image>();
            foreach (Image img in allImages)
            {
                if (img.gameObject == buttonGO)
                {
                    // Main button image - keep raycast enabled
                    img.raycastTarget = true;
                }
                else
                {
                    // Child images (sprites) - disable raycast
                    img.raycastTarget = false;
                }
            }
            
            Text[] allTexts = buttonGO.GetComponentsInChildren<Text>();
            foreach (Text txt in allTexts)
            {
                txt.raycastTarget = false;
            }
            
            // Set up click listener
            int index = i; // Capture for closure
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Debug.Log($"Button {index} clicked!");
                SelectTower(index);
            });
            
            towerButtons[i] = button;
        }
        
        // Select first tower by default
        if (placementManager.towerPrefabs.Length > 0 && placementManager.towerPrefabs[0] != null)
        {
            SelectTower(0);
        }
        
        buttonsCreated = true;
        Debug.Log($"TowerSelectionUI: Successfully created {towerButtons.Length} tower buttons");
    }

    public void SelectTower(int index)
    {
        if (placementManager == null)
        {
            Debug.LogWarning("TowerSelectionUI: PlacementManager is null!");
            return;
        }
        
        if (placementManager.towerPrefabs == null)
        {
            Debug.LogWarning("TowerSelectionUI: Tower prefabs array is null!");
            return;
        }
        
        if (index < 0 || index >= placementManager.towerPrefabs.Length)
        {
            Debug.LogWarning($"TowerSelectionUI: Invalid tower index {index} (valid range: 0-{placementManager.towerPrefabs.Length - 1})");
            return;
        }
        
        if (placementManager.towerPrefabs[index] == null)
        {
            Debug.LogWarning($"TowerSelectionUI: Tower prefab at index {index} is null!");
            return;
        }

        // Update visual selection - reset all buttons to normal color
        if (towerButtons != null)
        {
            for (int i = 0; i < towerButtons.Length; i++)
            {
                if (towerButtons[i] != null)
                {
                    Image img = towerButtons[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = (i == index) ? selectedColor : normalColor;
                    }
                }
            }
        }

        currentSelectedIndex = index;
        placementManager.selectedTowerIndex = index;
        placementManager.SetPlacementMode(true); // Enable placement mode
        placementManager.CreatePreview(); // Refresh preview

        Debug.Log($"Selected tower: {placementManager.towerPrefabs[index].name} (index {index})");
    }

    void OnDestroy()
    {
        // Clean up when destroyed
        if (panelInstance != null && Application.isPlaying)
        {
            Destroy(panelInstance);
        }
    }
}
