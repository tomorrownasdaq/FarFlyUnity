using UnityEngine;
using UnityEngine.Purchasing;
using TMPro;
using System;

public class DiamondPurchaser : MonoBehaviour, IStoreListener
{
    private const string DIAMOND_PACK_ID = "dia10";
    private int currentDiamonds = 0;
    [SerializeField] private TextMeshProUGUI DiamondText;
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    
    [SerializeField] private float initializationDelay = 2f; // 지연 시간(초)

    // 다른 변수들...

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
        if (DiamondText != null && !string.IsNullOrEmpty(DiamondText.text))
        {
            if (int.TryParse(DiamondText.text, out int initialCount))
            {
                currentDiamonds = initialCount;
                Debug.Log($"초기 다이아몬드 개수: {currentDiamonds}");
            }
            else
            {
                Debug.LogWarning("텍스트에서 초기 다이아몬드 개수를 읽어올 수 없습니다. 0으로 초기화합니다.");
                currentDiamonds = 0;
            }
        }
        else
        {
            Debug.LogWarning("Diamond Text UI가 할당되지 않았거나 비어 있습니다. 0으로 초기화합니다.");
            currentDiamonds = 0;
        }
        UpdateDiamondText();
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
            AddDiamonds(10);
            Debug.Log($"구매 성공: Diamond Pack. 현재 다이아몬드: {currentDiamonds}");
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

    private void AddDiamonds(int amount)
    {
        currentDiamonds += amount;
        UpdateDiamondText();
        Debug.Log($"다이아몬드 {amount}개 추가됨. 총 다이아몬드: {currentDiamonds}");
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