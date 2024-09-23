using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlayFab;
using PlayFab.EconomyModels;

public class StoreManager : MonoBehaviour
{
    public GameObject itemListContent;
    public GameObject itemPrefab;
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
            Filter = "contentType eq 'Ball'",
            Count = 50,
            OrderBy = "id asc",
            Store = null
        };
        PlayFabEconomyAPI.SearchItems(request, OnSearchItemsSuccess, OnError);
    }

    private void OnSearchItemsSuccess(SearchItemsResponse result)
    {
        catalogItems = result.Items;
        Debug.Log($"Successfully retrieved {catalogItems.Count} items from the catalog");
        DisplayItems();
    }

    private void DisplayItems()
    {
        // Clear existing items
        foreach (Transform child in itemListContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new item entries
        foreach (var item in catalogItems)
        {
            GameObject newItem = Instantiate(itemPrefab, itemListContent.transform);
            ShopItemUI shopItemUI = newItem.GetComponent<ShopItemUI>();
            if (shopItemUI != null)
            {
                string title = GetItemTitle(item);
                string price = GetItemPriceInfo(item);

                Debug.Log($"Setting item info - Title: {title}, Price: {price}");
                shopItemUI.SetItemInfo(title, price);
            }
            else
            {
                Debug.LogError("ShopItemUI component not found on prefab");
            }
        }
    }

    private string GetItemTitle(CatalogItem item)
    {
        if (item.Title != null && item.Title.Count > 0)
        {
            string title = item.Title.TryGetValue("en", out string englishTitle) ? englishTitle : item.Title.Values.First();
            Debug.Log($"Got item title: {title}");
            return title;
        }
        Debug.LogWarning("Item title is null or empty");
        return "Untitled Item";
    }

    private string GetItemPriceInfo(CatalogItem item)
    {
        if (item.PriceOptions != null && item.PriceOptions.Prices != null && item.PriceOptions.Prices.Count > 0)
        {
            var price = item.PriceOptions.Prices[0];
            if (price.Amounts != null && price.Amounts.Count > 0)
            {
                string priceInfo = $"{price.Amounts[0].Amount} ";
                Debug.Log($"Got item price: {priceInfo}");
                return priceInfo;
            }
        }
        Debug.LogWarning("Price information not available for item");
        return "Price not available";
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab Error: {error.GenerateErrorReport()}");
    }
}