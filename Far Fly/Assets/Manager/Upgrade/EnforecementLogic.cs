using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class EnhancementSystem : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI enhancementLevelText;
    public Button enhanceButton;
    public GameObject confirmationPanel;
    public TextMeshProUGUI costText;
    public Button okButton;
    public Button cancelButton;

    private int enhancementLevel;
    private int enhancementCost;

    void Start()
    {
        enhancementLevel = 0;
        UpdateUI();

        enhanceButton.onClick.AddListener(ShowConfirmationPanel);
        okButton.onClick.AddListener(Enhance);
        cancelButton.onClick.AddListener(HideConfirmationPanel);
    }

    void UpdateUI()
    {
        enhancementLevelText.text = $"Lv. {enhancementLevel}";
    }

    int GetCurrentGold()
    {
        if (int.TryParse(goldText.text.Replace("GD: ", ""), out int gold))
        {
            return gold;
        }
        Debug.LogError("골드 텍스트를 파싱할 수 없습니다.");
        return 0;
    }

    void SetCurrentGold(int gold)
    {
        goldText.text = $"{gold}";
    }

    void ShowConfirmationPanel()
    {
        enhancementCost = CalculateEnhancementCost();
        costText.text = $"Cost : {enhancementCost} Gold";
        confirmationPanel.SetActive(true);
    }

    void HideConfirmationPanel()
    {
        confirmationPanel.SetActive(false);
    }

    public void Enhance()
    {
        int currentGold = GetCurrentGold();
        if (currentGold >= enhancementCost)
        {
            SetCurrentGold(currentGold - enhancementCost);
            enhancementLevel++;
            UpdateUI();
            SyncWithPlayFab();
            HideConfirmationPanel();
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
            HideConfirmationPanel();
        }
    }

    int CalculateEnhancementCost()
    {
        return Mathf.RoundToInt(1000 * Mathf.Pow(1.6f, enhancementLevel));
    }

    void SyncWithPlayFab()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"rocket_number", enhancementLevel.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnPlayFabSuccess, OnPlayFabError);
    }

    void OnPlayFabSuccess(UpdateUserDataResult result)
    {
        Debug.Log("PlayFab 동기화 성공");
    }

    void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError($"PlayFab 동기화 실패: {error.ErrorMessage}");
    }
}