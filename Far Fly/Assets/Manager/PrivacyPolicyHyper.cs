using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PrivacyPolicyHyper : MonoBehaviour
{
    [System.Serializable]
    public class HyperlinkData
    {
        public HyperlinkText hyperlinkText; // Inspector에서 할당
        public string linkId;
        public string linkText;
        public string url;
    }

    public List<HyperlinkData> hyperlinkDataList = new List<HyperlinkData>();

    void Start()
    {
        foreach (var data in hyperlinkDataList)
        {
            if (data.hyperlinkText == null)
            {
                Debug.LogError($"HyperlinkText component for {data.linkId} is not assigned in the inspector!");
                continue;
            }

            // 링크 추가
            data.hyperlinkText.AddLink(data.linkId, data.url);

            // 링크가 포함된 텍스트 설정 (파란색과 밑줄 추가)
            string fullText = $"I agree to the <link=\"{data.linkId}\"><color=#0000FF><u>{data.linkText}</u></color></link>.";
            data.hyperlinkText.SetText(fullText);

            Debug.Log($"Hyperlink text set for {data.linkId}: {data.hyperlinkText.GetComponent<TMP_Text>().text}");
        }
    }
}