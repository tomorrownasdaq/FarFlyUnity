using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabSpecificCurrencyDisplay : MonoBehaviour
{
    [System.Serializable]
    public class CurrencyDisplay
    {
        public string currencyId;
        public TextMeshProUGUI displayText;
    }

    public List<CurrencyDisplay> currencyDisplays = new List<CurrencyDisplay>();

    void Start()
    {
        // PlayFab 로그인 후에 이 메서드를 호출하세요.
        SyncCurrency();
        Debug.Log("연동성공");
    }

    public void SyncCurrency()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            OnGetUserInventorySuccess,
            OnError
        );
    }

    private void OnGetUserInventorySuccess(GetUserInventoryResult result)
    {
        Dictionary<string, int> virtualCurrency = result.VirtualCurrency;

        foreach (var display in currencyDisplays)
        {
            UpdateCurrencyText(display.displayText, display.currencyId, virtualCurrency);
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab Error: {error.ErrorMessage}");
        SetErrorText("Failed to load currency data");
    }

    private void UpdateCurrencyText(TextMeshProUGUI textComponent, string currencyId, Dictionary<string, int> virtualCurrency)
    {
        if (textComponent != null)
        {
            if (virtualCurrency.TryGetValue(currencyId, out int value))
            {
                textComponent.text = value.ToString();
            }
            else
            {
                textComponent.text = "0";
                Debug.LogWarning($"Currency {currencyId} not found in player inventory");
            }
        }
        else
        {
            Debug.LogError($"Text component for {currencyId} is not assigned!");
        }
    }

    private void SetErrorText(string errorMessage)
    {
        foreach (var display in currencyDisplays)
        {
            if (display.displayText != null)
            {
                display.displayText.text = errorMessage;
            }
        }
    }
}