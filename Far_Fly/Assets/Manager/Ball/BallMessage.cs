using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro�� ����ϱ� ���� �߰�

public class BallMessage : MonoBehaviour
{
    public TMP_Text distanceText; // Inspector���� ������ Text ������Ʈ
    private float initialPositionX;
    private Camera mainCamera;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPositionX = transform.position.x;
        mainCamera = Camera.main;

        // UI �ؽ�Ʈ �ʱ�ȭ (Canvas �ڽ����� �����ϰ� Inspector���� ����)
        distanceText.gameObject.SetActive(false); // ó������ ��Ȱ��ȭ

    }

    void Update()
    {
        // ... (���� �ڵ�)

        if (Mathf.Abs(rb.linearVelocity.x) < 0.1f && Mathf.Abs(rb.linearVelocity.y) < 0.1f && transform.position.x>2 )
        {
            // �̵� �Ÿ� ��� �� �ؽ�Ʈ ������Ʈ
            float traveledDistance = transform.position.x - initialPositionX;
            distanceText.text = "Distance : " + traveledDistance.ToString("F1") + "m";

            // UI �ؽ�Ʈ Ȱ��ȭ �� ��ġ ����
            distanceText.gameObject.SetActive(true);
            RectTransform rt = distanceText.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero; // ȭ�� �߾ӿ� ��ġ
            rb.linearVelocity = new Vector2(0, 0); // Stop the ball
        }
        else
        {
            // ���� ������ ���� UI �ؽ�Ʈ ��Ȱ��ȭ
            distanceText.gameObject.SetActive(false);
        }
    }
}