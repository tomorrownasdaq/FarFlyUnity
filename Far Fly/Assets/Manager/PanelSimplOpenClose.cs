using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;
    public Button openButton;
    public Button closeButton;

    private void Start()
    {
        // 버튼에 클릭 이벤트 리스너 추가
        openButton.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);

        // 초기 상태 설정
        panel.SetActive(false);
    }

    private void OpenPanel()
    {
        panel.SetActive(true);
    }

    private void ClosePanel()
    {
        panel.SetActive(false);
    }
}