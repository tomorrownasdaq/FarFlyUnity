using UnityEngine;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;

    public void SetItemInfo(string title, string price)
    {
        Debug.Log($"SetItemInfo called - Title: {title}, Price: {price}");

        if (titleText != null)
        {
            titleText.text = title;
            Debug.Log($"Set title text to: {title}");
        }
        else
        {
            Debug.LogError("TitleText is null");
        }

        if (priceText != null)
        {
            priceText.text = price;
            Debug.Log($"Set price text to: {price}");
        }
        else
        {
            Debug.LogError("PriceText is null");
        }
    }
}