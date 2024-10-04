using UnityEngine;

public class BallSpeedManager : MonoBehaviour
{
    public Rigidbody2D ballRigidbody;
    public float stopThreshold = 10f;
    public float gameOverXThreshold = 10f;

    public bool gameOver = false;
    private Vector2 initialPosition;
    private float distanceTraveled = 0f;

    [SerializeField] private float debugUpdateInterval = 1f;
    private float debugTimer = 0f;

    private void Start()
    {
        if (ballRigidbody == null)
        {
            ballRigidbody = GetComponent<Rigidbody2D>();
            if (ballRigidbody == null)
            {
                Debug.LogError("Rigidbody2D component not found on the game object.");
                enabled = false;
                return;
            }
        }
        initialPosition = ballRigidbody.position;
        Debug.Log($"Initial position set to: {initialPosition}");
    }

    private void Update()
    {
        if (!gameOver && ballRigidbody != null)
        {
            // 현재 속도 계산
            float currentSpeed = ballRigidbody.velocity.magnitude;

            // 이동 거리 계산
            distanceTraveled = Vector2.Distance(initialPosition, ballRigidbody.position);

            // 디버그 정보 출력
            debugTimer += Time.deltaTime;
            if (debugTimer >= debugUpdateInterval)
            {
                Debug.Log($"Current speed: {currentSpeed}, Position: {ballRigidbody.position}, Distance: {distanceTraveled}");
                debugTimer = 0f;
            }

            // 정지 상태 확인
            if (currentSpeed < stopThreshold && ballRigidbody.position.x > gameOverXThreshold)
            {
                Debug.Log($"Game over condition met! Speed: {currentSpeed}, X position: {ballRigidbody.position.x}");
                gameOver = true;
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        Debug.Log($"GameOver called. Distance traveled: {distanceTraveled}");
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver(distanceTraveled);
        }
        else
        {
            Debug.LogError("GameOverManager instance is null. Make sure it's properly initialized.");
        }
    }

    public void ResetBall()
    {
        if (ballRigidbody != null)
        {
            gameOver = false;
            ballRigidbody.velocity = Vector2.zero;
            ballRigidbody.position = initialPosition;
            distanceTraveled = 0f;
            Debug.Log("Ball reset to initial position.");
        }
        else
        {
            Debug.LogError("Cannot reset ball: Rigidbody2D is null.");
        }
    }
}