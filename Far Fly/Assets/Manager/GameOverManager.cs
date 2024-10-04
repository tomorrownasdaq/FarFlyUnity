using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI goldRewardText;
    [SerializeField] private TextMeshProUGUI diamondRewardText;
    [SerializeField] private Button menuButton;

    [Header("Settings")]
    public string menuSceneName = "StageScene";
    [SerializeField] private float goldMultiplier = 0.1f;
    [SerializeField] private float diamondMultiplier = 0.01f;

    private void Awake()
    {
        Debug.Log("GameOverManager Awake method called.");
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad 제거
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("GameOverManager instance created.");
        }
        else if (Instance != this)
        {
            Debug.Log("Duplicate GameOverManager instance destroyed.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("GameOverManager Start method called.");
        SetupUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        if (scene.name != menuSceneName)
        {
            HideGameOverPanel();
        }
        SetupUI(); // UI 재설정 추가
    }

    private void SetupUI()
    {
        Debug.Log("Setting up UI...");
        if (gameOverPanel == null)
        {
            Debug.LogError("Game Over Panel is not assigned in the inspector!");
        }
        else
        {
            Debug.Log("Game Over Panel is correctly assigned.");
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(GoToMenu);
            Debug.Log("Menu button listener set up.");
        }
        else
        {
            Debug.LogError("Menu Button is not assigned in the inspector!");
        }
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("Game Over Panel hidden.");
        }
        else
        {
            Debug.LogWarning("Attempted to hide Game Over Panel, but it is not assigned.");
        }
    }

    public void ShowGameOver(float distance)
    {
        Debug.Log($"ShowGameOver called with distance: {distance}");
        if (gameOverPanel != null)
        {
            UpdateDistanceText(distance);
            UpdateRewardTexts(distance);
            AddRewardsToServer(distance);
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over Panel shown.");
        }
        else
        {
            Debug.LogError("Game Over Panel is not assigned! Make sure to assign it in the Inspector.");
            // 추가 디버그 정보
            Debug.LogError($"GameOverManager instance: {(Instance != null ? "exists" : "null")}");
            Debug.LogError($"This instance: {(this == Instance ? "is" : "is not")} the singleton instance");
        }
    }

    private void UpdateDistanceText(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = $"Distance : {distance:F2} m";
            Debug.Log($"Distance text updated: {distanceText.text}");
        }
        else
        {
            Debug.LogWarning("Distance Text is not assigned.");
        }
    }

    private void UpdateRewardTexts(float distance)
    {
        int goldReward = Mathf.FloorToInt(distance * goldMultiplier);
        int diamondReward = Mathf.FloorToInt(distance * diamondMultiplier);

        if (goldRewardText != null)
        {
            goldRewardText.text = $"{goldReward}";
            Debug.Log($"Gold reward text updated: {goldRewardText.text}");
        }
        else
        {
            Debug.LogWarning("Gold Reward Text is not assigned.");
        }

        if (diamondRewardText != null)
        {
            diamondRewardText.text = $"{diamondReward}";
            Debug.Log($"Diamond reward text updated: {diamondRewardText.text}");
        }
        else
        {
            Debug.LogWarning("Diamond Reward Text is not assigned.");
        }
    }

    private void AddRewardsToServer(float distance)
    {
        int goldReward = Mathf.FloorToInt(distance * goldMultiplier);
        int diamondReward = Mathf.FloorToInt(distance * diamondMultiplier);

        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "GL",
            Amount = goldReward
        };
        PlayFabClientAPI.AddUserVirtualCurrency(request, OnAddGoldSuccess, OnAddCurrencyFailure);

        request.VirtualCurrency = "DI";
        request.Amount = diamondReward;
        PlayFabClientAPI.AddUserVirtualCurrency(request, OnAddDiamondsSuccess, OnAddCurrencyFailure);

        Debug.Log($"Attempting to add rewards to server: Gold: {goldReward}, Diamonds: {diamondReward}");
    }

    private void OnAddGoldSuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log($"Successfully added {result.Balance} Gold to the player's account.");
    }

    private void OnAddDiamondsSuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log($"Successfully added {result.Balance} Diamonds to the player's account.");
    }

    private void OnAddCurrencyFailure(PlayFabError error)
    {
        Debug.LogError($"Failed to add currency: {error.ErrorMessage}");
    }

    private void GoToMenu()
    {
        Debug.Log("GoToMenu called.");
        HideGameOverPanel();
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        Debug.Log("GameOverManager OnDestroy called.");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}