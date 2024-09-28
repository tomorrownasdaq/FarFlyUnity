using UnityEngine;
using PlayFab;
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;

public class ShopItemLogic : MonoBehaviour
{
    private string itemId;
    private int price;
    private PlayFabSpecificCurrencyDisplay currencyDisplay;

    public event Action<int> OnGoldUpdated;

    private const string GOLD_CURRENCY_CODE = "GL";
    private const string CONFIRMATION_DIALOG_PREFAB_NAME = "ConfirmationDialogPrefab";

    private void Start()
    {
        currencyDisplay = FindObjectOfType<PlayFabSpecificCurrencyDisplay>();
        if (currencyDisplay == null)
        {
            Debug.LogError("PlayFabSpecificCurrencyDisplay not found in the scene!");
        }
    }

    public void SetItemInfo(string title, int itemPrice, string realItemId)
    {
        itemId = realItemId;
        price = itemPrice;
    }

    public bool CanPurchase(int currentGold)
    {
        return currentGold >= price;
    }

    public void TryPurchaseItem()
    {
        int currentGold = GetCurrentGoldAmount();
        if (CanPurchase(currentGold))
        {
            ShowConfirmationDialog();
        }
        else
        {
            Debug.Log("Not enough gold to purchase the item.");
        }
    }

    private int GetCurrentGoldAmount()
    {
        if (currencyDisplay != null)
        {
            foreach (var display in currencyDisplay.currencyDisplays)
            {
                if (display.currencyId == GOLD_CURRENCY_CODE)
                {
                    if (int.TryParse(display.displayText.text, out int goldAmount))
                    {
                        return goldAmount;
                    }
                    else
                    {
                        Debug.LogWarning("Failed to parse gold amount from display text");
                    }
                }
            }
        }
        Debug.LogWarning("Gold currency not found in PlayFabSpecificCurrencyDisplay");
        return 0;
    }

    private void ShowConfirmationDialog()
    {
        GameObject dialogPrefab = Resources.Load<GameObject>(CONFIRMATION_DIALOG_PREFAB_NAME);
        if (dialogPrefab == null)
        {
            Debug.LogError($"Cannot find {CONFIRMATION_DIALOG_PREFAB_NAME}. Make sure it's in the Resources folder.");
            return;
        }

        GameObject dialogInstance = Instantiate(dialogPrefab, transform.root);
        dialogInstance.SetActive(true);

        ConfirmationDialog dialog = dialogInstance.GetComponent<ConfirmationDialog>();
        if (dialog != null)
        {
            dialog.SetMessage($"Buy this item for {price} Gold");
            dialog.OnConfirm += () => {
                PurchaseItem();
                Destroy(dialogInstance);
            };
            dialog.OnCancel += () => {
                Debug.Log("Purchase cancelled.");
                Destroy(dialogInstance);
            };
        }
        else
        {
            Debug.LogError("ConfirmationDialog component not found.");
            Destroy(dialogInstance);
        }
    }

    private void PurchaseItem()
    {
        var request = new PurchaseInventoryItemsRequest
        {
            Amount = 1,
            Item = new InventoryItemReference
            {
                Id = itemId,
            },
            PriceAmounts = new List<PurchasePriceAmount>
            {
                new PurchasePriceAmount
                {
                    ItemId = GOLD_CURRENCY_CODE,
                    Amount = price
                }
            }
        };

        Debug.Log($"Attempting to purchase item. Details:\n" +
                  $"Item ID: {itemId}\n" +
                  $"Quantity: 1\n" +
                  $"Price: {price} {GOLD_CURRENCY_CODE}");

        PlayFabEconomyAPI.PurchaseInventoryItems(request,
            result => {
                Debug.Log($"Item purchased successfully. Details:\n" +
                          $"Item ID: {itemId}\n" +
                          $"Quantity: 1\n" +
                          $"Price Paid: {price} {GOLD_CURRENCY_CODE}\n");

                // Update currency display after successful purchase
                if (currencyDisplay != null)
                {
                    currencyDisplay.SyncCurrency();
                }
            },
            error => {
                Debug.LogError($"Failed to purchase item. Details:\n" +
                               $"Item ID: {itemId}\n" +
                               $"Error: {error.ErrorMessage}\n" +
                               $"Error Details: {error.ErrorDetails}");
            }
        );
    }
}