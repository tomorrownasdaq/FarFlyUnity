using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class PowerManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private GameObject powerNeedPanel; // Assign this in the inspector
    private const string POWER_CURRENCY_CODE = "PW";
    private const int POWER_COST = 3;

    public void DecreasePower()
    {
        int currentPower = GetCurrentPowerAmount();
        if (currentPower >= POWER_COST)
        {
            SubtractPower(POWER_COST);
            SceneManager.LoadScene("StageScene");
        }
        else
        {
            ShowPowerNeedPanel(POWER_COST - currentPower);
        }
    }

    private void ShowPowerNeedPanel(int powerNeeded)
    {
        if (powerNeedPanel != null)
        {
            powerNeedPanel.SetActive(true);
            TextMeshProUGUI messageText = powerNeedPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = $"Need {powerNeeded} more power.";
            }
            else
            {
                Debug.LogWarning("Message text component not found in PowerNeedPanel.");
            }
        }
        else
        {
            Debug.LogWarning("PowerNeedPanel is not assigned. Cannot show the panel.");
        }
    }

    private int GetCurrentPowerAmount()
    {
        if (powerText != null)
        {
            if (int.TryParse(powerText.text, out int powerAmount))
            {
                return powerAmount;
            }
            else
            {
                Debug.LogWarning("Failed to parse power amount from display text");
            }
        }
        Debug.LogWarning("Power text not found");
        return 0;
    }

    private void SubtractPower(int amount)
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = POWER_CURRENCY_CODE,
            Amount = amount
        };
        Debug.Log($"Attempting to decrease power. Details:\n" +
                  $"Currency: {POWER_CURRENCY_CODE}\n" +
                  $"Amount: {amount}");
        PlayFabClientAPI.SubtractUserVirtualCurrency(request,
            result => {
                Debug.Log($"Power decreased successfully. Details:\n" +
                          $"Currency: {POWER_CURRENCY_CODE}\n" +
                          $"Amount: {amount}\n" +
                          $"New Balance: {result.Balance}");
                // Update power display after successful subtraction
                UpdatePowerDisplay(result.Balance);
            },
            error => {
                Debug.LogError($"Failed to decrease power. Details:\n" +
                               $"Currency: {POWER_CURRENCY_CODE}\n" +
                               $"Error: {error.ErrorMessage}\n" +
                               $"Error Details: {error.ErrorDetails}");
            }
        );
    }

    private void UpdatePowerDisplay(int newBalance)
    {
        if (powerText != null)
        {
            powerText.text = newBalance.ToString();
        }
        else
        {
            Debug.LogWarning("Power text not found. Cannot update display.");
        }
    }

    private void FetchInitialBalance()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result => {
                if (result.VirtualCurrency.TryGetValue(POWER_CURRENCY_CODE, out int balance))
                {
                    UpdatePowerDisplay(balance);
                }
                else
                {
                    Debug.LogWarning($"Power currency {POWER_CURRENCY_CODE} not found in user inventory.");
                }
            },
            error => {
                Debug.LogError($"Failed to get user inventory: {error.ErrorMessage}");
            }
        );
    }

    private void Start()
    {
        FetchInitialBalance();
    }
}