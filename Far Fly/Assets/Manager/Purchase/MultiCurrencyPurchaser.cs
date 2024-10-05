using UnityEngine;
using UnityEngine.Purchasing;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class MultiCurrencyPurchaser : MonoBehaviour, IStoreListener
{
    private const string DIAMOND_PACK_ID = "dia10";
    private const string POWER_PACK_ID = "pw5";
    private const string GOLD_PACK_ID = "gd500";
    private const string DIAMOND_CURRENCY_ID = "DI";
    private const string POWER_CURRENCY_ID = "PW";
    private const string GOLD_CURRENCY_ID = "GL";

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
        builder.AddProduct(DIAMOND_PACK_ID, ProductType.Consumable);
        builder.AddProduct(POWER_PACK_ID, ProductType.Consumable);
        builder.AddProduct(GOLD_PACK_ID, ProductType.Consumable);
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
        if (string.Equals(args.purchasedProduct.definition.id, DIAMOND_PACK_ID, StringComparison.Ordinal))
        {
            AddCurrencyToPlayFab(DIAMOND_CURRENCY_ID, 10);
            return PurchaseProcessingResult.Complete;
        }
        else if (string.Equals(args.purchasedProduct.definition.id, POWER_PACK_ID, StringComparison.Ordinal))
        {
            AddCurrencyToPlayFab(POWER_CURRENCY_ID, 5);
            return PurchaseProcessingResult.Complete;
        }
        else if (string.Equals(args.purchasedProduct.definition.id, GOLD_PACK_ID, StringComparison.Ordinal))
        {
            AddCurrencyToPlayFab(GOLD_CURRENCY_ID, 500);
            return PurchaseProcessingResult.Complete;
        }
        else
        {
            Debug.LogWarning($"알 수 없는 제품 구매: {args.purchasedProduct.definition.id}");
            return PurchaseProcessingResult.Pending;
        }
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