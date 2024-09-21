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
    public float activationX = 100f;
    public int maxUsageCount = 3;
    public GameObject gameOverPanel; // Changed from gameOverCanvas to gameOverPanel

    private Rigidbody2D ballRigidbody;
    private bool isAccelerating = false;
    private float accelerationTimer = 0f;
    private int remainingUses;

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

        if (!isGameOver && !isAccelerating && ballObject.transform.position.x > activationX && remainingUses > 0)
        {
            StartAcceleration();
            remainingUses--;
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
        bool canUse = !isAccelerating && ballObject.transform.position.x > activationX && remainingUses > 0 && !isGameOver;
        rocketItemButton.interactable = canUse;
        if (itemCountText != null)
        {
            itemCountText.text = $"{remainingUses}";
        }
    }

    bool IsGameOver()
    {
        return gameOverPanel != null && gameOverPanel.activeSelf;
    }
}