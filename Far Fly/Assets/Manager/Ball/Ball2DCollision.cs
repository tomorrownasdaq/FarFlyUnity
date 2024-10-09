using UnityEngine;
using MoreMountains.Feedbacks;

public class Ball2DCollision : MonoBehaviour
{
    public MMF_Player mmfPlayer;
    public GameObject collisionEffectPrefab; // 충돌 시 생성할 이펙트 프리팹
    public float minVelocityForEffect = 10000f; // 이펙트 생성을 위한 최소 속도
    public float minVelocityForFeedback = 10f; // Feedback 활성화를 위한 최소 속도

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (rb.velocity.magnitude >= minVelocityForFeedback && mmfPlayer == null)
        {
            mmfPlayer = GetComponent<MMF_Player>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb.velocity.magnitude >= minVelocityForEffect)
        {
            if (mmfPlayer != null)
            {
                mmfPlayer.PlayFeedbacks();
            }

            if (collisionEffectPrefab != null)
            {
                Vector2 collisionPoint = collision.GetContact(0).point;
                Instantiate(collisionEffectPrefab, collisionPoint, Quaternion.identity);
            }
        }
    }
}