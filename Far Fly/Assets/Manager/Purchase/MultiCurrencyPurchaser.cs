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
        Debug.Log("MultiCurrencyPurchaser Start method called");
        Invoke("DelayedInitialization", initializationDelay);
    }

    private void DelayedInitialization()
    {
        Debug.Log("DelayedInitialization called");
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        Debug.Log("InitializePurchasing called");
        if (IsInitialized())
        {
            Debug.Log("Purchasing is already initialized");
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var group in currencyGroups)
        {
            foreach (var item in group.items)
            {
                Debug.Log($"Adding product: {item.productId}");
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
        Debug.Log($"Purchase button clicked for product: {productId}");
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
        Debug.Log("IAP OnInitialized called");
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP 초기화 성공");
        UpdatePriceDisplays();
        SetupPurchaseButtons();
    }

    private void UpdatePriceDisplays()
    {
        Debug.Log("UpdatePriceDisplays called");
        foreach (var group in currencyGroups)
        {
            foreach (var item in group.items)
            {
                Product product = storeController.products.WithID(item.productId);
                if (product != null)
                {
                    Debug.Log($"Updating price for product: {item.productId}, Price: {product.metadata.localizedPriceString}");
                    item.priceText.text = product.metadata.localizedPriceString;

                    // 추가: 가격이 0인 경우 로그
                    if (product.metadata.localizedPrice == 0)
                    {
                        Debug.LogWarning($"Product {item.productId} has a price of 0!");
                    }
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
        Debug.Log("SetupPurchaseButtons called");
        foreach (var group in currencyGroups)
        {
            foreach (var item in group.items)
            {
                item.purchaseButton.onClick.RemoveAllListeners();
                item.purchaseButton.onClick.AddListener(() => OnPurchaseButtonClick(item.productId));
                Debug.Log($"Purchase button set up for product: {item.productId}");
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
        Debug.Log($"ProcessPurchase called for product: {args.purchasedProduct.definition.id}");
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
        Debug.Log($"AddCurrencyToPlayFab called: currencyId = {currencyId}, amount = {amount}");
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

    // 추가: 수동으로 가격 업데이트를 트리거하는 메서드
    public void ManuallyUpdatePrices()
    {
        Debug.Log("ManuallyUpdatePrices called");
        if (IsInitialized())
        {
            UpdatePriceDisplays();
        }
        else
        {
            Debug.LogWarning("가격을 업데이트할 수 없습니다: IAP가 초기화되지 않았습니다.");
        }
    }
}