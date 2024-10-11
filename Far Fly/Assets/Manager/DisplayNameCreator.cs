using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DisplayNameCreator : MonoBehaviour
{
    public TMP_InputField displayNameInput;
    public Button goButton;
    public TextMeshProUGUI statusText;
    public List<Toggle> agreementToggles;
    private const int MIN_NAME_LENGTH = 3;
    private const int MAX_NAME_LENGTH = 16;
    private readonly Regex alphanumericRegex = new Regex(@"^[a-zA-Z0-9]+$");

    void Start()
    {
        goButton.onClick.AddListener(OnGoButtonClick);
        foreach (var toggle in agreementToggles)
        {
            toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(); });
        }
        displayNameInput.onValueChanged.AddListener(OnInputValueChanged);
        // 초기 버튼 상태 설정
        UpdateGoButtonState();
    }

    void OnInputValueChanged(string value)
    {
        // 영어와 숫자 이외의 문자 입력 방지
        if (!string.IsNullOrEmpty(value) && !alphanumericRegex.IsMatch(value))
        {
            displayNameInput.text = Regex.Replace(value, @"[^a-zA-Z0-9]", "");
        }
        UpdateGoButtonState();
    }

    void OnToggleValueChanged()
    {
        UpdateGoButtonState();
    }

    void UpdateGoButtonState()
    {
        int nameLength = displayNameInput.text.Length;
        bool isValidLength = nameLength >= MIN_NAME_LENGTH && nameLength <= MAX_NAME_LENGTH;
        bool isAlphanumeric = alphanumericRegex.IsMatch(displayNameInput.text);
        bool allTogglesChecked = AreAllTogglesChecked();

        goButton.interactable = isValidLength && isAlphanumeric && allTogglesChecked;

        if (!isValidLength)
        {
            statusText.text = $"Nickname must be between {MIN_NAME_LENGTH} and {MAX_NAME_LENGTH} characters.";
        }
        else if (!isAlphanumeric)
        {
            statusText.text = "Please use only English letters and numbers for your nickname.";
        }
        else if (!allTogglesChecked)
        {
            statusText.text = "Please agree to all terms.";
        }
        else
        {
            statusText.text = "";
        }
    }

    bool AreAllTogglesChecked()
    {
        foreach (var toggle in agreementToggles)
        {
            if (!toggle.isOn)
            {
                return false;
            }
        }
        return true;
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
            statusText.text = $"Nickname must be English between {MIN_NAME_LENGTH} and {MAX_NAME_LENGTH} characters.";
            return;
        }
        if (!alphanumericRegex.IsMatch(displayName))
        {
            statusText.text = "Please use only English letters and numbers for your nickname.";
            return;
        }
        if (!AreAllTogglesChecked())
        {
            statusText.text = "Please agree to all terms.";
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