using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EconomyModels;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

public class StageInventoryManager : MonoBehaviour
{
    private const string COLLECTION_ID = "inventory_ball";
    private Dictionary<string, InventoryItemData> inventoryItems = new Dictionary<string, InventoryItemData>();
    private string lastSelectedItemId;

    private void Start()
    {
        GetInventoryItems();
    }

    public void GetInventoryItems()
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
            ProcessInventoryItem(item);
        }
        if (result.Items.Count == 0)
        {
            Debug.Log($"No items found in the '{COLLECTION_ID}' collection.");
        }
    }

    private void ClearInventory()
    {
        inventoryItems.Clear();
        lastSelectedItemId = null;
    }

    private void ProcessInventoryItem(InventoryItem item)
    {
        var request = new GetItemRequest
        {
            Id = item.Id,
            AlternateId = null
        };
        PlayFabEconomyAPI.GetItem(request, result =>
        {
            string itemName = GetItemName(result.Item);
            string accDescription = GetACCDescription(result.Item);
            string imageUrl = GetItemImageUrl(result.Item);

            inventoryItems[item.Id] = new InventoryItemData
            {
                Id = item.Id,
                Name = itemName,
                ACCDescription = accDescription,
                ImageUrl = imageUrl
            };

            Debug.Log($"Processed item - Name: {itemName}, ID: {item.Id}, ACC: {accDescription}");
        }, OnError);
    }

    public void SelectItem(string itemId)
    {
        if (inventoryItems.ContainsKey(itemId))
        {
            lastSelectedItemId = itemId;
            Debug.Log($"Selected item: {itemId}");
            UpdateSelectedItemData(itemId);
        }
        else
        {
            Debug.LogError($"Item with ID {itemId} not found in inventory.");
        }
    }

    private void UpdateSelectedItemData(string itemId)
    {
        if (inventoryItems.TryGetValue(itemId, out InventoryItemData itemData))
        {
            SaveItemDataToUserData(itemId, itemData.ACCDescription, itemData.ImageUrl);
        }
        else
        {
            Debug.LogError($"Selected item with ID {itemId} not found in inventory items.");
        }
    }

    private void SaveItemDataToUserData(string itemId, string accDescription, string imageUrl)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "SelectedItemId", itemId },
                { "ACC", accDescription },
                { "ImageUrl", imageUrl }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => { Debug.Log($"Successfully updated item data for item {itemId} in User Data"); },
            error => { Debug.LogError($"Failed to update item data: {error.ErrorMessage}"); }
        );
    }

    private string GetItemName(PlayFab.EconomyModels.CatalogItem item)
    {
        if (item?.Title != null && item.Title.ContainsKey("NEUTRAL"))
        {
            return item.Title["NEUTRAL"];
        }
        return "Unknown Item";
    }

    private string GetACCDescription(PlayFab.EconomyModels.CatalogItem item)
    {
        if (item?.DisplayProperties != null)
        {
            var displayProperties = JObject.Parse(item.DisplayProperties.ToString());
            if (displayProperties.ContainsKey("ACC"))
            {
                return displayProperties["ACC"].ToString();
            }
        }
        return "ACC 설명이 없습니다.";
    }

    private string GetItemImageUrl(PlayFab.EconomyModels.CatalogItem item)
    {
        if (item?.Images != null && item.Images.Count > 0)
        {
            return item.Images[0].Url;
        }
        return "";
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"PlayFab 오류: {error.ErrorMessage}");
    }

    public void LoadSelectedItemData(Action<string, string, string> onDataLoaded)
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data.TryGetValue("SelectedItemId", out var selectedItemId) &&
                result.Data.TryGetValue("ACC", out var accDescription) &&
                result.Data.TryGetValue("ImageUrl", out var imageUrl))
            {
                onDataLoaded(selectedItemId.Value, accDescription.Value, imageUrl.Value);
            }
            else
            {
                Debug.LogWarning("선택된 아이템 데이터를 찾을 수 없습니다.");
                onDataLoaded(null, null, null);
            }
        }, error =>
        {
            Debug.LogError($"사용자 데이터를 불러오는 데 실패했습니다: {error.ErrorMessage}");
            onDataLoaded(null, null, null);
        });
    }

    // Helper class to store inventory item data
    private class InventoryItemData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ACCDescription { get; set; }
        public string ImageUrl { get; set; }
    }
}