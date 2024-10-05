using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using TMPro;

public class PowerRechargeTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private const string POWER_CURRENCY_CODE = "PW";
    private DateTime nextRechargeTime;
    private int currentPowerValue;

    private void Start()
    {
        FetchVirtualCurrencyRechargeTime();
        InvokeRepeating(nameof(UpdateTimerDisplay), 0f, 1f);
    }

    private void FetchVirtualCurrencyRechargeTime()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result => {
                if (result.VirtualCurrencyRechargeTimes.TryGetValue(POWER_CURRENCY_CODE, out var rechargeTime))
                {
                    nextRechargeTime = rechargeTime.SecondsToRecharge > 0
                        ? DateTime.Now.AddSeconds(rechargeTime.SecondsToRecharge)
                        : DateTime.Now;

                    // Store the current power value
                    if (result.VirtualCurrency.TryGetValue(POWER_CURRENCY_CODE, out int powerValue))
                    {
                        currentPowerValue = powerValue;
                    }
                    else
                    {
                        Debug.LogWarning($"Power currency {POWER_CURRENCY_CODE} not found in inventory.");
                    }

                    UpdateTimerDisplay();
                }
                else
                {
                    Debug.LogWarning($"Recharge time for {POWER_CURRENCY_CODE} not found.");
                }
            },
            error => {
                Debug.LogError($"Failed to get user inventory: {error.ErrorMessage}");
            }
        );
    }

    private void UpdateTimerDisplay()
    {
        if (currentPowerValue >= 30)
        {
            timerText.text = "0:00";
        }
        else
        {
            TimeSpan timeLeft = nextRechargeTime - DateTime.Now;
            if (timeLeft.TotalSeconds > 0)
            {
                timerText.text = $"Next Power in: {timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                timerText.text = "Power Ready!";
                FetchVirtualCurrencyRechargeTime(); // Refresh the recharge time
            }
        }
    }
}