using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public Image itemImage;

    public void SetItemInfo(string title, string price, string imageUrl)
    {
        titleText.text = title;
        priceText.text = price;
        // Image will be set separately via SetItemImage
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
}