using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class BallMessage : MonoBehaviour
{
    public TMP_Text distanceText; // Inspector에서 연결할 Text 컴포넌트
    private float initialPositionX;
    private Camera mainCamera;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPositionX = transform.position.x;
        mainCamera = Camera.main;

        // UI 텍스트 초기화 (Canvas 자식으로 생성하고 Inspector에서 연결)
        distanceText.gameObject.SetActive(false); // 처음에는 비활성화

    }

    void Update()
    {
        // ... (기존 코드)

        if (Mathf.Abs(rb.velocity.x) < 0.1f && Mathf.Abs(rb.velocity.y) < 0.1f && transform.position.x>2 )
        {
            // 이동 거리 계산 및 텍스트 업데이트
            float traveledDistance = transform.position.x - initialPositionX;
            distanceText.text = "Distance : " + traveledDistance.ToString("F1") + "m";

            // UI 텍스트 활성화 및 위치 조절
            distanceText.gameObject.SetActive(true);
            RectTransform rt = distanceText.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero; // 화면 중앙에 배치
            rb.velocity = new Vector2(0, 0); // Stop the ball
        }
        else
        {
            // 공이 움직일 때는 UI 텍스트 비활성화
            distanceText.gameObject.SetActive(false);
        }
    }
}