using UnityEngine;
using PlayFab;
using PlayFab.EconomyModels;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayFabInventoryManager : MonoBehaviour
{
    public GameObject inventoryItemPrefab;
    public Transform inventoryContent;
    public Button loadInventoryButton;

    private const string COLLECTION_ID = "inventory_ball";
    private Dictionary<string, InventoryItemUI> inventoryItems = new Dictionary<string, InventoryItemUI>();

    private void Start()
    {
        loadInventoryButton.onClick.AddListener(GetInventoryItems);
        HideExistingItems();
    }

    private void HideExistingItems()
    {
        // Disable all child objects of inventoryContent
        foreach (Transform child in inventoryContent)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void GetInventoryItems()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            var request = new GetInventoryItemsRequest
            {
                Count = 50,
                CollectionId = COLLECTION_ID
            };
            PlayFabEconomyAPI.GetInventoryItems(request, OnGetInventoryItemsSuccess, OnError);
        }
        else
        {
            Debug.LogError("플레이어가 로그인되어 있지 않습니다.");
        }
    }

    private void OnGetInventoryItemsSuccess(GetInventoryItemsResponse result)
    {
        ClearInventory();

        foreach (var item in result.Items)
        {
            CreateInventoryItem(item);
        }

        if (result.Items.Count == 0)
        {
            Debug.Log($"No items found in the '{COLLECTION_ID}' collection.");
        }
    }

    private void ClearInventory()
    {
        // Destroy all existing inventory items
        foreach (var item in inventoryItems.Values)
        {
            Destroy(item.gameObject);
        }
        inventoryItems.Clear();

        // Hide any remaining items in the content
        HideExistingItems();
    }

    private void CreateInventoryItem(InventoryItem item)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, inventoryContent);
        InventoryItemUI itemUI = newItem.GetComponent<InventoryItemUI>();
        inventoryItems[item.Id] = itemUI;

        // Ensure the new item is active
        newItem.SetActive(true);

        var request = new GetItemRequest
        {
            Id = item.Id,
            AlternateId = null
        };

        PlayFabEconomyAPI.GetItem(request, result =>
        {
            string itemName = GetItemName(result.Item);
            itemUI.SetItemInfo(itemName, item.Id);

            if (result.Item?.Images != null && result.Item.Images.Count > 0)
            {
                StartCoroutine(LoadItemImage(result.Item.Images[0].Url, itemUI));
            }
        }, OnError);
    }

    private string GetItemName(CatalogItem item)
    {
        if (item?.Title != null && item.Title.ContainsKey("NEUTRAL"))
        {
            return item.Title["NEUTRAL"];
        }
        return "Unknown Item";
    }

    private System.Collections.IEnumerator LoadItemImage(string imageUrl, InventoryItemUI itemUI)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
                itemUI.SetItemImage(texture);
            }
            else
            {
                Debug.LogError($"Failed to load image: {request.error}");
            }
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab 오류: {error.ErrorMessage}");
    }
}