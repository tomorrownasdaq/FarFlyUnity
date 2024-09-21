using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    public GameObject gameOverPanelPrefab;
    public string menuSceneName = "StageScene";

    private GameObject gameOverPanelInstance;
    private Text distanceText;
    private Button retryButton;
    private Button menuButton;
    private Canvas parentCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateGameOverPanel();
            HideGameOverPanel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != menuSceneName)
        {
            if (gameOverPanelInstance == null)
            {
                CreateGameOverPanel();
            }
            HideGameOverPanel();
        }
    }

    private void CreateGameOverPanel()
    {
        if (gameOverPanelInstance == null)
        {
            gameOverPanelInstance = Instantiate(gameOverPanelPrefab);
            DontDestroyOnLoad(gameOverPanelInstance);

            parentCanvas = FindObjectOfType<Canvas>();
            if (parentCanvas == null)
            {
                GameObject canvasObject = new GameObject("MainCanvas");
                parentCanvas = canvasObject.AddComponent<Canvas>();
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObject);
            }

            gameOverPanelInstance.transform.SetParent(parentCanvas.transform, false);

            SetupUI();
            gameOverPanelInstance.SetActive(false);
        }
    }

    private void SetupUI()
    {
        distanceText = gameOverPanelInstance.GetComponentInChildren<Text>();
        Button[] buttons = gameOverPanelInstance.GetComponentsInChildren<Button>(true);
        if (buttons.Length >= 2)
        {
            retryButton = buttons[0];
            menuButton = buttons[1];
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryGame);
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
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }
        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(false);
        }
    }

    public void ShowGameOver(float distance)
    {
        if (gameOverPanelInstance == null)
        {
            CreateGameOverPanel();
        }
        UpdateDistanceText(distance);
        gameOverPanelInstance.SetActive(true);

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
        }
        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(true);
        }
    }

    private void UpdateDistanceText(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = $"Distance traveled: {distance:F2} units";
        }
    }

    private void RetryGame()
    {
        HideGameOverPanel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        HideGameOverPanel();
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}