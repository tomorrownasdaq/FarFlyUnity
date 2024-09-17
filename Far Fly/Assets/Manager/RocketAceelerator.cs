using UnityEngine;
using UnityEngine.UI;

public class RocketAccelerator : MonoBehaviour
{
    public Button accelerateButton;
    public float accelerationForceUp = 10f;
    public float accelerationForceRight = 5f;
    public float accelerationDuration = 3f;
    public int maxClickCount = 3;
    private Rigidbody2D rocketRigidbody;
    private bool isAccelerating = false;
    private float accelerationTimer = 0f;
    private int clickCount = 0;
    private float lastAccelerationTime = 0f;
    public float accelerationCooldown = 0.5f; // 연속 가속 방지를 위한 쿨다운

    void Start()
    {
        rocketRigidbody = GetComponent<Rigidbody2D>();

        // 버튼을 화면 하단에 배치
        accelerateButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        accelerateButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        accelerateButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
        accelerateButton.onClick.AddListener(TryStartAcceleration);

        UpdateButtonInteractable();
    }

    void Update()
    {
        if (isAccelerating)
        {
            accelerationTimer += Time.deltaTime;
            if (accelerationTimer < accelerationDuration)
            {
                // 로켓에 위쪽과 오른쪽 방향으로 힘을 가함
                Vector2 force = new Vector2(accelerationForceRight, accelerationForceUp);
                rocketRigidbody.AddForce(force);
            }
            else
            {
                // 가속 시간이 끝나면 가속 종료
                isAccelerating = false;
                accelerationTimer = 0f;
            }
        }

        // 터치 입력 감지
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                TryStartAcceleration();
            }
        }

        // 마우스 클릭 감지 (에디터 및 데스크톱 플랫폼용)
        if (Input.GetMouseButtonDown(0))
        {
            TryStartAcceleration();
        }

        UpdateButtonInteractable();
    }

    void TryStartAcceleration()
    {
        if (clickCount < maxClickCount && transform.position.x > 2f && Time.time - lastAccelerationTime > accelerationCooldown)
        {
            isAccelerating = true;
            accelerationTimer = 0f;
            clickCount++;
            lastAccelerationTime = Time.time;
            Debug.Log($"Acceleration activated. Clicks remaining: {maxClickCount - clickCount}");
        }
    }

    void UpdateButtonInteractable()
    {
        accelerateButton.interactable = (clickCount < maxClickCount) && (transform.position.x > 2f) && (Time.time - lastAccelerationTime > accelerationCooldown);
    }
}