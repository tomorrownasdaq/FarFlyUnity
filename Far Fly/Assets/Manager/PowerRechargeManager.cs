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