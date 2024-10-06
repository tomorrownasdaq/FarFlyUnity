using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class EnhancementPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ConfirmationDialog confirmationDialog;

    private Action onConfirm;
    private int currentCost;
    private string currentCurrency;

    private void Start()
    {
        enhanceButton.onClick.AddListener(ShowConfirmationDialog);
        closeButton.onClick.AddListener(Hide);

        if (confirmationDialog != null)
        {
            confirmationDialog.OnConfirm += OnFinalConfirmClicked;
            confirmationDialog.OnCancel += HideConfirmationDialog;
        }

        Hide();
    }

    public void Show(int cost, string currency, int gold, int diamond, Action onConfirmAction)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        currentCost = cost;
        currentCurrency = currency;
        string costString = currency == "GL" ? $"{cost} 골드" : $"{cost / 1000} 다이아";
        costText.text = $"강화 비용: {costString}\n보유 골드: {gold}\n보유 다이아: {diamond}";
        onConfirm = onConfirmAction;

        bool canEnhance = (currency == "GL" && gold >= cost) || (currency == "DI" && diamond >= cost / 1000);
        enhanceButton.interactable = canEnhance;
    }

    private void ShowConfirmationDialog()
    {
        if (confirmationDialog != null)
        {
            string costString = currentCurrency == "GL" ? $"{currentCost} 골드" : $"{currentCost / 1000} 다이아";
            confirmationDialog.SetMessage($"정말로 {costString}를 사용하여 강화하시겠습니까?");
            confirmationDialog.gameObject.SetActive(true);
        }
    }

    private void HideConfirmationDialog()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.gameObject.SetActive(false);
        }
    }

    private void OnFinalConfirmClicked()
    {
        onConfirm?.Invoke();
        Hide();
    }

    private void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        HideConfirmationDialog();
    }

    private void OnDestroy()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.OnConfirm -= OnFinalConfirmClicked;
            confirmationDialog.OnCancel -= HideConfirmationDialog;
        }
    }
}