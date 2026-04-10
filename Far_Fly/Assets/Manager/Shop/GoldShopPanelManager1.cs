using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldShopPanelManager : MonoBehaviour
{

    public GameObject shopPanel; // Inspector에서 Shop Panel을 할당할 변수

    public void OpenShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            Debug.Log("Shop Panel이 열렸습니다.");
        }
        else
        {
            Debug.LogError("Shop Panel이 할당되지 않았습니다. Inspector에서 할당해주세요.");
        }
    }

    public void CloseShopPanel()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
            Debug.Log("Shop Panel이 닫혔습니다.");
        }
        else
        {
            Debug.LogError("Shop Panel이 할당되지 않았습니다. Inspector에서 할당해주세요.");
        }
    }

}
