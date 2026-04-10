using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image checkImage;
    private string itemId;
    private string accDescription;

    private void Start()
    {
        checkImage.gameObject.SetActive(false);
    }

    public void SetItemInfo(string itemName, string id, string description)
    {
        itemNameText.text = itemName;
        itemId = id;
        accDescription = description;
        UpdateDescriptionUI();
        Debug.Log($"SetItemInfo: Name={itemName}, ID={id}, Description={description}");
    }

    public void SetItemImage(Texture2D texture)
    {
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            itemImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("Received null texture for item image");
        }
    }

    public void SetSelected(bool selected)
    {
        checkImage.gameObject.SetActive(selected);
    }

    private void UpdateDescriptionUI()
    {
        if (descriptionText != null)
        {
            descriptionText.text = accDescription;
        }
        else
        {
            Debug.LogWarning("Description Text component is not set.");
        }
    }

    public string GetItemId()
    {
        return itemId;
    }

    public string GetACCDescription()
    {
        return accDescription;
    }

    public string GetItemName()
    {
        return itemNameText.text;
    }

    public Button GetSelectButton()
    {
        return selectButton;
    }
}