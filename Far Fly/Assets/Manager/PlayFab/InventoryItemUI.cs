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
    private bool isSelected = false;

    private void Start()
    {
        selectButton.onClick.AddListener(ToggleSelection);
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

    private void ToggleSelection()
    {
        isSelected = !isSelected;
        checkImage.gameObject.SetActive(isSelected);
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void ResetSelection()
    {
        isSelected = false;
        checkImage.gameObject.SetActive(false);
    }

    public string GetItemId()
    {
        return itemId;
    }
}