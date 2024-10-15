using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using GoogleMobileAds.Api;
using System;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI goldRewardText;
    [SerializeField] private TextMeshProUGUI diamondRewardText;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button doubleRewardButton;

    [Header("Settings")]
    public string menuSceneName = "MenuScene";
    [SerializeField] private float goldMultiplier = 0.1f;
    [SerializeField] private float diamondMultiplier = 0.01f;

    private RewardedAd rewardedAd;
    private const string RewardedAdUnitId = "ca-app-pub-6216768731453744~1066542453";
    private float currentDistance;
    private bool rewardDoubled = false;
    private bool isSubscribed = false;

    private void Awake()
    {
        Debug.Log("GameOverManager Awake method called.");
        if (Instance == null)
        {
            Instance = this;
            gameOverPanel.SetActive(false);
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
        InitializeAds();
        CheckSubscriptionStatus();
    }

    private void CheckSubscriptionStatus()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetUserDataSuccess, OnPlayFabError);
    }

    private void OnGetUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.TryGetValue("SubscriptionStatus", out UserDataRecord statusRecord))
        {
            isSubscribed = bool.Parse(statusRecord.Value);
            Debug.Log($"Subscription status: {isSubscribed}");
            UpdateUIBasedOnSubscription();
        }
        else
        {
            Debug.Log("Subscription status not found in User Data.");
            isSubscribed = false;
        }
    }

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError($"PlayFab operation failed: {error.ErrorMessage}");
    }

    private void UpdateUIBasedOnSubscription()
    {
        if (isSubscribed)
        {
            if (doubleRewardButton != null)
            {
                doubleRewardButton.gameObject.SetActive(false);
            }
            rewardDoubled = true;
            UpdateRewardTexts(currentDistance);
        }
    }

    private void InitializeAds()
    {
        Debug.Log("Initializing Ads...");
        MobileAds.Initialize(initStatus => {
            Debug.Log("AdMob SDK initialized");
            LoadRewardedAd();
        });
    }

    private void LoadRewardedAd()
    {
        Debug.Log("Loading Rewarded Ad...");
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(RewardedAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load: " + error);
                    return;
                }

                rewardedAd = ad;
                Debug.Log("Rewarded ad loaded successfully");

                rewardedAd.OnAdFullScreenContentClosed += HandleRewardedAdClosed;
                rewardedAd.OnAdFullScreenContentFailed += HandleRewardedAdFailedToShow;
            });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        if (scene.name != menuSceneName)
        {
            HideGameOverPanel();
        }
        SetupUI();
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

        if (doubleRewardButton != null)
        {
            doubleRewardButton.onClick.RemoveAllListeners();
            doubleRewardButton.onClick.AddListener(ShowRewardedAd);
            Debug.Log("Double Reward button listener set up.");
        }
        else
        {
            Debug.LogError("Double Reward Button is not assigned in the inspector!");
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
        currentDistance = distance;
        rewardDoubled = isSubscribed;  // 구독자는 항상 2배 보상
        if (gameOverPanel != null)
        {
            UpdateDistanceText(distance);
            UpdateRewardTexts(distance);
            gameOverPanel.SetActive(true);
            Debug.Log("Game Over Panel shown.");
            SubmitScoreToLeaderboard(distance);

            // 구독 상태 로그
            Debug.Log($"User subscription status: {(isSubscribed ? "Subscribed" : "Not subscribed")}");

            // 구독자인 경우 즉시 보상 지급
            if (isSubscribed)
            {
                AddRewardsToServer(distance, true);
            }
        }
        else
        {
            Debug.LogError("Game Over Panel is not assigned! Make sure to assign it in the Inspector.");
            Debug.LogError($"GameOverManager instance: {(Instance != null ? "exists" : "null")}");
            Debug.LogError($"This instance: {(this == Instance ? "is" : "is not")} the singleton instance");
        }
    }

    private void SubmitScoreToLeaderboard(float distance)
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            result => {
                var stats = result.Statistics;
                int currentBest = 0;
                foreach (var stat in stats)
                {
                    if (stat.StatisticName == "BestDistance")
                    {
                        currentBest = stat.Value;
                        break;
                    }
                }

                if (Mathf.RoundToInt(distance) > currentBest)
                {
                    UpdatePersonalBestAndLeaderboard(distance);
                }
                else
                {
                    Debug.Log($"Current distance ({distance}) did not beat personal best ({currentBest}). Score not updated.");
                }
            },
            error => {
                Debug.LogError($"Failed to get player statistics: {error.ErrorMessage}");
                // 에러 발생 시 일단 업데이트 시도
                UpdatePersonalBestAndLeaderboard(distance);
            }
        );
    }

    private void UpdatePersonalBestAndLeaderboard(float distance)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "BestDistance",
                    Value = Mathf.RoundToInt(distance)
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result => {
                Debug.Log("Successfully updated personal best and leaderboard");
                // 여기서 UI 업데이트나 추가 작업을 수행할 수 있습니다.
            },
            error => {
                Debug.LogError($"Failed to update statistics: {error.ErrorMessage}");
            }
        );
    }

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfully submitted new high score to the leaderboard");
    }

    private void OnLeaderboardUpdateFailure(PlayFabError error)
    {
        Debug.LogError($"Failed to submit score to leaderboard: {error.ErrorMessage}");
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

        if (rewardDoubled)
        {
            goldReward *= 2;
            diamondReward *= 2;
        }

        if (goldRewardText != null)
        {
            goldRewardText.text = $"{goldReward}";
            if (isSubscribed)
            {
                goldRewardText.text += " (2x Bonus)";
            }
            Debug.Log($"Gold reward text updated: {goldRewardText.text}");
        }
        else
        {
            Debug.LogWarning("Gold Reward Text is not assigned.");
        }

        if (diamondRewardText != null)
        {
            diamondRewardText.text = $"{diamondReward}";
            if (isSubscribed)
            {
                diamondRewardText.text += " (2x Bonus)";
            }
            Debug.Log($"Diamond reward text updated: {diamondRewardText.text}");
        }
        else
        {
            Debug.LogWarning("Diamond Reward Text is not assigned.");
        }
    }

    private void AddRewardsToServer(float distance, bool doubled)
    {
        int goldReward = Mathf.FloorToInt(distance * goldMultiplier);
        int diamondReward = Mathf.FloorToInt(distance * diamondMultiplier);

        if (doubled)
        {
            goldReward *= 2;
            diamondReward *= 2;
        }

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

    private void ShowRewardedAd()
    {
        if (isSubscribed)
        {
            Debug.Log("User is subscribed. Double rewards already applied.");
            return;
        }

        Debug.Log("Attempting to show Rewarded Ad...");
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Rewarded Ad watched successfully. Doubling rewards.");
                rewardDoubled = true;
                UpdateRewardTexts(currentDistance);
            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet.");
            LoadRewardedAd();
        }
    }

    private void HandleRewardedAdClosed()
    {
        Debug.Log("Rewarded Ad closed. Loading next ad.");
        LoadRewardedAd();
    }

    private void HandleRewardedAdFailedToShow(AdError error)
    {
        Debug.LogError("Rewarded ad failed to show: " + error);
        LoadRewardedAd();
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
        if (!isSubscribed)  // 구독자가 아닌 경우에만 여기서 보상 추가
        {
            AddRewardsToServer(currentDistance, rewardDoubled);
        }
        HideGameOverPanel();
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnDestroy()
    {
        Debug.Log("GameOverManager OnDestroy called.");
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
        }
    }
}