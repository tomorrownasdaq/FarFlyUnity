using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class EnhancementSystemXforce : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI enhancementLevelText;
    public Button enhanceButton;
    public GameObject confirmationPanel;
    public TextMeshProUGUI costText;
    public Button okButton;
    public Button cancelButton;
    public GameObject PowerBuyPanel; // PowerBuyPanel 참조 추가

    private int enhancementLevel;
    private int enhancementCost;

    void Start()
    {
        // 모든 리스너 제거 후 다시 추가
        enhanceButton.onClick.RemoveAllListeners();
        okButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        enhanceButton.onClick.AddListener(OnEnhanceButtonClick);
        okButton.onClick.AddListener(EnhanceUpgrade);
        cancelButton.onClick.AddListener(HideConfirmationPanel);


        // PlayFab에서 데이터 로드
        LoadDataFromPlayFab();
    }

    void OnEnhanceButtonClick()
    {
        ShowConfirmationUpgradePanel();
        // PowerBuyPanel이 열려있다면 닫기
        if (PowerBuyPanel != null)
        {
            PowerBuyPanel.SetActive(false);
        }
    }

    void LoadDataFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnPlayFabError);
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnInventoryReceived, OnPlayFabError);
    }

    void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("rocket_Xforce"))
        {
            if (int.TryParse(result.Data["rocket_Xforce"].Value, out int loadedLevel))
            {
                enhancementLevel = loadedLevel;
                UpdateUI();
            }
        }
    }

    void OnInventoryReceived(GetUserInventoryResult result)
    {
        if (result.VirtualCurrency.ContainsKey("DI"))
        {
            SetCurrentGold(result.VirtualCurrency["DI"]);
        }
        else
        {
            Debug.LogError("GL 가상화폐를 찾을 수 없습니다.");
        }
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

    void ShowConfirmationUpgradePanel()
    {
        enhancementCost = CalculateEnhancementCost();
        costText.text = $"Cost : {enhancementCost} DIAMOND";
        confirmationPanel.SetActive(true);
    }

    void HideConfirmationPanel()
    {
        confirmationPanel.SetActive(false);
    }

    public void EnhanceUpgrade()
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
            VirtualCurrency = "DI",
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
        Debug.Log($"강화 성공! 현재 다이아: {result.Balance}");
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
                {"rocket_Xforce", enhancementLevel.ToString()}
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