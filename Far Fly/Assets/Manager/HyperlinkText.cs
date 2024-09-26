using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HyperlinkText : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text m_TextComponent;

    private Dictionary<string, string> m_LinkDictionary = new Dictionary<string, string>();

    private void Awake()
    {
        if (m_TextComponent == null)
            m_TextComponent = GetComponent<TMP_Text>();
    }

    public void AddLink(string linkID, string url)
    {
        m_LinkDictionary[linkID] = url;
    }

    public void SetText(string text)
    {
        m_TextComponent.text = text;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = m_TextComponent.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();
            
            if (m_LinkDictionary.TryGetValue(linkID, out string url))
            {
                Application.OpenURL(url);
            }
        }
    }
}