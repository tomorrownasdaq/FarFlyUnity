using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class RetryManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    private const string POWER_CURRENCY_CODE = "PW";
    private const int RETRY_COST = 3;
    private static string lastMessage = "";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeSceneWithPWCheck());
    }

    private IEnumerator InitializeSceneWithPWCheck()
    {
        yield return null; // Wait for a frame to ensure all objects are initialized

        messageText = FindObjectOfType<TextMeshProUGUI>();

        FetchCurrentPower(power => {
            if (power < RETRY_COST)
            {
                DisplayInsufficientPowerMessage(power);
                // 여기에 PW 부족 시 추가 로직을 구현할 수 있습니다.
                // 예: 스토어로 이동하는 버튼 표시 등
            }
            else
            {
                if (messageText != null)
                {
                    messageText.text = "";
                }
            }
        });

        if (!string.IsNullOrEmpty(lastMessage) && messageText != null)
        {
            messageText.text = lastMessage;
            StartCoroutine(ClearMessageAfterDelay(3f));
        }
    }

    private System.Collections.IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
        {
            messageText.text = "";
        }
        lastMessage = "";
    }

    public void AttemptRetry()
    {
        FetchCurrentPower(power => {
            if (power >= RETRY_COST)
            {
                SubtractPower(RETRY_COST);
            }
            else
            {
                DisplayInsufficientPowerMessage(power);
                // 여기에 PW 부족 시 추가 로직을 구현할 수 있습니다.
                // 예: 스토어로 이동하는 버튼 표시 등
            }
        });
    }

    private void FetchCurrentPower(System.Action<int> callback)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result => {
                if (result.VirtualCurrency.TryGetValue(POWER_CURRENCY_CODE, out int powerAmount))
                {
                    callback(powerAmount);
                }
                else
                {
                    Debug.LogWarning($"Virtual currency {POWER_CURRENCY_CODE} not found.");
                    callback(0);
                }
            },
            error => {
                Debug.LogError($"Failed to get user inventory: {error.ErrorMessage}");
                DisplayErrorMessage("Failed to retrieve power information. Please try again.");
                callback(0);
            }
        );
    }

    private void SubtractPower(int amount)
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = POWER_CURRENCY_CODE,
            Amount = amount
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request,
            result => {
                Debug.Log($"Power decreased successfully. New Balance: {result.Balance}");
                lastMessage = $"Power decreased. New Balance: {result.Balance}";
                SceneManager.LoadScene("Stage1");
            },
            error => {
                Debug.LogError($"Failed to decrease power: {error.ErrorMessage}");
                DisplayErrorMessage("Failed to decrease power. Please try again.");
            }
        );
    }

    private void DisplayInsufficientPowerMessage(int currentPower)
    {
        string message = $"Not enough power to retry. You have {currentPower}, but need {RETRY_COST} power.";
        if (messageText != null)
        {
            messageText.text = message;
        }
        lastMessage = message;
        Debug.LogWarning(message);
        // 여기에 추가적인 로직을 넣을 수 있습니다. 
        // 예: 파워 구매 화면으로 이동하거나 무료 파워를 얻을 수 있는 옵션을 제공
    }

    private void DisplayErrorMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
        lastMessage = message;
        Debug.LogWarning(message);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}