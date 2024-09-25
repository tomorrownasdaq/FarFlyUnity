using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public Image itemImage;
    public Button buyButton;
    public TextMeshProUGUI goldText;
    private int price;

    private const string GOLD_CURRENCY_ID = "GL";
    private const string CONFIRMATION_DIALOG_PREFAB_NAME = "ConfirmationDialogPrefab";

    private void Start()
    {
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        GetCurrencyBalances();
    }

    public void SetItemInfo(string title, string itemPrice, string imageUrl)
    {
        titleText.text = title;
        priceText.text = itemPrice;
        // Try to parse the price string to an integer
        if (int.TryParse(itemPrice, out int parsedPrice))
        {
            price = parsedPrice;
        }
        else
        {
            Debug.LogWarning($"Failed to parse price: {itemPrice}");
            price = 0;
        }
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
            if (currentGold >= price)
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
        GameObject dialogPrefab = Resources.Load<GameObject>(CONFIRMATION_DIALOG_PREFAB_NAME);
        if (dialogPrefab == null)
        {
            Debug.LogError($"{CONFIRMATION_DIALOG_PREFAB_NAME}을 찾을 수 없습니다. Resources 폴더에 있는지 확인하세요.");
            return;
        }

        GameObject dialogInstance = Instantiate(dialogPrefab, transform.root);
        dialogInstance.SetActive(true);

        ConfirmationDialog dialog = dialogInstance.GetComponent<ConfirmationDialog>();
        if (dialog != null)
        {
            dialog.SetMessage($"Buy {titleText.text} on {price} Gold");
            dialog.OnConfirm += () => {
                PurchaseItem();
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

    private void PurchaseItem()
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = GOLD_CURRENCY_ID,
            Amount = price
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request,
            result => {
                int newBalance = result.Balance;
                UpdateGoldText(newBalance);
                Debug.Log($"아이템 구매 성공. 새로운 잔액: {newBalance}");
            },
            error => {
                Debug.LogError($"아이템 구매 실패: {error.ErrorMessage}");
            }
        );
    }

    public void GetCurrencyBalances()
    {
        var request = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(request,
            result => {
                int gold = result.VirtualCurrency.ContainsKey(GOLD_CURRENCY_ID) ? result.VirtualCurrency[GOLD_CURRENCY_ID] : 0;
                UpdateGoldText(gold);
                Debug.Log($"PlayFab에서 골드 잔액을 가져왔습니다: {gold}");
            },
            error => {
                Debug.LogError($"PlayFab에서 화폐 잔액을 가져오는 데 실패했습니다: {error.ErrorMessage}");
            }
        );
    }

    private void UpdateGoldText(int amount)
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