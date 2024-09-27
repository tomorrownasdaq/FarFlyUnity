using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.EconomyModels;

public class PlayFabShopManager : MonoBehaviour
{
    private List<CatalogItem> catalogItems = new List<CatalogItem>();

    void Start()
    {
        // Assuming the player is already logged in
        FetchCatalogItems();
    }

    public void FetchCatalogItems()
    {
      
    
        var request = new SearchItemsRequest
        {
            Filter = "contentType eq 'Ball'", // Filter for items with content type "ball"
            Count = 50,
            OrderBy = "id asc" // Order results by ID in ascending order
        };
        PlayFabEconomyAPI.SearchItems(request, OnSearchItemsSuccess, OnError);
   
    }

    private void OnSearchItemsSuccess(SearchItemsResponse result)
    {
        catalogItems = result.Items;
        Debug.Log($"Successfully retrieved {catalogItems.Count} items from the catalog");

        foreach (var item in catalogItems)
        {
            string imageUrl = GetItemImageUrl(item);
            string priceInfo = GetItemPriceInfo(item);

            Debug.Log($"Item: {item.Title}");
            Debug.Log($"ID: {item.Id}");
            Debug.Log($"Description: {item.Description}");
            Debug.Log($"Image URL: {imageUrl}");
            Debug.Log($"Price: {priceInfo}");
            Debug.Log("--------------------");

            // Here you would typically update your UI with this information
        }
    }

    private string GetItemImageUrl(CatalogItem item)
    {
        if (item.Images != null && item.Images.Count > 0)
        {
            return item.Images[0].Url;
        }
        return "No image available";
    }

    private string GetItemPriceInfo(CatalogItem item)
    {
        if (item.PriceOptions != null && item.PriceOptions.Prices != null && item.PriceOptions.Prices.Count > 0)
        {
            var price = item.PriceOptions.Prices[0];
            if (price.Amounts != null && price.Amounts.Count > 0)
            {
                return $"{price.Amounts[0].Amount} {price.Amounts[0].ItemId}";
            }
        }
        return "Price not available";
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab Error: {error.GenerateErrorReport()}");
    }

    // Additional methods for purchasing items, checking inventory, etc. can be added here
}