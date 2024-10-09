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

    private const int MIN_NAME_LENGTH = 2;
    private const int MAX_NAME_LENGTH = 16;

    void Start()
    {
        goButton.onClick.AddListener(OnGoButtonClick);
        agreementToggle.onValueChanged.AddListener(OnToggleValueChanged);
        displayNameInput.onValueChanged.AddListener(OnInputValueChanged);

        // 초기 버튼 상태 설정
        UpdateGoButtonState();
    }

    void OnInputValueChanged(string value)
    {
        UpdateGoButtonState();
    }

    void OnToggleValueChanged(bool isChecked)
    {
        UpdateGoButtonState();
    }

    void UpdateGoButtonState()
    {
        int nameLength = displayNameInput.text.Length;
        bool isValidLength = nameLength >= MIN_NAME_LENGTH && nameLength <= MAX_NAME_LENGTH;
        goButton.interactable = isValidLength && agreementToggle.isOn;

        if (!isValidLength)
        {
            statusText.text = $"Nickname must be between {MIN_NAME_LENGTH} and {MAX_NAME_LENGTH} characters.";
        }
        else
        {
            statusText.text = "";
        }
    }

    void OnGoButtonClick()
    {
        string displayName = displayNameInput.text;

        if (string.IsNullOrEmpty(displayName))
        {
            statusText.text = "Please enter a display name.";
            return;
        }

        if (displayName.Length < MIN_NAME_LENGTH || displayName.Length > MAX_NAME_LENGTH)
        {
            statusText.text = $"Nickname must be between {MIN_NAME_LENGTH} and {MAX_NAME_LENGTH} characters.";
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
                    statusText.text = "This Nickname is already taken. Please try another.";
                }
                else
                {
                    statusText.text = "Error creating DisplayName. Please try again.";
                }
            }
        );
    }
}