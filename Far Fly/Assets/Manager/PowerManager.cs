using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class PowerManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI powerText;
    private const string POWER_CURRENCY_CODE = "PW";

    public void DecreasePower()
    {
        int currentPower = GetCurrentPowerAmount();
        if (currentPower > 0)
        {
            SubtractPower(1);
            SceneManager.LoadScene("StageScene");
        }
        else
        {
            Debug.Log("Not enough power to decrease.");
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

    // You might want to add a method to fetch the initial balance when the game starts
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

    // Call this method in Start() or when you need to initialize the power display
    private void Start()
    {
        FetchInitialBalance();
    }
}