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
        enhanceButton.onClick.AddListener(ShowConfirmationPanel);
        okButton.onClick.AddListener(Enhance);
        cancelButton.onClick.AddListener(HideConfirmationPanel);
        // PlayFab에서 데이터 로드
        LoadDataFromPlayFab();
    }

    void LoadDataFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnPlayFabError);
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnInventoryReceived, OnPlayFabError);
    }

    void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("rocket_number"))
        {
            if (int.TryParse(result.Data["rocket_number"].Value, out int loadedLevel))
            {
                enhancementLevel = loadedLevel;
                UpdateUI();
            }
        }
    }

    void OnInventoryReceived(GetUserInventoryResult result)
    {
        SetCurrentGold(result.VirtualCurrency["GL"]);
    }

    void UpdateUI()
    {
        enhancementLevelText.text = $"{enhancementLevel}";
    }

    int GetCurrentGold()
    {
        if (int.TryParse(goldText.text, out int gold))
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
        costText.text = $"Cost : {enhancementCost} GD";
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
            SubtractGoldFromPlayFab(enhancementCost);
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
            HideConfirmationPanel();
        }
    }

    void SubtractGoldFromPlayFab(int amount)
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = "GL",
            Amount = amount
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnGoldSubtracted, OnPlayFabError);
    }

    void OnGoldSubtracted(ModifyUserVirtualCurrencyResult result)
    {
        SetCurrentGold(result.Balance);
        enhancementLevel++;
        UpdateUI();
        SyncRocketNumberWithPlayFab();
        HideConfirmationPanel();
        Debug.Log($"강화 성공! 현재 골드: {result.Balance}");
    }

    int CalculateEnhancementCost()
    {
        return Mathf.RoundToInt(1000 * Mathf.Pow(1.6f, enhancementLevel));
    }

    void SyncRocketNumberWithPlayFab()
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