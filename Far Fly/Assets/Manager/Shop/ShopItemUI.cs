using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI goldText;

    private ShopItemPurchaser purchaser;
    private string itemId;

    private void Awake()
    {
        Debug.Log($"ShopItemUI Awake called for {gameObject.name}");
        InitializePurchaser();
    }

    private void OnEnable()
    {
        Debug.Log($"ShopItemUI OnEnable called for {gameObject.name}");
        if (purchaser == null)
        {
            InitializePurchaser();
        }
        SetupBuyButton();
    }

    private void InitializePurchaser()
    {
        purchaser = GetComponent<ShopItemPurchaser>() ?? gameObject.AddComponent<ShopItemPurchaser>();

        if (purchaser == null)
        {
            Debug.LogError($"Failed to add ShopItemPurchaser to {gameObject.name}!");
            return;
        }

        Debug.Log($"ShopItemPurchaser initialized on {gameObject.name}");
        purchaser.GetCurrencyBalances();
    }

    private void SetupBuyButton()
    {
        if (buyButton == null)
        {
            Debug.LogError($"Buy button is not assigned in the ShopItemUI for {gameObject.name}!");
            return;
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    public void SetItemInfo(string title, string itemPrice, string imageUrl, string id)
    {
        Debug.Log($"SetItemInfo called for {gameObject.name}: {title}, {itemPrice}, {id}");

        SetText(titleText, title, "Title");
        SetText(priceText, itemPrice, "Price");

        itemId = id;

        if (purchaser == null)
        {
            Debug.LogWarning($"ShopItemPurchaser is null in SetItemInfo for {gameObject.name}. Attempting to initialize.");
            InitializePurchaser();
        }

        purchaser?.SetItemInfo(itemPrice, id);

        // Note: We're not using imageUrl here, but you might want to use it for loading the image
    }

    private void SetText(TextMeshProUGUI textComponent, string value, string componentName)
    {
        if (textComponent != null)
        {
            textComponent.text = value;
        }
        else
        {
            Debug.LogError($"{componentName} Text is not assigned in the ShopItemUI for {gameObject.name}!");
        }
    }

    public void SetItemImage(Texture2D texture)
    {
        if (itemImage == null)
        {
            Debug.LogError($"Item Image component is not assigned in the ShopItemUI for {gameObject.name}!");
            return;
        }

        if (texture == null)
        {
            Debug.LogWarning($"Received null texture for item image on {gameObject.name}");
            return;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        itemImage.sprite = sprite;
        Debug.Log($"Item image set for {titleText?.text ?? "unknown item"} on {gameObject.name}");
    }

    private void OnBuyButtonClicked()
    {
        if (goldText == null || !int.TryParse(goldText.text, out int currentGold))
        {
            Debug.LogError($"현재 골드 금액을 파싱하는 데 실패했습니다. ({gameObject.name})");
            return;
        }

        if (purchaser == null || !purchaser.CanAffordItem(currentGold))
        {
            Debug.Log($"아이템 {titleText?.text ?? "unknown item"}을(를) 구매하기에 골드가 부족합니다.");
            return;
        }

        ShowConfirmationDialog();
    }

    private void ShowConfirmationDialog()
    {
        GameObject dialogPrefab = Resources.Load<GameObject>("ConfirmationDialogPrefab");
        if (dialogPrefab == null)
        {
            Debug.LogError("ConfirmationDialogPrefab을 찾을 수 없습니다. Resources 폴더에 있는지 확인하세요.");
            return;
        }

        GameObject dialogInstance = Instantiate(dialogPrefab, transform.root);
        ConfirmationDialog dialog = dialogInstance.GetComponent<ConfirmationDialog>();

        if (dialog == null)
        {
            Debug.LogError($"ConfirmationDialog 컴포넌트를 찾을 수 없습니다. ({gameObject.name})");
            Destroy(dialogInstance);
            return;
        }

        dialog.SetMessage($"Buy {titleText.text} for {priceText.text} Gold?");
        dialog.OnConfirm += ConfirmPurchase;
        dialog.OnCancel += CancelPurchase;

        dialogInstance.SetActive(true);
    }

    private void ConfirmPurchase()
    {
        if (purchaser != null)
        {
            purchaser.PurchaseItem();
        }
        else
        {
            Debug.LogError($"ShopItemPurchaser is null when trying to purchase item on {gameObject.name}.");
        }
    }

    private void CancelPurchase()
    {
        Debug.Log($"구매가 취소되었습니다: {titleText.text} on {gameObject.name}");
    }

    public void UpdateGoldText(int amount)
    {
        if (goldText != null)
        {
            goldText.text = amount.ToString();
            Debug.Log($"Gold amount updated: {amount} for {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"Gold Text UI가 할당되지 않았습니다. ({gameObject.name})");
        }
    }
}