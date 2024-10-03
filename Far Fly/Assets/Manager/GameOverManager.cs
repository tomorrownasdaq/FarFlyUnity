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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != menuSceneName)
        {
            HideGameOverPanel();
        }
    }

    private void SetupUI()
    {
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(GoToMenu);
        }
    }

    private void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowGameOver(float distance)
    {
        if (gameOverPanel != null)
        {
            UpdateDistanceText(distance);
            UpdateRewardTexts(distance);
            AddRewardsToServer(distance);
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Game Over Panel is not assigned!");
        }
    }

    private void UpdateDistanceText(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = $"Distance traveled: {distance:F2} units";
        }
    }

    private void UpdateRewardTexts(float distance)
    {
        int goldReward = Mathf.FloorToInt(distance * goldMultiplier);
        int diamondReward = Mathf.FloorToInt(distance * diamondMultiplier);

        if (goldRewardText != null)
        {
            goldRewardText.text = $"{goldReward}";
        }

        if (diamondRewardText != null)
        {
            diamondRewardText.text = $"{diamondReward}";
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
        HideGameOverPanel();
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}