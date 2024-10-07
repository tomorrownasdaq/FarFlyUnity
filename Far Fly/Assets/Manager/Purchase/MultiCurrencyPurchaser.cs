using UnityEngine;
using UnityEngine.Purchasing;
using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class CurrencyItem
{
    public string productId;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI priceText;
    public Button purchaseButton;
}

[System.Serializable]
public class CurrencyGroup
{
    public string currencyId;
    public List<CurrencyItem> items;
}

public class MultiCurrencyPurchaser : MonoBehaviour, IStoreListener
{
    [SerializeField]
    private List<CurrencyGroup> currencyGroups = new List<CurrencyGroup>
    {
        new CurrencyGroup { currencyId = "DI", items = new List<CurrencyItem>() },
        new CurrencyGroup { currencyId = "PW", items = new List<CurrencyItem>() },
        new CurrencyGroup { currencyId = "GL", items = new List<CurrencyItem>() }
    };

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    [SerializeField] private float initializationDelay = 0.5f;

    void Start()
    {
        Invoke("DelayedInitialization", initializationDelay);
    }

    private void DelayedInitialization()
    {
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        if (IsInitialized()) return;
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var group in currencyGroups)
        {
            foreach (var item in group.items)
            {
                builder.AddProduct(item.productId, ProductType.Consumable);
            }
        }

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void OnPurchaseButtonClick(string productId)
    {
        BuyProduct(productId);
    }

    private void BuyProduct(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"구매 시도: {product.definition.id}");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError($"구매 실패: {productId} 상품을 찾을 수 없거나 구매할 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("구매 실패: IAP가 초기화되지 않았습니다.");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP 초기화 성공");
        UpdatePriceDisplays();
        SetupPurchaseButtons();
    }

    private void UpdatePriceDisplays()
    {
        foreach (var group in currencyGroups)
        {
            foreach (var item in group.items)
            {
                Product product = storeController.products.WithID(item.productId);
                if (product != null)
                {
                    item.priceText.text = product.metadata.localizedPriceString;
                }
                else
                {
                    Debug.LogWarning($"상품을 찾을 수 없음: {item.productId}");
                }
            }
        }
    }

    private void SetupPurchaseButtons()
    {
        foreach (var group in currencyGroups)
        {
            foreach (var item in group.items)
            {
                item.purchaseButton.onClick.RemoveAllListeners();
                item.purchaseButton.onClick.AddListener(() => OnPurchaseButtonClick(item.productId));
            }
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        string errorMessage = message ?? "추가 정보가 제공되지 않았습니다.";
        Debug.LogError($"IAP 초기화 실패: {error}. 추가 정보: {errorMessage}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        foreach (var group in currencyGroups)
        {
            foreach (var item in group.items)
            {
                if (string.Equals(args.purchasedProduct.definition.id, item.productId, StringComparison.Ordinal))
                {
                    int amount = GetCurrencyAmount(item);
                    AddCurrencyToPlayFab(group.currencyId, amount);
                    return PurchaseProcessingResult.Complete;
                }
            }
        }

        Debug.LogWarning($"알 수 없는 제품 구매: {args.purchasedProduct.definition.id}");
        return PurchaseProcessingResult.Pending;
    }

    private int GetCurrencyAmount(CurrencyItem item)
    {
        if (int.TryParse(item.amountText.text, out int amount))
        {
            return amount;
        }
        Debug.LogWarning($"통화 수량을 파싱할 수 없음: {item.productId}. 기본값 0 반환.");
        return 0;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"구매 실패: {product.definition.id}, 이유: {failureReason}");
    }

    private void AddCurrencyToPlayFab(string currencyId, int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = currencyId,
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request,
            result => {
                Debug.Log($"{currencyId} {amount}개 추가됨. 새 잔액: {result.Balance}");
                UpdateCurrencyDisplay();
            },
            error => {
                Debug.LogError($"PlayFab {currencyId} 추가 실패: {error.ErrorMessage}");
            }
        );
    }

    private void UpdateCurrencyDisplay()
    {
        PlayFabSpecificCurrencyDisplay currencyDisplay = FindObjectOfType<PlayFabSpecificCurrencyDisplay>();
        if (currencyDisplay != null)
        {
            currencyDisplay.SyncCurrency();
        }
        else
        {
            Debug.LogWarning("PlayFabSpecificCurrencyDisplay를 찾을 수 없습니다. 통화 표시가 업데이트되지 않았습니다.");
        }
    }
}