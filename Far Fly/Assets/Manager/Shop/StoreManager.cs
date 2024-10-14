using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlayFab;
using PlayFab.EconomyModels;
using PlayFab.ClientModels;
using UnityEngine.Networking;
using System.Collections;

public class StoreManager : MonoBehaviour
{
    public GameObject itemListContent;
    public GameObject itemPrefab;
    private List<PlayFab.EconomyModels.CatalogItem> catalogItems = new List<PlayFab.EconomyModels.CatalogItem>();
    private HashSet<string> inventoryItemIds = new HashSet<string>();

    void Start()
    {
        // Assuming the player is already logged in
        GetUserInventory();
    }

    public void GetUserInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetUserInventorySuccess, OnError);
    }

    private void OnGetUserInventorySuccess(GetUserInventoryResult result)
    {
        inventoryItemIds = new HashSet<string>(result.Inventory.Select(item => item.ItemId));
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
        catalogItems = result.Items.Where(item => !inventoryItemIds.Contains(item.Id)).ToList();
        Debug.Log($"Successfully retrieved {catalogItems.Count} items from the catalog (excluding inventory items)");
        DisplayItems();
    }

    private void DisplayItems()
    {
        // Disable existing items
        foreach (Transform child in itemListContent.transform)
        {
            child.gameObject.SetActive(false);
        }

        // Create or enable item entries
        for (int i = 0; i < catalogItems.Count; i++)
        {
            GameObject itemObject;
            if (i < itemListContent.transform.childCount)
            {
                // Reuse existing object
                itemObject = itemListContent.transform.GetChild(i).gameObject;
                itemObject.SetActive(true);
            }
            else
            {
                // Create new object if necessary
                itemObject = Instantiate(itemPrefab, itemListContent.transform);
            }

            ShopItemUI shopItemUI = itemObject.GetComponent<ShopItemUI>();
            if (shopItemUI != null)
            {
                string title = GetItemTitle(catalogItems[i]);
                string price = GetItemPriceInfo(catalogItems[i]);
                string imageUrl = GetItemImageUrl(catalogItems[i]);
                string itemId = catalogItems[i].Id;
                Debug.Log($"Setting item info - Title: {title}, Price: {price}, ImageUrl: {imageUrl}, ItemId: {itemId}");
                shopItemUI.SetItemInfo(title, price, imageUrl, itemId);
                StartCoroutine(LoadItemImage(shopItemUI, imageUrl));
            }
            else
            {
                Debug.LogError("ShopItemUI component not found on prefab");
            }
        }
    }

    private string GetItemTitle(PlayFab.EconomyModels.CatalogItem item)
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

    private string GetItemPriceInfo(PlayFab.EconomyModels.CatalogItem item)
    {
        if (item.PriceOptions != null && item.PriceOptions.Prices != null && item.PriceOptions.Prices.Count > 0)
        {
            var price = item.PriceOptions.Prices[0];
            if (price.Amounts != null && price.Amounts.Count > 0)
            {
                // Convert the price to a string, it might be a decimal or float
                string priceInfo = price.Amounts[0].Amount.ToString();
                Debug.Log($"Got item price: {priceInfo}");
                return priceInfo;
            }
        }
        Debug.LogWarning("Price information not available for item");
        return "Price not available";
    }

    private string GetItemImageUrl(PlayFab.EconomyModels.CatalogItem item)
    {
        if (item.Images != null && item.Images.Count > 0)
        {
            string imageUrl = item.Images[0].Url;
            Debug.Log($"Got item image URL: {imageUrl}");
            return imageUrl;
        }
        Debug.LogWarning("Image URL not available for item");
        return null;
    }

    private IEnumerator LoadItemImage(ShopItemUI shopItemUI, string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load image: {www.error}");
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                shopItemUI.SetItemImage(texture);
            }
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab Error: {error.GenerateErrorReport()}");
    }
}