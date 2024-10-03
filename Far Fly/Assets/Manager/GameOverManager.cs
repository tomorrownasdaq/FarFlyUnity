using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    [SerializeField] private GameObject gameOverPanelPrefab;
    public string menuSceneName = "StageScene";

    private GameObject gameOverPanelInstance;
    private Text distanceText;
    private Button menuButton;
    private Canvas parentCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 프리팹을 복제하여 보존
            if (gameOverPanelPrefab != null)
            {
                gameOverPanelPrefab = Instantiate(gameOverPanelPrefab);
                gameOverPanelPrefab.SetActive(false);
                DontDestroyOnLoad(gameOverPanelPrefab);
            }
            else
            {
                Debug.LogError("Game Over Panel Prefab is not assigned in the inspector!");
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != menuSceneName)
        {
            EnsureGameOverPanel();
            HideGameOverPanel();
        }
    }

    private void EnsureGameOverPanel()
    {
        if (gameOverPanelPrefab == null)
        {
            Debug.LogError("Game Over Panel Prefab is null! It might have been destroyed.");
            return;
        }

        if (gameOverPanelInstance == null)
        {
            CreateGameOverPanel();
        }
        else
        {
            SetupCanvasAndParent();
        }
    }

    private void CreateGameOverPanel()
    {
        if (gameOverPanelPrefab != null)
        {
            gameOverPanelInstance = Instantiate(gameOverPanelPrefab);
            DontDestroyOnLoad(gameOverPanelInstance);
            SetupCanvasAndParent();
            SetupUI();
        }
        else
        {
            Debug.LogError("Failed to create Game Over Panel: Prefab is null!");
        }
    }

    private void SetupCanvasAndParent()
    {
        parentCanvas = FindObjectOfType<Canvas>();
        if (parentCanvas == null)
        {
            GameObject canvasObject = new GameObject("MainCanvas");
            parentCanvas = canvasObject.AddComponent<Canvas>();
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (gameOverPanelInstance != null)
        {
            gameOverPanelInstance.transform.SetParent(parentCanvas.transform, false);
        }
    }

    private void SetupUI()
    {
        if (gameOverPanelInstance == null) return;

        distanceText = gameOverPanelInstance.GetComponentInChildren<Text>();
        Button[] buttons = gameOverPanelInstance.GetComponentsInChildren<Button>(true);
        if (buttons.Length >= 1)
        {
            menuButton = buttons[0];
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(GoToMenu);
        }
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanelInstance != null)
        {
            gameOverPanelInstance.SetActive(false);
        }
    }

    public void ShowGameOver(float distance)
    {
        EnsureGameOverPanel();

        if (gameOverPanelInstance != null)
        {
            UpdateDistanceText(distance);
            gameOverPanelInstance.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to create or find GameOverPanel!");
        }
    }

    private void UpdateDistanceText(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = $"Distance traveled: {distance:F2} units";
        }
    }

    private void GoToMenu()
    {
        HideGameOverPanel();
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (gameOverPanelPrefab != null)
        {
            Destroy(gameOverPanelPrefab);
        }
        if (gameOverPanelInstance != null)
        {
            Destroy(gameOverPanelInstance);
        }
    }
}