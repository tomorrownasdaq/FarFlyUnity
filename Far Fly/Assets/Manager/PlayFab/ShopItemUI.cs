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

    // Constants for currency IDs
    private const string GOLD_CURRENCY_ID = "GL";

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

    private void Start()
    {
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        GetCurrencyBalances();
    }

    private void OnBuyButtonClicked()
    {
        if (int.TryParse(goldText.text, out int currentGold))
        {
            if (currentGold >= price)
            {
                PurchaseItem();
            }
            else
            {
                Debug.Log("Not enough gold to purchase this item.");
            }
        }
        else
        {
            Debug.LogError("Failed to parse current gold amount.");
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
                Debug.Log($"Item purchased successfully. New balance: {newBalance}");
            },
            error => {
                Debug.LogError($"Failed to purchase item: {error.ErrorMessage}");
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