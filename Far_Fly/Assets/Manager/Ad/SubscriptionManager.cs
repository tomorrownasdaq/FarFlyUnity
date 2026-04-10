using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.Purchasing;

public class SubscriptionManager : MonoBehaviour, IStoreListener
{
    [SerializeField] private Button subscriptionButton;
    [SerializeField] private Button doubleRewardButton; // 새로 추가된 부분

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;
    private const string subscriptionProductId = "adskipfarfly";
    public bool IsSubscribed { get; private set; } = false;

    private void Start()
    {
        SetupUI();
        InitializePurchasing();
        CheckSubscriptionStatus();
    }

    private void SetupUI()
    {
        if (subscriptionButton != null)
        {
            subscriptionButton.onClick.RemoveAllListeners();
            subscriptionButton.onClick.AddListener(PurchaseSubscription);
            Debug.Log("Subscription button listener set up.");
        }
        else
        {
            Debug.LogError("Subscription Button is not assigned in the inspector!");
        }

        // doubleRewardButton 확인 (새로 추가된 부분)
        if (doubleRewardButton == null)
        {
            Debug.LogError("Double Reward Button is not assigned in the inspector!");
        }
    }

    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(subscriptionProductId, ProductType.Subscription);
        UnityPurchasing.Initialize(this, builder);
    }

    private void CheckSubscriptionStatus()
    {
        var request = new GetUserDataRequest
        {
            Keys = new List<string> { "SubscriptionStatus" }
        };
        PlayFabClientAPI.GetUserData(request, OnGetUserDataSuccess, OnPlayFabError);
    }

    private void OnGetUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.TryGetValue("SubscriptionStatus", out UserDataRecord statusRecord))
        {
            IsSubscribed = bool.Parse(statusRecord.Value);
            UpdateUIForSubscription();
        }
        else
        {
            Debug.Log("Subscription status not found in User Data.");
            IsSubscribed = false;
            UpdateUIForSubscription();
        }
    }

    private void UpdateUIForSubscription()
    {
        if (IsSubscribed)
        {
            subscriptionButton.gameObject.SetActive(false);
            if (doubleRewardButton != null)
            {
                doubleRewardButton.gameObject.SetActive(false);
            }
            Debug.Log("User is subscribed. Subscription and Double Reward buttons hidden.");
        }
        else
        {
            subscriptionButton.gameObject.SetActive(true);
            if (doubleRewardButton != null)
            {
                doubleRewardButton.gameObject.SetActive(true);
            }
            Debug.Log("User is not subscribed. Subscription and Double Reward buttons shown.");
        }
    }

    private void PurchaseSubscription()
    {
        Debug.Log("Attempting to purchase subscription...");
        if (storeController != null && storeController.products != null)
        {
            Product product = storeController.products.WithID(subscriptionProductId);
            if (product != null && product.availableToPurchase)
            {
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError("Subscription product not found or not available for purchase.");
            }
        }
        else
        {
            Debug.LogError("Store controller or products not initialized.");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("In-App Purchasing successfully initialized");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"In-App Purchasing initialize failed: {error}. {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, subscriptionProductId, StringComparison.Ordinal))
        {
            Debug.Log("Subscription purchased successfully.");
            UpdateSubscriptionStatus(true);
            return PurchaseProcessingResult.Complete;
        }
        else
        {
            Debug.LogError($"ProcessPurchase: FAIL. Unrecognized product: {args.purchasedProduct.definition.id}");
            return PurchaseProcessingResult.Pending;
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed: {product.definition.id}, {failureReason}");
    }

    private void UpdateSubscriptionStatus(bool isSubscribed)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"SubscriptionStatus", isSubscribed.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnUpdateUserDataSuccess, OnPlayFabError);
    }

    private void OnUpdateUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Subscription status updated successfully in User Data.");
        IsSubscribed = true;
        UpdateUIForSubscription();
    }

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError($"PlayFab operation failed: {error.ErrorMessage}");
    }
}