using UnityEngine;
using PlayFab;
using PlayFab.EconomyModels;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

public class PlayFabInventoryManager : MonoBehaviour
{
    public GameObject inventoryItemPrefab;
    public Transform inventoryContent;
    public Button loadInventoryButton;
    private const string COLLECTION_ID = "inventory_ball";
    private Dictionary<string, InventoryItemUI> inventoryItems = new Dictionary<string, InventoryItemUI>();
    private string lastSelectedItemId;

    private void Start()
    {
        loadInventoryButton.onClick.AddListener(GetInventoryItems);
        HideExistingItems();
    }

    private void HideExistingItems()
    {
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
        foreach (var item in inventoryItems.Values)
        {
            Destroy(item.gameObject);
        }
        inventoryItems.Clear();
        lastSelectedItemId = null;
        HideExistingItems();
    }

    private void CreateInventoryItem(InventoryItem item)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, inventoryContent);
        InventoryItemUI itemUI = newItem.GetComponent<InventoryItemUI>();
        inventoryItems[item.Id] = itemUI;
        newItem.SetActive(true);

        itemUI.GetSelectButton().onClick.AddListener(() => SelectItem(item.Id));

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
            Debug.Log($"Setting item info - Name: {itemName}, ID: {item.Id}, ACC: {accDescription}");
            itemUI.SetItemInfo(itemName, item.Id, accDescription);
            if (result.Item?.Images != null && result.Item.Images.Count > 0)
            {
                StartCoroutine(LoadItemImage(result.Item.Images[0].Url, itemUI));
            }
        }, OnError);
    }

    private void SelectItem(string itemId)
    {
        if (lastSelectedItemId != null && inventoryItems.ContainsKey(lastSelectedItemId))
        {
            inventoryItems[lastSelectedItemId].SetSelected(false);
        }

        lastSelectedItemId = itemId;
        inventoryItems[itemId].SetSelected(true);

        Debug.Log($"Selected item: {itemId}");
    }

    private string GetItemName(CatalogItem item)
    {
        Debug.Log($"GetItemName called with item: {JsonUtility.ToJson(item)}");
        if (item?.Title != null && item.Title.ContainsKey("NEUTRAL"))
        {
            string title = item.Title["NEUTRAL"];
            Debug.Log($"Found Title: {title}");
            return title;
        }
        Debug.Log("Title not found or NEUTRAL key missing");
        return "Unknown Item";
    }

    private string GetACCDescription(CatalogItem item)
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