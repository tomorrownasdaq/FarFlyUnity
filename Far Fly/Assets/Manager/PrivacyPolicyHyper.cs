using UnityEngine;
using TMPro;

public class PrivacyPolicyHyper : MonoBehaviour
{
    public HyperlinkText hyperlinkText; // Inspector에서 할당

    void Start()
    {
        if (hyperlinkText == null)
        {
            Debug.LogError("HyperlinkText component is not assigned in the inspector!");
            return;
        }

        // 링크 추가
        hyperlinkText.AddLink("link1", "https://gta7890.mycafe24.com/%EA%B3%B5%EC%A7%80%EA%B2%8C%EC%8B%9C%ED%8C%90/?vid=1");

        // 링크가 포함된 텍스트 설정 (파란색과 밑줄 추가)
        hyperlinkText.SetText("I agree the <link=\"link1\"><color=#0000FF><u>Privacy Policy</u></color></link>.");

        Debug.Log("Privacy policy text set: " + hyperlinkText.GetComponent<TMP_Text>().text);
    }
}