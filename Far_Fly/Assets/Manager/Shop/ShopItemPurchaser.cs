using UnityEngine;
using PlayFab;
using System.Collections;
using PlayFab.ClientModels;
using PlayFab.EconomyModels;
using System;

public class ShopItemPurchaser : MonoBehaviour
{
    [SerializeField] private GameObject buySuccessPanel;
    private int price;
    private string itemId;
    private string playerId;
    private string titlePlayerAccountId;
    private const string GOLD_CURRENCY_ID = "GL";
    private ShopItemUI uiComponent;

    private void Awake()
    {
        Debug.Log($"ShopItemPurchaser Awake called for {gameObject.name}");
    }

    private void Start()
    {
        Debug.Log($"ShopItemPurchaser Start called for {gameObject.name}");
        InitializeUIComponent();
        GetPlayerId();
    }

    private void OnEnable()
    {
        Debug.Log($"ShopItemPurchaser OnEnable called for {gameObject.name}");
        GetCurrencyBalances();
    }

    private void InitializeUIComponent()
    {
        uiComponent = GetComponent<ShopItemUI>();
        if (uiComponent == null)
        {
            Debug.LogError($"ShopItemUI component not found on {gameObject.name}!");
        }
        else
        {
            Debug.Log($"ShopItemUI component found on {gameObject.name}");
        }
    }

    public void SetItemInfo(string itemPrice, string id)
    {
        Debug.Log($"SetItemInfo called for {gameObject.name}: Price={itemPrice}, ID={id}");
        itemId = id;
        if (int.TryParse(itemPrice, out int parsedPrice))
        {
            price = parsedPrice;
        }
        else
        {
            Debug.LogWarning($"Failed to parse price: {itemPrice} for {gameObject.name}");
            price = 0;
        }
    }

    private void GetPlayerId()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticPlayer.PlayFabId))
        {
            Debug.LogError($"PlayFabId is null or empty for {gameObject.name}. Make sure the player is logged in.");
            return;
        }
        playerId = PlayFabSettings.staticPlayer.PlayFabId;
        Debug.Log($"Retrieved Master Player ID for {gameObject.name}: {playerId}");

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
                    Debug.Log($"Retrieved Title Player Account ID for {gameObject.name}: {titlePlayerAccountId}");
                }
                else
                {
                    Debug.LogError($"Failed to retrieve Title Player Account ID for {gameObject.name}.");
                }
            },
            error => {
                Debug.LogError($"Error getting player info for {gameObject.name}: {error.ErrorMessage}");
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
            Debug.LogError($"Title Player Account ID is not set for {gameObject.name}. Cannot purchase item.");
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
                UpdateUIGoldText(newBalance);
                Debug.Log($"골드 차감 성공 for {gameObject.name}. 새로운 잔액: {newBalance}");

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
                    CollectionId = "inventory_ball"
                };

                PlayFabEconomyAPI.AddInventoryItems(addItemRequest,
                    addItemResult => {
                        Debug.Log($"아이템 {itemId}를 플레이어 {titlePlayerAccountId}의 인벤토리에 추가했습니다. ({gameObject.name})");
                        ShowBuySuccessPanel();
                    },
                    addItemError => {
                        Debug.LogError($"아이템을 인벤토리에 추가하는 데 실패했습니다 for {gameObject.name}: {addItemError.ErrorMessage}");
                    }
                );
            },
            error => {
                Debug.LogError($"아이템 구매 실패 for {gameObject.name}: ItemId: {itemId}, Amount: {price}, Error: {error.ErrorMessage}");
            }
        );
    }

    private void ShowBuySuccessPanel()
    {
        if (buySuccessPanel != null)
        {
            buySuccessPanel.SetActive(true);
            StartCoroutine(HideBuySuccessPanelAfterDelay());
        }
        else
        {
            Debug.LogWarning("BuySuccess Panel is not assigned in the inspector.");
        }
    }

    private IEnumerator HideBuySuccessPanelAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        buySuccessPanel.SetActive(false);
    }


    public void GetCurrencyBalances()
    {
        var request = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(request,
            result => {
                int gold = result.VirtualCurrency.ContainsKey(GOLD_CURRENCY_ID) ? result.VirtualCurrency[GOLD_CURRENCY_ID] : 0;
                UpdateUIGoldText(gold);
                Debug.Log($"PlayFab에서 골드 잔액을 가져왔습니다 for {gameObject.name}: {gold}");
            },
            error => {
                Debug.LogError($"PlayFab에서 화폐 잔액을 가져오는 데 실패했습니다 for {gameObject.name}: {error.ErrorMessage}");
            }
        );
    }

    private void UpdateUIGoldText(int goldAmount)
    {
        if (uiComponent != null)
        {
            uiComponent.UpdateGoldText(goldAmount);
        }
        else
        {
            Debug.LogWarning($"ShopItemUI component is null when trying to update gold text for {gameObject.name}");
            InitializeUIComponent();
        }
    }
}