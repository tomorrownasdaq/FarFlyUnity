using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject rankingPanel; // 최상위 RankingPanel
    [SerializeField] private TextMeshProUGUI[] nameTexts; // 이름을 표시할 Text 배열
    [SerializeField] private TextMeshProUGUI[] distanceTexts; // 거리를 표시할 Text 배열
    [SerializeField] private Button showLeaderboardButton;
    [SerializeField] private Button closeLeaderboardButton;

    private void Start()
    {
        Debug.Log("LeaderboardManager Start called");
        InitializePanel();
        SetupButtons();
    }

    private void InitializePanel()
    {
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(false);
            Debug.Log("Ranking panel set to inactive initially");
        }
        else
        {
            Debug.LogError("Ranking panel is not assigned in the inspector");
        }
    }

    private void SetupButtons()
    {
        if (showLeaderboardButton != null)
        {
            showLeaderboardButton.onClick.RemoveAllListeners();
            showLeaderboardButton.onClick.AddListener(ShowLeaderboard);
            Debug.Log("Show leaderboard button listener added");
        }
        else
        {
            Debug.LogError("Show leaderboard button is not assigned in the inspector");
        }

        if (closeLeaderboardButton != null)
        {
            closeLeaderboardButton.onClick.RemoveAllListeners();
            closeLeaderboardButton.onClick.AddListener(CloseLeaderboard);
            Debug.Log("Close leaderboard button listener added");
        }
        else
        {
            Debug.LogError("Close leaderboard button is not assigned in the inspector");
        }
    }

    public void ShowLeaderboard()
    {
        Debug.Log("ShowLeaderboard method called");
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(true);
            Debug.Log("Ranking panel activated");
            FetchLeaderboard();
        }
        else
        {
            Debug.LogError("Cannot show leaderboard: ranking panel is not assigned");
        }
    }

    public void CloseLeaderboard()
    {
        Debug.Log("CloseLeaderboard method called");
        if (rankingPanel != null)
        {
            rankingPanel.SetActive(false);
            Debug.Log("Ranking panel deactivated");
        }
        else
        {
            Debug.LogError("Cannot close leaderboard: ranking panel is not assigned");
        }
    }

    private void FetchLeaderboard()
    {
        Debug.Log("Fetching leaderboard data...");
        var request = new GetLeaderboardRequest
        {
            StatisticName = "Distance",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardFetched, OnLeaderboardFetchFailed);
    }

    private void OnLeaderboardFetched(GetLeaderboardResult result)
    {
        Debug.Log($"Leaderboard data fetched successfully. Total entries: {result.Leaderboard.Count}");

        // Clear existing entries
        for (int i = 0; i < nameTexts.Length; i++)
        {
            if (nameTexts[i] != null) nameTexts[i].text = "";
            if (distanceTexts[i] != null) distanceTexts[i].text = "";
        }

        // Populate new data
        for (int i = 0; i < result.Leaderboard.Count && i < nameTexts.Length && i < distanceTexts.Length; i++)
        {
            var entry = result.Leaderboard[i];
            string playerName = string.IsNullOrEmpty(entry.DisplayName) ? "Anonymous" : entry.DisplayName;
            float distance = entry.StatValue;

            if (nameTexts[i] != null) nameTexts[i].text = playerName;
            if (distanceTexts[i] != null) distanceTexts[i].text = distance.ToString("F2") + " m";

            Debug.Log($"Entry {i + 1}: {playerName} - Distance: {distance:F2} m");
        }

        if (result.Leaderboard.Count == 0)
        {
            Debug.Log("No leaderboard entries found.");
        }
    }

    private void OnLeaderboardFetchFailed(PlayFabError error)
    {
        Debug.LogError($"Failed to fetch leaderboard: {error.ErrorMessage}");
        Debug.LogError($"Error details: {error.ErrorDetails}");
    }
}