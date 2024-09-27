using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DisplayNameCreator : MonoBehaviour
{
    public TMP_InputField displayNameInput;
    public Button goButton;
    public TextMeshProUGUI statusText;
    public Toggle agreementToggle;

    void Start()
    {
        goButton.onClick.AddListener(OnGoButtonClick);
        agreementToggle.onValueChanged.AddListener(OnToggleValueChanged);

        // 초기 버튼 상태 설정
        goButton.interactable = false;

        // InputField 상태 로깅
        Debug.Log("InputField interactable: " + displayNameInput.interactable);
        Debug.Log("InputField readOnly: " + displayNameInput.readOnly);
    }

    void Update()
    {
        // 매 프레임마다 InputField의 텍스트 로깅
        Debug.Log("Current InputField text: " + displayNameInput.text);
    }

    void OnToggleValueChanged(bool isChecked)
    {
        goButton.interactable = isChecked;
    }

    void OnGoButtonClick()
    {
        string displayName = displayNameInput.text;
        if (string.IsNullOrEmpty(displayName))
        {
            statusText.text = "Please enter a display name.";
            return;
        }

        if (!agreementToggle.isOn)
        {
            statusText.text = "Please agree to the terms.";
            return;
        }

        UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result => {
                Debug.Log("DisplayName updated successfully");
                statusText.text = "DisplayName created successfully!";
                SceneManager.LoadScene("MenuScene");
            },
            error => {
                Debug.LogError(error.GenerateErrorReport());
                if (error.Error == PlayFabErrorCode.NameNotAvailable)
                {
                    statusText.text = "This DisplayName is already taken. Please try another.";
                }
                else
                {
                    statusText.text = "Error creating DisplayName. Please try again.";
                }
            }
        );
    }
}