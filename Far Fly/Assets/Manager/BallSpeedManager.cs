using UnityEngine;

public class BallSpeedManager : MonoBehaviour
{
    public Rigidbody2D ballRigidbody;
    public float stopThreshold = 0.1f;
    
    private bool gameOver = false;
    private Vector2 initialPosition;
    private float distanceTraveled = 0f;

    private void Start()
    {
        if (ballRigidbody == null)
        {
            ballRigidbody = GetComponent<Rigidbody2D>();
        }
        initialPosition = ballRigidbody.position;
    }

    private void Update()
    {
        if (!gameOver)
        {
            // 현재 속도 계산
            float currentSpeed = ballRigidbody.velocity.magnitude;

            // 이동 거리 계산
            distanceTraveled = Vector2.Distance(initialPosition, ballRigidbody.position);

            // 정지 상태 확인
            if (currentSpeed < stopThreshold && transform.position.x > 2)
            {
                gameOver = true;
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        // GameOverManager를 통해 게임 오버 화면 표시
        GameOverManager.Instance.ShowGameOver(distanceTraveled);
    }

    public void ResetBall()
    {
        gameOver = false;
        ballRigidbody.velocity = Vector2.zero;
        ballRigidbody.position = initialPosition;
        distanceTraveled = 0f;
    }
}