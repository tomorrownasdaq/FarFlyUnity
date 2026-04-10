using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public UnityEngine.UI.Image itemImage;
    public Button buyButton;
    public TextMeshProUGUI goldText;
    private ShopItemPurchaser purchaser;

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
        purchaser = GetComponent<ShopItemPurchaser>();
        if (purchaser == null)
        {
            Debug.Log($"ShopItemPurchaser not found on {gameObject.name}, adding it.");
            purchaser = gameObject.AddComponent<ShopItemPurchaser>();
        }

        if (purchaser == null)
        {
            Debug.LogError($"Failed to add ShopItemPurchaser to {gameObject.name}!");
        }
        else
        {
            Debug.Log($"ShopItemPurchaser initialized on {gameObject.name}");
            purchaser.GetCurrencyBalances();
        }
    }

    private void SetupBuyButton()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
        else
        {
            Debug.LogError($"Buy button is not assigned in the ShopItemUI for {gameObject.name}!");
        }
    }

    public void SetItemInfo(string title, string itemPrice, string imageUrl, string itemId)
    {
        Debug.Log($"SetItemInfo called for {gameObject.name}: {title}, {itemPrice}, {itemId}");

        if (titleText != null)
        {
            titleText.text = title;
        }
        else
        {
            Debug.LogError($"Title Text is not assigned in the ShopItemUI for {gameObject.name}!");
        }

        if (priceText != null)
        {
            priceText.text = itemPrice;
        }
        else
        {
            Debug.LogError($"Price Text is not assigned in the ShopItemUI for {gameObject.name}!");
        }

        if (purchaser == null)
        {
            Debug.LogWarning($"ShopItemPurchaser is null in SetItemInfo for {gameObject.name}. Attempting to initialize.");
            InitializePurchaser();
        }

        if (purchaser != null)
        {
            purchaser.SetItemInfo(itemPrice, itemId);
        }
        else
        {
            Debug.LogError($"Failed to initialize ShopItemPurchaser in SetItemInfo for {gameObject.name}!");
        }

        // Note: We're not using imageUrl here, but you might want to use it for loading the image
    }

    public void SetItemImage(Texture2D texture)
    {
        if (itemImage != null)
        {
            if (texture != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                itemImage.sprite = sprite;
                Debug.Log($"Item image set for {titleText?.text ?? "unknown item"} on {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Received null texture for item image on {gameObject.name}");
            }
        }
        else
        {
            Debug.LogError($"Item Image component is not assigned in the ShopItemUI for {gameObject.name}!");
        }
    }

    private void OnBuyButtonClicked()
    {
        if (goldText != null && int.TryParse(goldText.text, out int currentGold))
        {
            if (purchaser != null && purchaser.CanAffordItem(currentGold))
            {
                ShowConfirmationDialog();
            }
            else
            {
                Debug.Log($"아이템 {titleText?.text ?? "unknown item"}을(를) 구매하기에 골드가 부족합니다.");
            }
        }
        else
        {
            Debug.LogError($"현재 골드 금액을 파싱하는 데 실패했습니다. ({gameObject.name})");
        }
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
        dialogInstance.SetActive(true);

        ConfirmationDialog dialog = dialogInstance.GetComponent<ConfirmationDialog>();
        if (dialog != null)
        {
            dialog.SetMessage($"Buy {titleText.text} for {priceText.text} Gold?");
            dialog.OnConfirm += () => {
                if (purchaser != null)
                {
                    purchaser.PurchaseItem();
                }
                else
                {
                    Debug.LogError($"ShopItemPurchaser is null when trying to purchase item on {gameObject.name}.");
                }
                Destroy(dialogInstance);
            };
            dialog.OnCancel += () => {
                Debug.Log($"구매가 취소되었습니다: {titleText.text} on {gameObject.name}");
                Destroy(dialogInstance);
            };
        }
        else
        {
            Debug.LogError($"ConfirmationDialog 컴포넌트를 찾을 수 없습니다. ({gameObject.name})");
            Destroy(dialogInstance);
        }
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