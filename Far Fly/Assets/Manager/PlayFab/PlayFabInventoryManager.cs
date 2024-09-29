using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayFabInventoryManager : MonoBehaviour
{
    public GameObject inventoryItemPrefab;
    public Transform inventoryContent;
    

    private Dictionary<string, InventoryItemUI> inventoryItems = new Dictionary<string, InventoryItemUI>();

    private void Start()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            GetInventoryItems();
            
        }
        else
        {
            Debug.LogError("플레이어가 로그인되어 있지 않습니다.");
        }
    }

    private void GetInventoryItems()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetUserInventorySuccess, OnError);
    }

    private void OnGetUserInventorySuccess(GetUserInventoryResult result)
    {
        foreach (var item in result.Inventory)
        {
            CreateOrUpdateInventoryItem(item);
        }
    }

    private void CreateOrUpdateInventoryItem(ItemInstance item)
    {
        if (!inventoryItems.ContainsKey(item.ItemId))
        {
            GameObject newItem = Instantiate(inventoryItemPrefab, inventoryContent);
            InventoryItemUI itemUI = newItem.GetComponent<InventoryItemUI>();
            inventoryItems[item.ItemId] = itemUI;
        }

        InventoryItemUI currentItemUI = inventoryItems[item.ItemId];
        currentItemUI.SetItemInfo(item.DisplayName, item.ItemId);

        // 아이템 이미지 로드 (아이템 카탈로그에서 이미지 URL을 가져온다고 가정)
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), result =>
        {
            CatalogItem catalogItem = result.Catalog.Find(x => x.ItemId == item.ItemId);
            if (catalogItem != null && !string.IsNullOrEmpty(catalogItem.ItemImageUrl))
            {
                StartCoroutine(LoadItemImage(catalogItem.ItemImageUrl, currentItemUI));
            }
        }, OnError);
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