using UnityEngine;
using UnityEngine.Purchasing;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class MultiCurrencyPurchaser : MonoBehaviour, IStoreListener
{
    // 다이아몬드 팩
    private const string DIAMOND_PACK_100 = "dia100";
    private const string DIAMOND_PACK_600 = "dia600";
    private const string DIAMOND_PACK_2500 = "dia2500";

    // 골드 팩
    private const string GOLD_PACK_30000 = "gd30000";
    private const string GOLD_PACK_7000 = "gd7000";
    private const string GOLD_PACK_1000 = "gd1000";

    // 파워 팩
    private const string POWER_PACK_5 = "pw5";
    private const string POWER_PACK_30 = "pw30";
    private const string POWER_PACK_100 = "pw100";

    // 통화 코드
    private const string DIAMOND_CURRENCY_CODE = "DI";
    private const string GOLD_CURRENCY_CODE = "GL";
    private const string POWER_CURRENCY_CODE = "PW";

    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    [SerializeField] private float initializationDelay = 2f;

    private PlayFabSpecificCurrencyDisplay currencyDisplay;

    void Start()
    {
        currencyDisplay = FindObjectOfType<PlayFabSpecificCurrencyDisplay>();
        if (currencyDisplay == null)
        {
            Debug.LogError("PlayFabSpecificCurrencyDisplay not found in the scene!");
        }

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

        // 모든 제품 추가
        builder.AddProduct(DIAMOND_PACK_100, ProductType.Consumable);
        builder.AddProduct(DIAMOND_PACK_600, ProductType.Consumable);
        builder.AddProduct(DIAMOND_PACK_2500, ProductType.Consumable);
        builder.AddProduct(GOLD_PACK_30000, ProductType.Consumable);
        builder.AddProduct(GOLD_PACK_7000, ProductType.Consumable);
        builder.AddProduct(GOLD_PACK_1000, ProductType.Consumable);
        builder.AddProduct(POWER_PACK_5, ProductType.Consumable);
        builder.AddProduct(POWER_PACK_30, ProductType.Consumable);
        builder.AddProduct(POWER_PACK_100, ProductType.Consumable);

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
        string errorMessage = message ?? "No additional information provided.";
        Debug.LogError($"IAP 초기화 실패: {error}. 추가 정보: {errorMessage}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        switch (args.purchasedProduct.definition.id)
        {
            case DIAMOND_PACK_100:
                AddCurrencyToPlayFab(DIAMOND_CURRENCY_CODE, 100);
                break;
            case DIAMOND_PACK_600:
                AddCurrencyToPlayFab(DIAMOND_CURRENCY_CODE, 600);
                break;
            case DIAMOND_PACK_2500:
                AddCurrencyToPlayFab(DIAMOND_CURRENCY_CODE, 2500);
                break;
            case GOLD_PACK_30000:
                AddCurrencyToPlayFab(GOLD_CURRENCY_CODE, 30000);
                break;
            case GOLD_PACK_7000:
                AddCurrencyToPlayFab(GOLD_CURRENCY_CODE, 7000);
                break;
            case GOLD_PACK_1000:
                AddCurrencyToPlayFab(GOLD_CURRENCY_CODE, 1000);
                break;
            case POWER_PACK_5:
                AddCurrencyToPlayFab(POWER_CURRENCY_CODE, 5);
                break;
            case POWER_PACK_30:
                AddCurrencyToPlayFab(POWER_CURRENCY_CODE, 30);
                break;
            case POWER_PACK_100:
                AddCurrencyToPlayFab(POWER_CURRENCY_CODE, 100);
                break;
            default:
                Debug.LogWarning($"알 수 없는 제품 구매: {args.purchasedProduct.definition.id}");
                return PurchaseProcessingResult.Pending;
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"구매 실패: {product.definition.id}, 이유: {failureReason}");
    }

    private void AddCurrencyToPlayFab(string currencyCode, int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = currencyCode,
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request,
            result => {
                Debug.Log($"{currencyCode} {amount}개 추가됨. 현재 잔액: {result.Balance}");
                if (currencyDisplay != null)
                {
                    currencyDisplay.SyncCurrency();
                }
                else
                {
                    Debug.LogWarning("PlayFabSpecificCurrencyDisplay not found. Unable to sync currency display.");
                }
            },
            error => {
                Debug.LogError($"PlayFab {currencyCode} 추가 실패: {error.ErrorMessage}");
            }
        );
    }
}