using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    public GameObject gameOverCanvasPrefab;
    public string menuSceneName = "StageScene";
    private GameObject gameOverCanvasInstance;
    private Text distanceText;
    private Button retryButton;
    private Button menuButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateGameOverCanvas();
            HideGameOverCanvas();  // Awake에서도 캔버스를 숨깁니다.
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
            if (gameOverCanvasInstance == null)
            {
                CreateGameOverCanvas();
            }
            HideGameOverCanvas(); // 새 씬이 로드될 때마다 캔버스와 버튼들을 숨깁니다.
        }
    }

    private void CreateGameOverCanvas()
    {
        if (gameOverCanvasInstance == null)
        {
            gameOverCanvasInstance = Instantiate(gameOverCanvasPrefab);
            DontDestroyOnLoad(gameOverCanvasInstance);
            SetupUI();
            gameOverCanvasInstance.SetActive(false);  // 생성 직후 캔버스를 숨깁니다.
        }
    }

    private void SetupUI()
    {
        distanceText = gameOverCanvasInstance.GetComponentInChildren<Text>();
        Button[] buttons = gameOverCanvasInstance.GetComponentsInChildren<Button>(true);
        if (buttons.Length >= 2)
        {
            retryButton = buttons[0];
            menuButton = buttons[1];
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryGame);
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(GoToMenu);
        }
        else
        {
            Debug.LogError("Not enough buttons found in the GameOver Canvas!");
        }
    }

    private void HideGameOverCanvas()
    {
        if (gameOverCanvasInstance != null)
        {
            gameOverCanvasInstance.SetActive(false);
            Debug.Log("GameOverCanvas hidden. Active state: " + gameOverCanvasInstance.activeSelf);
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
        
        if (gameOverCanvasInstance == null)
        {
            CreateGameOverCanvas();
        }
        UpdateDistanceText(distance);
        gameOverCanvasInstance.SetActive(true);
        // 게임 오버 시 버튼들을 표시합니다.
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
        HideGameOverCanvas();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        HideGameOverCanvas();
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}