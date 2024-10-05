using UnityEngine;
using UnityEngine.UI;

public class RocketItemButtonAccelerator : MonoBehaviour
{
    public GameObject ballObject;
    public Button rocketItemButton;
    public Text itemCountText;
    public float accelerationForceX = 10f;
    public float accelerationForceY = 5f;
    public float accelerationDuration = 2f;
    public float activationX = 10f;
    public int maxUsageCount = 3;
    public float cooldownDuration = 5f; // 쿨타임 지속 시간

    public BallSpeedManager ballSpeedManager;

    private Rigidbody2D ballRigidbody;
    private bool isAccelerating = false;
    private float accelerationTimer = 0f;
    private int remainingUses;
    private float cooldownTimer = 0f; // 쿨타임 타이머

    void Start()
    {
        if (ballObject == null)
        {
            Debug.LogError("Ball object is not assigned!");
            return;
        }
        ballRigidbody = ballObject.GetComponent<Rigidbody2D>();
        if (ballRigidbody == null)
        {
            Debug.LogError("Rigidbody2D component not found on the ball object!");
            return;
        }
        if (rocketItemButton == null)
        {
            Debug.LogError("Rocket Item Button is not assigned!");
            return;
        }

        if (ballSpeedManager == null)
        {
            ballSpeedManager = FindObjectOfType<BallSpeedManager>();
            if (ballSpeedManager == null)
            {
                Debug.LogError("BallSpeedManager not found in the scene!");
            }
        }

        rocketItemButton.onClick.AddListener(TryActivateRocketItem);
        remainingUses = maxUsageCount;
        UpdateUI();
    }

    void Update()
    {
        if (isAccelerating)
        {
            accelerationTimer += Time.deltaTime;
            if (accelerationTimer >= accelerationDuration)
            {
                StopAcceleration();
            }
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
                Debug.Log("Cooldown finished. Rocket item is ready to use.");
            }
        }

        UpdateUI();
    }

    void FixedUpdate()
    {
        if (isAccelerating)
        {
            Vector2 force = new Vector2(accelerationForceX, accelerationForceY);
            ballRigidbody.AddForce(force);
        }
    }

    void TryActivateRocketItem()
    {
        bool isGameOver = IsGameOver();
        if (!isGameOver && !isAccelerating && ballObject.transform.position.x > activationX && remainingUses > 0 && cooldownTimer <= 0)
        {
            StartAcceleration();
            remainingUses--;
            cooldownTimer = cooldownDuration; // 쿨타임 시작
        }
        else if (isGameOver)
        {
            Debug.Log("Cannot activate rocket item. Game is over.");
        }
        else if (remainingUses <= 0)
        {
            Debug.Log("No more uses left for the rocket item.");
        }
        else if (ballObject.transform.position.x <= activationX)
        {
            Debug.Log("Cannot activate rocket item. Ball hasn't reached the activation point yet.");
        }
        else if (cooldownTimer > 0)
        {
            Debug.Log($"Cannot activate rocket item. Cooldown time remaining: {cooldownTimer:F1} seconds.");
        }
    }

    void StartAcceleration()
    {
        isAccelerating = true;
        accelerationTimer = 0f;
        Debug.Log($"Rocket item activated! Ball is accelerating. Remaining uses: {remainingUses}");
        UpdateUI();
    }

    void StopAcceleration()
    {
        isAccelerating = false;
        accelerationTimer = 0f;
        Debug.Log("Acceleration finished.");
        UpdateUI();
    }

    void UpdateUI()
    {
        bool isGameOver = IsGameOver();
        bool isBallPastActivationPoint = ballObject != null && ballObject.transform.position.x > activationX;
        bool hasRemainingUses = remainingUses > 0;
        bool isCooldownActive = cooldownTimer > 0;

        bool canUse = !isGameOver && !isAccelerating && isBallPastActivationPoint && hasRemainingUses && !isCooldownActive;

        if (rocketItemButton != null)
        {
            rocketItemButton.interactable = canUse;
        }

        if (itemCountText != null)
        {
            if (isCooldownActive)
            {
                itemCountText.text = $"{remainingUses} ({cooldownTimer:F1}s)";
            }
            else
            {
                itemCountText.text = $"{remainingUses}";
            }
        }
    }

    bool IsGameOver()
    {
        return ballSpeedManager != null && ballSpeedManager.gameOver;
    }

    void OnEnable()
    {
        InvokeRepeating("LogStatus", 0f, 1f);
    }

    void OnDisable()
    {
        CancelInvoke("LogStatus");
    }

    void LogStatus()
    {

    }
}