using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EconomyModels;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class StageInventory : MonoBehaviour
{
    private const string COLLECTION_ID = "inventory_ball";
    private Dictionary<string, InventoryItem> inventoryItems = new Dictionary<string, InventoryItem>();
    private string lastSelectedItemId;

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
        inventoryItems[item.Id] = item;

        var request = new GetItemRequest
        {
            Id = item.Id,
            AlternateId = null
        };
        PlayFabEconomyAPI.GetItem(request, result =>
        {
            Debug.Log($"GetItem response for ID {item.Id}: {JsonUtility.ToJson(result.Item)}");
            string itemName = GetItemName(result.Item);
            string accDescription = GetACCDescription(result.Item);
            string imageUrl = GetItemImageUrl(result.Item);
            Debug.Log($"Item info - Name: {itemName}, ID: {item.Id}, ACC: {accDescription}, ImageURL: {imageUrl}");
        }, OnError);
    }

    public void SelectItem(string itemId)
    {
        lastSelectedItemId = itemId;
        Debug.Log($"Selected item: {itemId}");
        UpdateSelectedItemData(itemId);
    }

    private void UpdateSelectedItemData(string itemId)
    {
        if (inventoryItems.ContainsKey(itemId))
        {
            var request = new PlayFab.EconomyModels.GetItemRequest
            {
                Id = itemId,
                AlternateId = null
            };

            PlayFabEconomyAPI.GetItem(request, result =>
            {
                string accDescription = GetACCDescription(result.Item);
                string imageUrl = GetItemImageUrl(result.Item);
                SaveItemDataToUserData(itemId, accDescription, imageUrl);
            }, OnError);
        }
        else
        {
            Debug.LogError($"Selected item with ID {itemId} not found in inventory items.");
        }
    }

    private void SaveItemDataToUserData(string itemId, string accDescription, string imageUrl)
    {
        var request = new PlayFab.ClientModels.UpdateUserDataRequest
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
            string title = item.Title["NEUTRAL"];
            Debug.Log($"Found Title: {title}");
            return title;
        }
        Debug.Log("Title not found or NEUTRAL key missing");
        return "Unknown Item";
    }

    private string GetACCDescription(PlayFab.EconomyModels.CatalogItem item)
    {
        if (item?.DisplayProperties != null)
        {
            var displayProperties = JObject.Parse(item.DisplayProperties.ToString());
            Debug.Log($"DisplayProperties: {displayProperties}");
            if (displayProperties.ContainsKey("ACC"))
            {
                string accDescription = displayProperties["ACC"].ToString();
                Debug.Log($"Found ACC Description: {accDescription}");
                return accDescription;
            }
            else
            {
                Debug.Log("ACC key not found in DisplayProperties");
            }
        }
        else
        {
            Debug.Log("Item or DisplayProperties is null");
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

    // 선택된 아이템 데이터를 불러오는 메서드 (다른 스크립트에서 사용 가능)
    public void LoadSelectedItemData(System.Action<string, string, string> onDataLoaded)
    {
        var request = new PlayFab.ClientModels.GetUserDataRequest();
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
}