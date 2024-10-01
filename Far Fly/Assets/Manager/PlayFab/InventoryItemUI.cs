using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image checkImage;
    private string itemId;

    private void Start()
    {
        checkImage.gameObject.SetActive(false);
    }

    public void SetItemInfo(string itemName, string id)
    {
        itemNameText.text = itemName;
        itemId = id;
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

    public string GetItemId()
    {
        return itemId;
    }

    public Button GetSelectButton()
    {
        return selectButton;
    }
}