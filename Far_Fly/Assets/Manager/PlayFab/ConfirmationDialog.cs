using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationDialog : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;

    public event Action OnConfirm;
    public event Action OnCancel;

    private bool isInitialized = false;

    private void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmAction);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelAction);
        }

        isInitialized = true;
    }

    private void ConfirmAction()
    {
        OnConfirm?.Invoke();
    }

    private void CancelAction()
    {
        OnCancel?.Invoke();
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    private void OnDisable()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmAction);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(CancelAction);
        }
    }
}