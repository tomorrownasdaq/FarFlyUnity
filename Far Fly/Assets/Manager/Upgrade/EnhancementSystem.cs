using UnityEngine;
using UnityEngine.UI;
using System;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;

[Serializable]
public class EnhancementData
{
    public string title;
    public int initialValue;
    public float multiplier;
    public int currentLevel;
    public string currencyType;
    public TextMeshProUGUI levelText;
    public Button enhanceButton;
}

public class EnhancementSystem : MonoBehaviour
{
    public EnhancementData[] enhancementData;
    public GameObject confirmationPanel;
    public TextMeshProUGUI confirmationPanelText;
    public Button confirmButton;
    public Button cancelButton;
    [SerializeField]
    private PlayFabSpecificCurrencyDisplay currencyDisplayScript;
    private int currentIndex = 0;

    private void Start()
    {
        for (int i = 0; i < enhancementData.Length; i++)
        {
            int index = i;
            enhancementData[i].enhanceButton.onClick.AddListener(() => ShowConfirmationPanel(index));
        }
        confirmButton.onClick.AddListener(Enhance);
        cancelButton.onClick.AddListener(CancelEnhancement);
        LoadEnhancementData();
    }

    private void LoadEnhancementData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnError);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            for (int i = 0; i < enhancementData.Length; i++)
            {
                string key = $"Enhancement_{i}";
                if (result.Data.ContainsKey(key))
                {
                    enhancementData[i].currentLevel = int.Parse(result.Data[key].Value);
                }
            }
        }
        UpdateAllUI();
        if (currencyDisplayScript != null)
        {
            currencyDisplayScript.SyncCurrency();
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab error: {error.ErrorMessage}");
    }

    private void UpdateAllUI()
    {
        for (int i = 0; i < enhancementData.Length; i++)
        {
            UpdateUI(i);
        }
    }

    private void UpdateUI(int index)
    {
        EnhancementData data = enhancementData[index];
        data.levelText.text = $"{data.currentLevel}";
    }

    private int CalculatePrice(EnhancementData data)
    {
        return Mathf.RoundToInt(data.initialValue * Mathf.Pow(data.multiplier, data.currentLevel));
    }

    private void ShowConfirmationPanel(int index)
    {
        currentIndex = index;
        confirmationPanel.SetActive(true);
        EnhancementData current = enhancementData[currentIndex];
        int price = CalculatePrice(current);
        confirmationPanelText.text = $"Upgrade {current.title}\nPrice: {price} {current.currencyType}";
    }

    public void Enhance()
    {
        EnhancementData current = enhancementData[currentIndex];
        int price = CalculatePrice(current);
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = current.currencyType,
            Amount = price
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request,
            result => {
                current.currentLevel++;
                UpdateEnhancementData(currentIndex, current.currentLevel);
                UpdateUI(currentIndex);
                confirmationPanel.SetActive(false);
                Debug.Log($"강화 성공! {current.title} 새 레벨: {current.currentLevel}");
                if (currencyDisplayScript != null)
                {
                    currencyDisplayScript.SyncCurrency();
                }
                else
                {
                    Debug.LogWarning("PlayFabSpecificCurrencyDisplay 스크립트가 할당되지 않았습니다.");
                }
            },
            error => {
                Debug.LogError($"강화 실패: {error.ErrorMessage}");
                confirmationPanel.SetActive(false);
            }
        );
    }

    private void UpdateEnhancementData(int index, int level)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { $"Enhancement_{index}", level.ToString() }
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }

    private void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Successfully updated enhancement data");
    }

    public void CancelEnhancement()
    {
        confirmationPanel.SetActive(false);
    }
}