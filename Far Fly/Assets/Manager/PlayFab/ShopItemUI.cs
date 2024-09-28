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

    private void Start()
    {
        purchaser = GetComponent<ShopItemPurchaser>();
        if (purchaser == null)
        {
            Debug.LogError("ShopItemPurchaser component not found!");
            return;
        }
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
        else
        {
            Debug.LogError("Buy button is not assigned in the ShopItemUI!");
        }
        purchaser.GetCurrencyBalances();
    }

    public void SetItemInfo(string title, string itemPrice, string imageUrl, string itemId)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
        else
        {
            Debug.LogError("Title Text is not assigned in the ShopItemUI!");
        }

        if (priceText != null)
        {
            priceText.text = itemPrice;
        }
        else
        {
            Debug.LogError("Price Text is not assigned in the ShopItemUI!");
        }

        if (purchaser != null)
        {
            purchaser.SetItemInfo(itemPrice, itemId);
        }
        else
        {
            Debug.LogError("ShopItemPurchaser is null in SetItemInfo!");
        }

        // Note: We're not using imageUrl here, but you might want to use it for loading the image
    }

    public void SetItemImage(Texture2D texture)
    {
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            itemImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("Received null texture for item image");
        }
    }

    private void OnBuyButtonClicked()
    {
        if (int.TryParse(goldText.text, out int currentGold))
        {
            if (purchaser.CanAffordItem(currentGold))
            {
                ShowConfirmationDialog();
            }
            else
            {
                Debug.Log("아이템을 구매하기에 골드가 부족합니다.");
            }
        }
        else
        {
            Debug.LogError("현재 골드 금액을 파싱하는 데 실패했습니다.");
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
                purchaser.PurchaseItem();
                Destroy(dialogInstance);
            };
            dialog.OnCancel += () => {
                Debug.Log("구매가 취소되었습니다.");
                Destroy(dialogInstance);
            };
        }
        else
        {
            Debug.LogError("ConfirmationDialog 컴포넌트를 찾을 수 없습니다.");
            Destroy(dialogInstance);
        }
    }

    public void UpdateGoldText(int amount)
    {
        if (goldText != null)
        {
            goldText.text = amount.ToString();
        }
        else
        {
            Debug.LogWarning("Gold Text UI가 할당되지 않았습니다.");
        }
    }
}