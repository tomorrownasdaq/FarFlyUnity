using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class RocketItemButtonAccelerator : MonoBehaviour
{
    public GameObject ballObject;
    public Button rocketItemButton;
    public TextMeshProUGUI itemCountText;
    public float activationX = 10f;
    public float cooldownDuration = 5f;

    public BallSpeedManager ballSpeedManager;

    [System.Serializable]
    public class EnhancementValue
    {
        public float baseValue;
        public float growthRate;
        public int currentLevel;
        public float additionalValue; // 새로 추가된 필드
    }

    [Header("Enhancement Data")]
    public EnhancementValue maxUsageCount;
    public EnhancementValue forceX;
    public EnhancementValue forceY;
    public EnhancementValue duration;

    private Rigidbody2D ballRigidbody;
    private bool isAccelerating = false;
    private float accelerationTimer = 0f;
    private float cooldownTimer = 0f;
    private int remainingUses; // 로컬에서 관리할 남은 사용 횟수

    void Start()
    {
        InitializeComponents();
        LoadEnhancementData();
    }

    void InitializeComponents()
    {
        ballRigidbody = ballObject?.GetComponent<Rigidbody2D>();
        ballSpeedManager = ballSpeedManager ?? FindObjectOfType<BallSpeedManager>();

        if (ballRigidbody == null || ballSpeedManager == null || rocketItemButton == null)
        {
            Debug.LogError("Essential components are missing!");
            return;
        }

        rocketItemButton.onClick.AddListener(TryActivateRocketItem);
    }

    void LoadEnhancementData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnPlayFabError);
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null)
        {
            if (result.Data.ContainsKey("Enhancement_0"))
                maxUsageCount.currentLevel = int.Parse(result.Data["Enhancement_0"].Value);
            if (result.Data.ContainsKey("Enhancement_1"))
                forceX.currentLevel = int.Parse(result.Data["Enhancement_1"].Value);
            if (result.Data.ContainsKey("Enhancement_2"))
                forceY.currentLevel = int.Parse(result.Data["Enhancement_2"].Value);
            if (result.Data.ContainsKey("Enhancement_3"))
                duration.currentLevel = int.Parse(result.Data["Enhancement_3"].Value);
        }
        remainingUses = (int)CalculateEnhancedValue(maxUsageCount); // 초기 남은 사용 횟수 설정
        UpdateUI();
    }

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError($"PlayFab error: {error.ErrorMessage}");
    }

    void Update()
    {
        HandleAcceleration();
        HandleCooldown();
        UpdateUI();
    }

    void FixedUpdate()
    {
        if (isAccelerating)
        {
            Vector2 force = new Vector2(
                CalculateEnhancedValue(forceX),
                CalculateEnhancedValue(forceY)
            );
            ballRigidbody.AddForce(force);
        }
    }

    void TryActivateRocketItem()
    {
        bool isGameOver = IsGameOver();
        bool isBallPastActivationPoint = ballObject.transform.position.x > activationX;
        bool hasRemainingUses = remainingUses > 0;
        bool isCooldownActive = cooldownTimer > 0;

        if (!isGameOver && !isAccelerating && isBallPastActivationPoint && hasRemainingUses && !isCooldownActive)
        {
            StartAcceleration();
        }
        else
        {
            LogActivationError(isGameOver, hasRemainingUses, isBallPastActivationPoint, isCooldownActive);
        }
    }

    void StartAcceleration()
    {
        isAccelerating = true;
        accelerationTimer = 0f;
        remainingUses--; // 로컬 카운트만 감소
        Debug.Log($"Rocket item activated! Ball is accelerating. Remaining uses: {remainingUses}");
        UpdateUI();
    }

    void StopAcceleration()
    {
        isAccelerating = false;
        accelerationTimer = 0f;
        cooldownTimer = cooldownDuration;
        Debug.Log("Acceleration finished.");
        UpdateUI();
    }

    void HandleAcceleration()
    {
        if (isAccelerating)
        {
            accelerationTimer += Time.deltaTime;
            if (accelerationTimer >= CalculateEnhancedValue(duration))
            {
                StopAcceleration();
            }
        }
    }

    void HandleCooldown()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
                Debug.Log("Cooldown finished. Rocket item is ready to use.");
            }
        }
    }

    void UpdateUI()
    {
        bool canUse = !IsGameOver() && !isAccelerating && ballObject.transform.position.x > activationX
                      && remainingUses > 0 && cooldownTimer <= 0;

        rocketItemButton.interactable = canUse;

        if (itemCountText != null)
        {
            if (cooldownTimer > 0)
            {
                itemCountText.text = $"{remainingUses} \n ({cooldownTimer:F1}s)";
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

    void LogActivationError(bool isGameOver, bool hasRemainingUses, bool isBallPastActivationPoint, bool isCooldownActive)
    {
        if (isGameOver)
            Debug.Log("Cannot activate rocket item. Game is over.");
        else if (!hasRemainingUses)
            Debug.Log("No more uses left for the rocket item.");
        else if (!isBallPastActivationPoint)
            Debug.Log("Cannot activate rocket item. Ball hasn't reached the activation point yet.");
        else if (isCooldownActive)
            Debug.Log($"Cannot activate rocket item. Cooldown time remaining: {cooldownTimer:F1} seconds.");
    }

    private float CalculateEnhancedValue(EnhancementValue enhancement)
    {
        return enhancement.baseValue + (enhancement.growthRate * enhancement.currentLevel) + enhancement.additionalValue;
    }
}