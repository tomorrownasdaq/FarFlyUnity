using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class ShopDataManager : MonoBehaviour
{
    public void PurchaseItem(string itemId, int price, string currencyCode)
    {
        var request = new PurchaseItemRequest
        {
            ItemId = itemId,
            Price = price,
            VirtualCurrency = currencyCode
        };

        PlayFabClientAPI.PurchaseItem(request, OnPurchaseSuccess, OnPurchaseFailure);
    }

    private void OnPurchaseSuccess(PurchaseItemResult result)
    {
        Debug.Log("아이템 구매 성공!");
        // 여기에 구매 성공 후 로직을 추가하세요 (예: UI 업데이트)
    }

    private void OnPurchaseFailure(PlayFabError error)
    {
        Debug.LogError($"아이템 구매 실패: {error.ErrorMessage}");
        // 여기에 구매 실패 시 처리 로직을 추가하세요
    }

    public void GetCatalogItems()
    {
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), OnCatalogReceived, OnCatalogFailure);
    }

    private void OnCatalogReceived(GetCatalogItemsResult result)
    {
        foreach (var item in result.Catalog)
        {
            Debug.Log($"카탈로그 아이템: {item.DisplayName}, 가격: {item.VirtualCurrencyPrices["RM"]}");
            // 여기에 상점 UI를 업데이트하는 로직을 추가하세요
        }
    }

    private void OnCatalogFailure(PlayFabError error)
    {
        Debug.LogError($"카탈로그 로드 실패: {error.ErrorMessage}");
    }
}