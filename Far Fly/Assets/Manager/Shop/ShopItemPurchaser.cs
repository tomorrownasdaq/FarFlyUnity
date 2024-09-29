using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EconomyModels;
using System;

public class ShopItemPurchaser : MonoBehaviour
{
    private int price;
    private string itemId;
    private string playerId;
    private string titlePlayerAccountId;
    private const string GOLD_CURRENCY_ID = "GL";
    private ShopItemUI uiComponent;

    private void Start()
    {
        GetCurrencyBalances();
        uiComponent = GetComponent<ShopItemUI>();
        if (uiComponent == null)
        {
            Debug.LogError("ShopItemUI component not found!");
            return;
        }
        GetPlayerId();
    }

    public void SetItemInfo(string itemPrice, string id)
    {
        itemId = id;
        if (int.TryParse(itemPrice, out int parsedPrice))
        {
            price = parsedPrice;
        }
        else
        {
            Debug.LogWarning($"Failed to parse price: {itemPrice}");
            price = 0;
        }
    }

    private void GetPlayerId()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticPlayer.PlayFabId))
        {
            Debug.LogError("PlayFabId is null or empty. Make sure the player is logged in.");
            return;
        }
        playerId = PlayFabSettings.staticPlayer.PlayFabId;
        Debug.Log($"Retrieved Master Player ID: {playerId}");

        // Get Title Player Account ID
        var request = new GetPlayerCombinedInfoRequest
        {
            PlayFabId = playerId,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserAccountInfo = true
            }
        };

        PlayFabClientAPI.GetPlayerCombinedInfo(request,
            result => {
                if (result.InfoResultPayload != null &&
                    result.InfoResultPayload.AccountInfo != null &&
                    result.InfoResultPayload.AccountInfo.TitleInfo != null &&
                    result.InfoResultPayload.AccountInfo.TitleInfo.TitlePlayerAccount != null)
                {
                    titlePlayerAccountId = result.InfoResultPayload.AccountInfo.TitleInfo.TitlePlayerAccount.Id;
                    Debug.Log($"Retrieved Title Player Account ID: {titlePlayerAccountId}");
                }
                else
                {
                    Debug.LogError("Failed to retrieve Title Player Account ID.");
                }
            },
            error => {
                Debug.LogError($"Error getting player info: {error.ErrorMessage}");
            }
        );
    }

    public bool CanAffordItem(int currentGold)
    {
        return currentGold >= price;
    }

    public void PurchaseItem()
    {
        if (string.IsNullOrEmpty(titlePlayerAccountId))
        {
            Debug.LogError("Title Player Account ID is not set. Cannot purchase item.");
            return;
        }

        var subtractCurrencyRequest = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = GOLD_CURRENCY_ID,
            Amount = price
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(subtractCurrencyRequest,
            subtractResult => {
                int newBalance = subtractResult.Balance;
                uiComponent.UpdateGoldText(newBalance);
                Debug.Log($"골드 차감 성공. 새로운 잔액: {newBalance}");

                var addItemRequest = new AddInventoryItemsRequest
                {
                    Entity = new PlayFab.EconomyModels.EntityKey
                    {
                        Id = titlePlayerAccountId,
                        Type = "title_player_account"
                    },
                    Item = new PlayFab.EconomyModels.InventoryItemReference
                    {
                        Id = itemId,
                    },
                    Amount = 1,
                    CollectionId = "inventory"
                };

                PlayFabEconomyAPI.AddInventoryItems(addItemRequest,
                    addItemResult => {
                        Debug.Log($"아이템 {itemId}를 플레이어 {titlePlayerAccountId}의 인벤토리에 추가했습니다.");
                    },
                    addItemError => {
                        Debug.LogError($"아이템을 인벤토리에 추가하는 데 실패했습니다: {addItemError.ErrorMessage}");
                    }
                );
            },
            error => {
                Debug.LogError($"아이템 구매 실패: ItemId: {itemId}, Amount: {price}, Error: {error.ErrorMessage}");
            }
        );
    }

    public void GetCurrencyBalances()
    {
        var request = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(request,
            result => {
                int gold = result.VirtualCurrency.ContainsKey(GOLD_CURRENCY_ID) ? result.VirtualCurrency[GOLD_CURRENCY_ID] : 0;
                uiComponent.UpdateGoldText(gold);
                Debug.Log($"PlayFab에서 골드 잔액을 가져왔습니다: {gold}");
            },
            error => {
                Debug.LogError($"PlayFab에서 화폐 잔액을 가져오는 데 실패했습니다: {error.ErrorMessage}");
            }
        );
    }
}