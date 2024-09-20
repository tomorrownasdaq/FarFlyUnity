using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using TMPro;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class Power5Purchaser : MonoBehaviour, IStoreListener
{
    private const string DIAMOND_PACK_ID = "pw5";
    private const string VIRTUAL_CURRENCY_CODE = "PW"; // PlayFab에서 설정한 다이아몬드의 Virtual Currency 코드
    private int currentDiamonds = 0;
    [SerializeField] private TextMeshProUGUI DiamondText;
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    [SerializeField] private float initializationDelay = 2f;

    void Start()
    {
        Invoke("DelayedInitialization", initializationDelay);
    }

    private void DelayedInitialization()
    {
        LoadInitialDiamondCount();
        InitializePurchasing();
    }

    private void LoadInitialDiamondCount()
    {
        GetVirtualCurrencyBalance();
    }

    private void GetVirtualCurrencyBalance()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result => {
                if (result.VirtualCurrency.TryGetValue(VIRTUAL_CURRENCY_CODE, out int balance))
                {
                    currentDiamonds = balance;
                    UpdateDiamondText();
                    Debug.Log($"PlayFab에서 다이아몬드 잔액을 가져왔습니다: {currentDiamonds}");
                }
                else
                {
                    Debug.LogWarning($"PlayFab에서 {VIRTUAL_CURRENCY_CODE} 통화를 찾을 수 없습니다.");
                }
            },
            error => {
                Debug.LogError($"PlayFab에서 다이아몬드 잔액을 가져오는 데 실패했습니다: {error.ErrorMessage}");
            }
        );
    }

    private void InitializePurchasing()
    {
        if (IsInitialized()) return;
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(DIAMOND_PACK_ID, ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void OnPurchaseButtonClick()
    {
        BuyDiamondPack();
    }

    private void BuyDiamondPack()
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(DIAMOND_PACK_ID);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"구매 시도: {product.definition.id}");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError($"구매 실패: {DIAMOND_PACK_ID} 상품을 찾을 수 없거나 구매할 수 없습니다.");
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
        if (string.Equals(args.purchasedProduct.definition.id, DIAMOND_PACK_ID, System.StringComparison.Ordinal))
        {
            AddDiamondsToPlayFab(5);
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

    private void AddDiamondsToPlayFab(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = VIRTUAL_CURRENCY_CODE,
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request,
            result => {
                currentDiamonds = result.Balance;
                UpdateDiamondText();
                Debug.Log($"다이아몬드 {amount}개 추가됨. 현재 다이아몬드: {currentDiamonds}");
            },
            error => {
                Debug.LogError($"PlayFab 다이아몬드 추가 실패: {error.ErrorMessage}");
            }
        );
    }

    private void UpdateDiamondText()
    {
        if (DiamondText != null)
        {
            DiamondText.text = $"{currentDiamonds}";
        }
        else
        {
            Debug.LogWarning("Diamond Text UI가 할당되지 않았습니다.");
        }
    }
}